using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// Class the handle the object placement in the world
    /// </summary>
    public class ObjectPlacer : MonoBehaviour
    {

        /****************************************************
        * Editor Interface
        * *************************************************/

        [Header("Object Settings")]

        [Tooltip("Fill this only if you don't use ObjectSelector!")]
        [SerializeField] GameObject objectToPlace;

        [Tooltip("This material will be applied to the object when it is not placed!")]
        [SerializeField] Material ghostMaterial;

        //**********************************************************************************************
        [Header("Place Settings")]

        [Tooltip("Camera used to raycast and find place position, if empty the script will try to use the main camera")]
        [SerializeField] Camera cam;

        [Tooltip("Layers that this script will use to get hit points to place objects")]
        [SerializeField] LayerMask groundLayer;

        [SerializeField] float maxPlaceDistance = 10f;

        [SerializeField] bool placeInScreenCenter = false;

        //**********************************************************************************************
        [Header("Rotation Settings")]

        [Tooltip("Face the object to the player, THIS BLOCKS SNAP ROTATION!")]
        [SerializeField] bool faceMe = false;

        [SerializeField] float snapRotationAngle = 45;

        //**********************************************************************************************
        [Header("Input Settings")] 

        [Tooltip("Key to press to enable the builder mode. This is also used by ObjectSelector")]
        [SerializeField] KeyCode toggleKey = KeyCode.E;

        [Tooltip("Key to press to place a item in the scene")]
        [SerializeField] KeyCode placeKey = KeyCode.Mouse0;

        [Tooltip("Key to press rotate (forward) the object based on snapRotaionDeg")]
        [SerializeField] KeyCode positiveRotateKey = KeyCode.Mouse1;

        [Tooltip("Key to press rotate (backward) the object based on snapRotaionDeg")]
        [SerializeField] KeyCode negativeRotateKey = KeyCode.None;

        /****************************************************
        * Public variables & Classes
        * *************************************************/
        public enum PlaceMode { mousePos, screenCenter };

        public enum RotaionMode { snap, facePlacer };

        public KeyCode ToggleKey { get { return toggleKey; } }

        public bool IsActive { get { return isActive; } }

        /****************************************************
        * Events
        * *************************************************/

        public delegate void BuildEvent();

        public event BuildEvent OnGhostObjectCreation;
        public event BuildEvent OnGhostObjectDestroy;
        public event BuildEvent OnGhostObjectPlace;

        /****************************************************
        * Variables
        * *************************************************/

        bool isActive = true;

        bool canPlace = false;

        bool mouseIsNotOnUI = true;

        //object rotation
        float objectSnapCurrentRotaion = 0;

        //object pivot
        float pivotMargin;
        bool usingFakePivot = false;
        Vector3 pivotOffsetExtra;

        //old prefab state
        bool[] bodiesPrevState;
        UnityEngine.Rendering.ShadowCastingMode oldShadowState;

        //Mesh Type
        bool useCompleMesh = false;


        /****************************************************
        * Components & references
        * *************************************************/

        Transform ghostObjInstance;
        MeshRenderer ghostRenderer;

        Material[] oldMaterials;
        Transform myTransform;

        ComplexGhostCreator complexGhostCreator;

        /****************************************************
        * Init
        * *************************************************/

        private void Start()
        {
            if (cam == null)
                cam = Camera.main;
            myTransform = transform;

            if (ghostMaterial == null) Debug.LogWarning("Missing ghostMaterial, ignore this warning if you don't want to use it.");
            if (cam == null) Debug.LogError("Missing cam, please assign it!");
        }

        /****************************************************
        * Input
        * *************************************************/

        void Update()
        {
            //toggle contruction mode
            if (Input.GetKeyDown(toggleKey) && isActive)
            {
                Toggle();
            }

            //place the object
            if (canPlace)
            {
                if (Input.GetKeyDown(placeKey) && mouseIsNotOnUI)
                    PlaceGhostObject();

                if (!faceMe)
                {

                    if (Input.GetKeyDown(positiveRotateKey))
                        SnapRotation(+1); // positive rotation

                    if (Input.GetKeyDown(negativeRotateKey))
                        SnapRotation(-1); // negative rotation
                }
            }

        }


        /****************************************************
        * Ghost Obj Position update
        * *************************************************/

        private void FixedUpdate()
        {
            //update ghost object position
            if (canPlace && ghostObjInstance != null)
            {
                MoveGhostObject();
                //update rotation to face the placer
                if (faceMe)
                {
                    snapRotationAngle =  GetFaceToRotation(myTransform, ghostObjInstance);
                }
            }
        }

        /****************************************************
        * Activation
        * *************************************************/

        /// <summary>
        /// Toggle Object Placer
        /// </summary>
        public void Toggle()
        {
            canPlace = !canPlace;
            Toggle(canPlace);
        }

        /// <summary>
        /// Active/Disable Object Placer
        /// </summary>
        /// <param name="val">Is active</param>
        public void Toggle(bool val)
        {
            canPlace = val;
            if (canPlace) CreateGhostObject();
            else DestroyGhostObject();
        }


        /// <summary>
        /// Active the script input handler, use toggle to start placing objects
        /// </summary>
        /// <param name="val"></param>
        public void Enable(bool val)
        {
            isActive = val;
            Toggle(false);
        }

        /****************************************************
         * Ghost Object Creation & Movement & Place
         * *************************************************/

        /// <summary>
        /// Create ghost object
        /// </summary>
        void CreateGhostObject()
        {
            DestroyGhostObject();

            if (objectToPlace == null)
            {
                Debug.LogError("No prefab to instantiate! Aborting ghost creation.");
                return;
            }
            ghostObjInstance = Instantiate(objectToPlace, myTransform.position, Quaternion.identity).GetComponent<Transform>();

            //get the root renderer (first one is considered root)
            ghostRenderer = ghostObjInstance.GetComponentInChildren<MeshRenderer>();

            if (ghostRenderer == null)
            {
                Debug.LogError("Object: " + objectToPlace + " has no mesh renderers! Aborting ghost creation.");
                Destroy(ghostObjInstance);
                return;
            }

            EnableGhostMaterials(ghostRenderer);

            //replace materials with ghost
            EnableGhostShadows(false);
            if (useCompleMesh)
                complexGhostCreator.CreateComplexGhost(ghostObjInstance, ghostMaterial);

            //remove collisions and physics
            EnableObjectCollision(ghostObjInstance,false);
            EnableGhostObjRigidbodies(false);

            //reset old object rotation
            objectSnapCurrentRotaion = 0;

            //check where is the pivot, if it is not in the base create a fake one
            usingFakePivot = false;
            pivotMargin = ghostRenderer.bounds.extents.y * 2 / 3; //set the base pivot margin. its' height must be lower than obj center * 2/3

            bool objPivotIsBase = CheckIfObjectPivotIsCenter(ghostObjInstance,ghostRenderer, out pivotOffsetExtra);

            //create a fake pivot if the real one is not in base
            if (!objPivotIsBase)
            {
                ghostObjInstance = CreateBasePivot(ghostObjInstance, ghostRenderer,pivotOffsetExtra);
                usingFakePivot = true;
            }

            if (OnGhostObjectCreation != null)
            {
                OnGhostObjectCreation();
            }
        }


        /// <summary>
        /// Move the ghost object
        /// </summary>
        void MoveGhostObject()
        {
            RaycastHit hit;

            Ray r;
            //Create a ray for the raycast
            if (!placeInScreenCenter)
            {
                //use mouse pointer to place object
                r = cam.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                //use screen center to place object
                Transform camT = cam.transform;
                r = new Ray(camT.position, camT.forward);
            }

            //find hit position and move there the object
            if (Physics.Raycast(r, out hit, maxPlaceDistance, groundLayer))
            {
                //set object position to hit point
                Vector3 pos = hit.point;
                ghostObjInstance.position = pos;

                AlignGhostToSurface(ghostObjInstance,hit.normal);
            }
        }


        /// <summary>
        /// Place the object in the scene
        /// </summary>
        void PlaceGhostObject()
        {
            if (ghostObjInstance != null)
            {
                //reset old materials
                if (useCompleMesh) complexGhostCreator.RemoveComplexGhost();
                EnableGhostMaterials(false);

                EnableGhostObjRigidbodies(true); //reset rigidbody state
                EnableObjectCollision(ghostObjInstance, true); //reset collisions
                EnableGhostShadows(true); //reset old shadow state

                Debug.Log("Created: " + ghostObjInstance.name);

                if (usingFakePivot) // remove fake pivot if using one
                {
                    ghostObjInstance.GetComponent<PivotHelper>().DeletePivot();
                }               

                ghostObjInstance = null; //leave the object in the scene

                if (OnGhostObjectPlace != null)
                {
                    OnGhostObjectPlace();
                }

                CreateGhostObject(); //create a new ghost object
            }
            else Debug.LogError("Unable to spawn object, ghost reference is null!");
        }


        /// <summary>
        /// Remove ghost object from scene
        /// </summary>
        void DestroyGhostObject()
        {
            if (ghostObjInstance != null)
            {
                Destroy(ghostObjInstance.gameObject);
                if (complexGhostCreator != null) complexGhostCreator.ClearCache();

                if (OnGhostObjectDestroy != null)
                {
                    OnGhostObjectDestroy();
                }
            }
        }


        /****************************************************
        * Ghost Object Alignament & Rotation
        * *************************************************/

        /// <summary>
        /// Align ghost object to surface based on raycast hit normal
        /// </summary>
        /// <param name="itemToAlign">Item to align</param>
        /// <param name="hitNormal">Normal to use to align object</param>
        void AlignGhostToSurface(Transform itemToAlign,Vector3 hitNormal)
        {
            if (itemToAlign == null) return;

            itemToAlign.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(new Vector3(0, objectSnapCurrentRotaion, 0));

        }

        /// <summary>
        /// Check if the object pivot is in center or not.
        /// This function returns the pivot Offset (can be Vector3.zero)
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <param name="renderer">Renderer attached to the item</param>
        /// <param name="pivotOffset">Offset of the pivot to be in base</param>
        /// <returns></returns>
        bool CheckIfObjectPivotIsCenter(Transform item, MeshRenderer renderer, out Vector3 pivotOffset)
        {
            if (renderer == null)
            {
                Debug.LogError("No ghost renderer!");
                pivotOffset = Vector3.zero;
                return false;
            }

            Vector3 delta = item.position - renderer.bounds.center;
            if (delta.magnitude >= pivotMargin && delta.y < 0) //delta.y < 0 fix issues that not centerd pivots above the object center were taken as base pivots
            {
                pivotOffset = Vector3.zero;
                return true;
            }
            else
            {
                pivotOffset = delta; // save pivot delta to use to create a fake pivot
                return false;
            }
        }

        /// <summary>
        /// Create a pivot parent to better place the object
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <param name="renderer">Renderer of the item</param>
        /// <param name="pivotOffset">Offset of the pivot</param>
        /// <returns></returns>
        Transform CreateBasePivot(Transform item,MeshRenderer renderer, Vector3 pivotOffset)
        {
            if (item == null || renderer == null)
            {
                Debug.LogError("CreateBasePivot can't have null parameters!");
            }

            GameObject pivotG = new GameObject("Temp_Ghost_Pivot_Parent"); // create parent
            Transform pivotT = pivotG.transform;

            pivotG.AddComponent<PivotHelper>(); // add helper class to remove the pivot when object is spawned

            //get mesh center
            Vector3 meshCenter = renderer.bounds.extents;
            // apply pivot delta
            meshCenter.x = pivotOffset.x;
            meshCenter.z = pivotOffset.z;
            meshCenter.y += pivotOffset.y; 


            item.SetParent(pivotT); // set the current object as parent
            item.localPosition = meshCenter; // move the object and leave the parent object in the pivot position

            return pivotT;
        }


        /// <summary>
        /// Rotate ghost object to face the placer
        /// </summary>
        /// <param name="target">Object placer</param>
        /// <param name="other">Item to rotate</param>
        /// <returns></returns>
        float GetFaceToRotation(Transform target,Transform other)
        {
            if (objectToPlace == null || ghostRenderer)
                Debug.LogError("GetFaceToRotaion can't have null parameters");

            Vector3 dir = target.position - other.position;
            return Quaternion.LookRotation(dir.normalized).eulerAngles.y;
        }

        /// <summary>
        /// Snap rotation of the object
        /// </summary>
        /// <param name="mult">Rotation multiplier</param>
        void SnapRotation(int mult)
        {
            objectSnapCurrentRotaion += mult * snapRotationAngle;
        }

        /****************************************************
        * Ghost Object properties modifiers
        * *************************************************/

        /// <summary>
        /// Active/Reset shadow preset of renderer
        /// </summary>
        /// <param name="val">Active shadows</param>
        void EnableGhostShadows(bool val)
        {
            if (!val)
            {
                oldShadowState = ghostRenderer.shadowCastingMode;
                ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                ghostRenderer.shadowCastingMode = oldShadowState;
            }

        }

        /// <summary>
        /// Toggle ghost colliders
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <param name="val">Active colliders</param>
        void EnableObjectCollision(Transform item ,bool val)
        {
            if (item == null) return;

            var cols = item.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = val;
            }
        }


        /// <summary>
        /// Set all rigidbodies to be kinematic or reset them with previous state
        /// </summary>
        /// <param name="val">Active rigidbodies</param>
        void EnableGhostObjRigidbodies(bool val)
        {
            //get all bodies
            var bodies = ghostObjInstance.GetComponentsInChildren<Rigidbody>();

            if (val == false)
            {
                GetOldRigidBodyStateAndOvverride(val, bodies);
            }
            else
            {
                SetOldRigidbodyState(bodies);
            }

        }

        /// <summary>
        /// Save the old state for the body reset
        /// </summary>
        /// <param name="val">New kinematic value</param>
        /// <param name="bodies">List of rigidbodies to use</param>
        void GetOldRigidBodyStateAndOvverride(bool val, Rigidbody[] bodies)
        {
            bodiesPrevState = new bool[bodies.Length];

            for (int i = 0; i < bodies.Length; i++)
            {
                bodiesPrevState[i] = bodies[i].isKinematic;
                bodies[i].isKinematic = !val;
            }
        }

        /// <summary>
        /// Set the old rigidbodies state
        /// </summary>
        /// <param name="bodies"></param>
        void SetOldRigidbodyState(Rigidbody[] bodies)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].isKinematic = bodiesPrevState[i];
            }
        }


        /// <summary>
        /// Set all materials to ghost and store the old ones.
        /// If ghostMaterial is null, no operation is performed
        /// </summary>
        /// <param name="val">Use ghost material</param>
        void EnableGhostMaterials(bool val)
        {
            if (ghostRenderer == null) return;
            if (ghostMaterial == null) return; //don't perform operation if ghost material is not assigned

            if (val)
            {
                oldMaterials = ghostRenderer.materials;
                ghostRenderer.materials = CreateGhostMaterialArray(ghostRenderer.materials.Length);
            }
            else
            {
                ghostRenderer.materials = oldMaterials;
            }
        }

        /// <summary>
        /// Create a list of ghost materials to apply
        /// </summary>
        /// <param name="lenght">Arry lenght</param>
        /// <returns></returns>
        Material[] CreateGhostMaterialArray(int lenght)
        {
            Material[] ghosts = new Material[lenght];
            for (int i = 0; i < ghosts.Length; i++)
            {
                ghosts[i] = ghostMaterial;
            }
            return ghosts;
        }


        /****************************************************
        * External Prafab Setup
        * *************************************************/

        /// <summary>
        /// Add complex mesh handler if it's required
        /// </summary>
        void AddComplexGhostGenerator()
        {
            if (useCompleMesh)
            {
                if (complexGhostCreator == null)
                    complexGhostCreator = gameObject.AddComponent<ComplexGhostCreator>();
            }
        }

        /// <summary>
        /// Set the prefab to spawn, no ghost is created
        /// </summary>
        /// <param name="item">Item to spawn</param>
        public void SetObjectToPlace(BuildItem item)
        {
            if (item == null || !item.isValid())
            {
                Debug.LogError("Invalid item!");
                return;
            }

            objectToPlace = item.Prefab;
            useCompleMesh = item.isComplexMesh;

            AddComplexGhostGenerator();
        }

        /// <summary>
        /// Set the prefab to spawn and create its ghost
        /// </summary>
        /// <param name="item">Item to spawn</param>
        public void SetObjectToPlaceAndCreateGhost(BuildItem item)
        {
            if (item == null || !item.isValid())
            {
                
                Debug.LogError("invalid Item!");
                return;
            }
            SetObjectToPlace(item);
            CreateGhostObject();
        }


        /// <summary>
        /// Set a object to place without using ScriptableObjects
        /// </summary>
        /// <param name="prefab">Prefab to spawn</param>
        /// <param name="isComplexMesh">Require complex mesh computation</param>
        public void SetObjcetToPlace(GameObject prefab, bool isComplexMesh = false)
        {
            if (prefab == null)
            {
                Debug.LogError("Null prefab!");
                return;
            }
            objectToPlace = prefab;
            useCompleMesh = isComplexMesh;

            AddComplexGhostGenerator();
        }

        /// <summary>
        /// Set a object to place without using ScriptableObjects, and then create its ghost
        /// </summary>
        /// <param name="prefab">Prefab to spawn</param>
        /// <param name="isComplexMesh">Require complex mesh computation</param>
        public void SetObjectToPlaceAndCreateGhost(GameObject prefab, bool isComplexMesh = false)
        {
            if (prefab == null)
            {
                Debug.LogError("Null prefab!");
                return;
            }
            SetObjcetToPlace(prefab, isComplexMesh);
            CreateGhostObject();
        }


        /****************************************************
        * External Setup Misc
        * *************************************************/

        /// <summary>
        /// Set if the mouse is over a UI element. If yes this script won't place objects
        /// </summary>
        /// <param name="value"></param>
        public void SetIsMouseNotOnUI(bool value)
        {
            mouseIsNotOnUI = value;
        }

        /// <summary>
        /// Set the place mode
        /// </summary>
        /// <param name="pm">Mode</param>
        public void SetPlaceMode(PlaceMode pm)
        {
            switch (pm)
            {
                case PlaceMode.mousePos:
                    placeInScreenCenter = false;
                    break;
                case PlaceMode.screenCenter:
                    placeInScreenCenter = true;
                    break;
            }
        }

        /// <summary>
        /// Set rotation mode
        /// </summary>
        /// <param name="rm">Mode</param>
        public void SetRotaionMode(RotaionMode rm)
        {
            switch (rm)
            {
                case RotaionMode.snap:
                    faceMe = false;
                    break;
                case RotaionMode.facePlacer:
                    faceMe = true;
                    break;
            }
        }

        /// <summary>
        /// Set the snap angle to use in object snap rotation
        /// </summary>
        /// <param name="angle"></param>
        public void SetSnapAngle(float angle)
        {
            snapRotationAngle = angle;
        }

        /// <summary>
        /// Set the ghost material
        /// </summary>
        /// <param name="newGhostMaterial">material to use</param>
        public void SetGhostMaterial(Material newGhostMaterial)
        {
            ghostMaterial = newGhostMaterial;
        }


        /****************************************************
        * Debug
        * *************************************************/

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Ray r = cam.ScreenPointToRay(Input.mousePosition);
                Gizmos.DrawLine(r.origin, r.origin + r.direction * maxPlaceDistance);
            }
        }
#endif

    }
}
