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

        [Header("Objects Settings")]

        [Tooltip("Fill this only if you don't use ObjectSelector!")]
        [SerializeField]
        GameObject objectToPlace;

        [Tooltip("This material will be applied to the object when it is not placed!")]
        [SerializeField]
        Material ghostMaterial;

        [Header("Place Settings")] // -------------------------------------------------

        [Tooltip("Camera used to raycast and find place position, if empty the script will try to use the main camera")]
        [SerializeField]
        Camera cam;

        [Tooltip("Layers that this script will use to get hit points to place objects")]
        [SerializeField]
        LayerMask GroundLayer;

        [SerializeField]
        float maxPlaceDistance = 10f;

        [SerializeField]
        bool placeInScreenCenter = false;

        [Tooltip("Face the object to the player, THIS BLOCKS SNAP ROTAION!")]
        [SerializeField]
        bool faceMe = false;

        [SerializeField]
        float snapRotationAngle = 45;

        [Header("Input Settings")] // -------------------------------------------------

        [Tooltip("Key to press to enable the builder mode. This is also used by ObjectSelector")]
        [SerializeField]
        KeyCode toggleKey = KeyCode.E;

        [Tooltip("Key to press to place a item in the scene")]
        [SerializeField]
        KeyCode placeKey = KeyCode.Mouse0;

        [Tooltip("Key to press rotate (forward) the object based on snapRotaionDeg")]
        [SerializeField]
        KeyCode positiveRotateKey = KeyCode.Mouse1;

        [Tooltip("Key to press rotate (backward) the object based on snapRotaionDeg")]
        [SerializeField]
        KeyCode negativeRotateKey = KeyCode.None;

        /****************************************************
        * Public variables
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

        float pivotMargin = .1f;

        bool mouseIsNotOnUI = false;

        bool usingFakePivot = false;

        bool[] bodiesPrevState;

        float objectSnapCurrentRotaion = 0;

        Vector3 pivotOffsetExtra;


        /****************************************************
        * Components & references
        * *************************************************/

        Transform ghostObjInstance;
        MeshRenderer ghostRenderer;

        Material[] oldMaterials;
        Transform myTransform;

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
            if (canPlace)
            {
                MoveGhostObject();
                if (faceMe)
                {
                    RotateGhostToFaceMe();
                }
            }
        }

        /****************************************************
        * Activation
        * *************************************************/

        public void Toggle()
        {
            canPlace = !canPlace;
            if (canPlace) CreateGhostObject();
            else DestroyGhostObject();
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
            ghostObjInstance = Instantiate(objectToPlace, myTransform.position, Quaternion.identity).GetComponent<Transform>();

            //set ghost obj material
            var renderer = ghostObjInstance.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                ghostRenderer = renderer;
                EnableGhostMaterials();
                ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // remove shadows
            }

            //remove collisions and physics
            EnableGhostObjCollision(false);
            EnableGhostObjRigidbodies(false);

            //reset old object rotation
            objectSnapCurrentRotaion = 0;

            //check where is the pivot, if is not in the base create a fake one
            usingFakePivot = false;
            pivotMargin = ghostRenderer.bounds.extents.y * 2 / 3; //set the base pivot margin. its' height must be lower than obj center * 2/3
            bool objPivotIsBase = CheckIfObjectPivotIsCenter();
            //Debug.Log("Pivot is base: " + objPivotIsBase);
            if (!objPivotIsBase) CreateBasePivot();
        }


        //move the ghost object
        void MoveGhostObject()
        {
            RaycastHit hit;

            Ray r;
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
                Vector3 pos = hit.point;
                ghostObjInstance.position = pos;
                AlignGhostToSurface(hit.normal);
            }
        }


        //place the object in the scene
        void PlaceGhostObject()
        {
            if (ghostObjInstance != null)
            {
                ghostRenderer.materials = oldMaterials; //reset material with the old one
                EnableGhostObjRigidbodies(true); //reset rigidbody state
                EnableGhostObjCollision(true); //reset collisions
                ghostRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; // enable shadows
                Debug.Log("Created: " + ghostObjInstance.name);
                if (usingFakePivot)
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
            if (ghostObjInstance != null) Destroy(ghostObjInstance.gameObject);
        }


        /****************************************************
        * Ghost Object Alignament & Rotation
        * *************************************************/

        //align ghost object to surface based on raycast hit normal
        void AlignGhostToSurface(Vector3 hitNormal)
        {
            if (ghostObjInstance == null) return;

            ghostObjInstance.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(new Vector3(0, objectSnapCurrentRotaion, 0));

        }

        //check if the object pivot is in center or not
        bool CheckIfObjectPivotIsCenter()
        {
            if (ghostRenderer == null)
            {
                Debug.LogError("NO GHOST RENDER FOUND!");
                return false;
            }

            Vector3 delta = ghostObjInstance.position - ghostRenderer.bounds.center;
            if (delta.magnitude >= pivotMargin && delta.y < 0) //delta.y < 0 fix issues that not centerd pivots above the object center were taken as base pivots
            {
                pivotOffsetExtra = Vector3.zero;
                return true;
            }
            else
            {
                pivotOffsetExtra = delta; // save pivot delta to use to create a fake pivot
                return false;
            }
        }

        //create a pivot parent to better place the object
        void CreateBasePivot()
        {
            GameObject pivotG = new GameObject("Temp_Ghost_Pivot_Parent"); // create parent
            pivotG.AddComponent<PivotHelper>(); // add helper class to remove the pivot

            //recover mesh center
            Vector3 meshCenter = ghostRenderer.bounds.extents;
            meshCenter.x = pivotOffsetExtra.x;
            meshCenter.z = pivotOffsetExtra.z;
            meshCenter.y += pivotOffsetExtra.y; // apply pivot delta

            Transform pivotT = pivotG.transform;
            ghostObjInstance.SetParent(pivotT); // set the current object as child 
            ghostObjInstance.localPosition = meshCenter; // move the object up to make the parent the base pivot

            ghostObjInstance = pivotT; // replace old object reference with the parent
            usingFakePivot = true; //remember that we created a fake pivot
        }


        //rotate ghost object to face the placer
        void RotateGhostToFaceMe()
        {
            Vector3 dir = myTransform.position - ghostObjInstance.position;
            objectSnapCurrentRotaion = Quaternion.LookRotation(dir.normalized).eulerAngles.y;
        }

        //snap rotation of the object
        void SnapRotation(int mult)
        {
            objectSnapCurrentRotaion += mult * snapRotationAngle;
        }

        /****************************************************
        * Ghost Object properties modifiers
        * *************************************************/

        //toggle ghost colliders
        void EnableGhostObjCollision(bool val)
        {
            var cols = ghostObjInstance.GetComponentsInChildren<Collider>();
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


        //set all materials to ghost
        void EnableGhostMaterials()
        {
            if (ghostRenderer == null) return;
            oldMaterials = ghostRenderer.materials;

            ghostRenderer.materials = CreateGhostMaterialArray(ghostRenderer.materials.Length);
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
        * External Setup
        * *************************************************/

        //Set the prefab to spawn and create its ghost
        public void SetObjectToPlace(GameObject prefab)
        {
            objectToPlace = prefab;
            CreateGhostObject();
        }


        //set the prefab to spawn, NO GHOST is created
        public void SetObjectToPlaceNOGHOST(GameObject prefab)
        {
            objectToPlace = prefab;
        }

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
