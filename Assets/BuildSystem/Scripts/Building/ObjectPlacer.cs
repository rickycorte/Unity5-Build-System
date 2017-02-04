using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{

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
        [SerializeField] LayerMask GroundLayer;

        [SerializeField] float maxPlaceDistance = 10f;

        [SerializeField] bool placeInScreenCenter = false;

        //**********************************************************************************************
        [Header("Rotation Settings")]

        [Tooltip("Face the object to the player, THIS BLOCKS SNAP ROTAION!")]
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

        public KeyCode TOGGLEKEY { get { return toggleKey; } }

        public bool IsActive { get { return isActive; } }


        /****************************************************
        * Variables
        * *************************************************/

        bool isActive = true;

        bool canPlace = false;

        bool mouseIsNotOnUI = false;

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

            if (ghostMaterial == null) Debug.LogError("Missing ghostMaterial, please assign it!");
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
            if (canPlace && ghostObjInstance != null)
            {
                MoveGhostObject();
                if (faceMe)
                {
                    snapRotationAngle =  GetFaceToRotation(myTransform, ghostObjInstance);
                }
            }
        }

        /****************************************************
        * Activation
        * *************************************************/

        public void Toggle()
        {
            canPlace = !canPlace;
            Toggle(canPlace);
        }


        public void Toggle(bool val)
        {
            canPlace = val;
            if (canPlace) CreateGhostObject();
            else DestroyGhostObject();
        }


        //active the script input handler, use toggle to start placeing objects
        public void Enable(bool val)
        {
            isActive = val;
            Toggle(false);
        }

        /****************************************************
         * Ghost Object Creation & Movement & Place
         * *************************************************/

        //create ghost object
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
        }


        //move the ghost object
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
            if (Physics.Raycast(r, out hit, maxPlaceDistance, GroundLayer))
            {
                //set object position to hit point
                Vector3 pos = hit.point;
                ghostObjInstance.position = pos;

                AlignGhostToSurface(ghostObjInstance,hit.normal);
            }
        }


        //place the object in the scene
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

                CreateGhostObject(); //create a new ghost object
            }
            else Debug.LogError("Unable to spawn object, ghost reference is null!");
        }


        //remove ghost object from scene
        void DestroyGhostObject()
        {
            if (ghostObjInstance != null)
            {
                Destroy(ghostObjInstance.gameObject);
                if (complexGhostCreator != null) complexGhostCreator.ClearCache();
            }
        }


        /****************************************************
        * Ghost Object Alignament & Rotation
        * *************************************************/

        //align ghost object to surface based on raycast hit normal
        void AlignGhostToSurface(Transform itemToAlign,Vector3 hitNormal)
        {
            if (itemToAlign == null) return;

            itemToAlign.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(new Vector3(0, objectSnapCurrentRotaion, 0));

        }

        //check if the object pivot is in center or not
        //this function returns the pivot Offset (can be Vector3.zero)
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

        //create a pivot parent to better place the object
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


        //rotate ghost object to face the placer
        float GetFaceToRotation(Transform target,Transform other)
        {
            if (objectToPlace == null || ghostRenderer)
                Debug.LogError("GetFaceToRotaion can't have null parameters");

            Vector3 dir = target.position - other.position;
            return Quaternion.LookRotation(dir.normalized).eulerAngles.y;
        }

        //snap rotation of the object
        void SnapRotation(int mult)
        {
            objectSnapCurrentRotaion += mult * snapRotationAngle;
        }

        /****************************************************
        * Ghost Object properties modifiers
        * *************************************************/

        //active/reset shadow preset of renderer
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

        //toggle ghost colliders
        void EnableObjectCollision(Transform item ,bool val)
        {
            if (item == null) return;

            var cols = item.GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = val;
            }
        }


        //set all rigidbodies to be kinematic or reset them viwh previous state
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

        //save the old state for the body reset
        void GetOldRigidBodyStateAndOvverride(bool val, Rigidbody[] bodies)
        {
            bodiesPrevState = new bool[bodies.Length];

            for (int i = 0; i < bodies.Length; i++)
            {
                bodiesPrevState[i] = bodies[i].isKinematic;
                bodies[i].isKinematic = !val;
            }
        }

        //set the old body state
        void SetOldRigidbodyState(Rigidbody[] bodies)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].isKinematic = bodiesPrevState[i];
            }
        }


        //set all materials to ghost and store the old ones
        void EnableGhostMaterials(bool val)
        {
            if (ghostRenderer == null) return;

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

        //create a list of ghost materials to apply
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

        //add complex mesh handler if it's required
        void AddComplexGhostGenerator()
        {
            if (useCompleMesh)
            {
                if (complexGhostCreator == null)
                    complexGhostCreator = gameObject.AddComponent<ComplexGhostCreator>();
            }
        }

        //set the prefab to spawn, NO GHOST is created
        public void SetObjectToPlace(BuildItem item)
        {
            if (item == null || !BuildItem.isValid(item))
            {
                Debug.LogError("Invalid item!");
                return;
            }

            objectToPlace = item.Prefab;
            useCompleMesh = item.isComplexMesh;

            AddComplexGhostGenerator();
        }

        //Set the prefab to spawn and create its ghost
        public void SetObjectToPlaceAndCreateGhost(BuildItem item)
        {
            if (item == null || !BuildItem.isValid(item))
            {
                
                Debug.LogError("invalid Item!");
                return;
            }
            SetObjectToPlace(item);
            CreateGhostObject();
        }


        //set a object to place without using ScriptableObjects
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

        //set a object to place without using ScriptableObjects, and then crete its ghost
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

        //set if the mouse is over a ui element and this script should not place and object
        public void SetIsMouseNotOnUI(bool value)
        {
            mouseIsNotOnUI = value;
        }

        //set the place mode
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

        //set the object rotaion mode
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

        //Set the snap angle to use in object snap rotation
        public void SetSnapAngle(float angle)
        {
            snapRotationAngle = angle;
        }


        /****************************************************
        * Debug
        * *************************************************/

#if UNITY_EDITOR
        //DEBUG
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
