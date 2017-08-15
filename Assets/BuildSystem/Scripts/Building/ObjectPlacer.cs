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

        [Tooltip("Camera used to raycast and find place position, if empty the script will try to use the main camera")]
        public Camera cam;

        [Header("Object Settings")]

        [Tooltip("Fill this only if you don't use ObjectSelector!")]
        [SerializeField] BuildItem objectToPlace;

        //**********************************************************************************************
        [Header("Place Settings")]

        [Tooltip("Layers that this script will use to get hit points to place objects")]
        public LayerMask groundLayer;

        [Tooltip("Max distance from camera where you can place objects")]
        public float maxPlaceDistance = 10f;

        [Tooltip("Reccomned for FPS games")]
        [SerializeField] bool placeInScreenCenter = false;

        //**********************************************************************************************
        [Header("Rotation Settings")]

        [Tooltip("Face the object to the player, THIS BLOCKS SNAP ROTATION!")]
        [SerializeField] bool faceMe = false;

        [Tooltip("Amount of degrees that will be added to rotate ghost object")]
        [SerializeField] float snapRotationAngle = 45;

        [Tooltip("Should the script reset obejct rotation to 0 or keep the previuos one. Note: works only with snap rotaion")]
        public bool resetRotationAfterPlace = false;

        [Tooltip("Keep rotating object when holding down the rotate key")]
        [SerializeField] bool useContinuousRotation = false;

        [Tooltip("Continuos rotation speed")]
        public float continuousRotationSpeed = 7f;

        //**********************************************************************************************
        [Header("Input Settings")] 

        [Tooltip("Key to press to enable the builder mode. This is also used by ObjectSelector")]
        public KeyCode toggleKey = KeyCode.E;

        [Tooltip("Key to press to place a item in the scene")]
        public KeyCode placeKey = KeyCode.Mouse0;

        [Tooltip("Key to press rotate (forward) the object based on snapRotaionDeg")]
        public KeyCode positiveRotateKey = KeyCode.Mouse1;

        [Tooltip("Key to press rotate (backward) the object based on snapRotaionDeg")]
        public KeyCode negativeRotateKey = KeyCode.None;

        [Header("More Settings")]
        [Tooltip("Disable the remover script when placer is active. Note: remover must be next to this script")]
        public bool shouldToggleRemover = true;

        /****************************************************
        * Public variables & Classes
        * *************************************************/
        public enum PlaceMode { mousePos, screenCenter };

        public enum RotationMode { snap, facePlacer, continuous };

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
        bool usingFakePivot = false;

        /****************************************************
        * Components & references
        * *************************************************/

        Transform ghostObjInstance;

        Transform myTransform;

        ObjectRemover object_remover;

        /****************************************************
        * Init
        * *************************************************/

        private void Start()
        {
            if (cam == null)
                cam = Camera.main;
            myTransform = transform;

            if (cam == null) Debug.LogError("Missing cam, please assign it!");

            object_remover = GetComponent<ObjectRemover>();

            if (faceMe && useContinuousRotation)
            {
                Debug.LogError("Settings conflict, faceMe mode can't be enabled in continuous rotation");
                faceMe = false;
            }
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
                    if (!useContinuousRotation)
                    {
                        if (Input.GetKeyDown(positiveRotateKey))
                            AddRotation(+1,snapRotationAngle); // positive rotation

                        if (Input.GetKeyDown(negativeRotateKey))
                            AddRotation(-1,snapRotationAngle); // negative rotation
                    }
                    else
                    {
                        if (Input.GetKey(positiveRotateKey))
                            AddRotation(+1, continuousRotationSpeed * Time.deltaTime); // positive rotation

                        if (Input.GetKey(negativeRotateKey))
                            AddRotation(-1, continuousRotationSpeed * Time.deltaTime); // negative rotation
                    }

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
                    objectSnapCurrentRotaion =  GetFaceToRotation(myTransform, ghostObjInstance);
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
            if (canPlace)
            {
                CreateGhostObject();
                if (object_remover != null)
                {
                    object_remover.Activate(false);
                }
            }
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
                Debug.LogError("No item to instantiate! Aborting ghost creation.");
                return;
            }
      
            ghostObjInstance = Instantiate(objectToPlace.ghostCache, myTransform.position, Quaternion.identity).GetComponent<Transform>();

            //reset old object rotation
            if (resetRotationAfterPlace)
                objectSnapCurrentRotaion = 0;

            //check where is the pivot, if it is not in the base create a fake one
            usingFakePivot = false;
            

            Vector3 pivotOffsetExtra;
            bool objPivotIsBase = isPivotInBase(ghostObjInstance, out pivotOffsetExtra);

            //create a fake pivot if the real one is not in base
            if (!objPivotIsBase)
            {
                ghostObjInstance = CreateBasePivot(ghostObjInstance, pivotOffsetExtra);
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

                Debug.Log("Created: " + ghostObjInstance.name);

                if (usingFakePivot) // remove fake pivot if using one
                {
                    ghostObjInstance = ghostObjInstance.GetComponent<PivotHelper>().DeletePivot();
                }

                //place real object in scene
                Instantiate(objectToPlace.Prefab, ghostObjInstance.position, ghostObjInstance.rotation);

                Destroy(ghostObjInstance.gameObject);

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

                if (OnGhostObjectDestroy != null)
                {
                    OnGhostObjectDestroy();
                }
            }
        }


        /****************************************************
        * Ghost Object Alignament 
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


        /****************************************************
        * Ghost Object Rotation
        * *************************************************/

        /// <summary>
        /// Rotate ghost object to face the placer
        /// </summary>
        /// <param name="target">Object placer</param>
        /// <param name="other">Item to rotate</param>
        /// <returns></returns>
        float GetFaceToRotation(Transform target, Transform other)
        {
            if (target == null || other == null)
                Debug.LogError("GetFaceToRotaion can't have null parameters");

            Vector3 dir = target.position - other.position;
            return Quaternion.LookRotation(dir.normalized).eulerAngles.y;
        }

        /// <summary>
        /// Add amout rotation with dir mult
        /// </summary>
        /// <param name="mult">Direction to add rotation</param>
        /// <param name="amount">Degs to add to rotation</param>
        void AddRotation(int mult, float amount)
        {
            objectSnapCurrentRotaion += mult * amount;
        }

        /****************************************************
        * Pivot helpers
        * *************************************************/

        /// <summary>
        /// Create a pivot parent to better place the object
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <param name="renderer">Renderer of the item</param>
        /// <param name="pivotOffset">Offset of the pivot</param>
        /// <returns></returns>
        Transform CreateBasePivot(Transform item, Vector3 pivotOffset)
        {
            var normMesh = item.GetComponentInChildren<MeshRenderer>();
            var skinMesh = item.GetComponentInChildren<SkinnedMeshRenderer>();

            if (normMesh  == null && skinMesh == null)
            {
                Debug.LogError("No renderers found!");
                return null;
            }

            GameObject pivotG = new GameObject("Temp_Ghost_Pivot_Parent"); // create parent
            Transform pivotT = pivotG.transform;

            pivotG.AddComponent<PivotHelper>(); // add helper class to remove the pivot when object is spawned

            //get mesh center
            Vector3 meshCenter = (normMesh != null) ? normMesh.bounds.extents : skinMesh.bounds.extents;
            // apply pivot delta
            meshCenter.x = pivotOffset.x;
            meshCenter.z = pivotOffset.z;
            meshCenter.y += pivotOffset.y; 


            item.SetParent(pivotT); // set the current object as parent
            item.localPosition = meshCenter; // move the object and leave the parent object in the pivot position

            return pivotT;
        }

        /// <summary>
        /// Check if the object pivot is in center or not.
        /// This function returns the pivot Offset (can be Vector3.zero)
        /// </summary>
        /// <param name="item">Item to use</param>
        /// <param name="renderer">Renderer attached to the item</param>
        /// <param name="pivotOffset">Offset of the pivot to be in base</param>
        /// <returns></returns>
        bool isPivotInBase(Transform item, out Vector3 pivotOffset)
        {
            var normMesh = item.GetComponentInChildren<MeshRenderer>();
            var skinMesh = item.GetComponentInChildren<SkinnedMeshRenderer>();

            if (normMesh == null && skinMesh == null)
            {
                Debug.LogError("No mesh renderer found!");
                pivotOffset = Vector3.zero;
                return false;
            }

            var pivotMargin = (normMesh != null)? normMesh.bounds.extents.y * 2 / 3 : skinMesh.bounds.extents.y * 2 / 3 ; //set the base pivot margin. 
            //Its' height must be lower than obj center * 2/3

            Vector3 delta = item.position - ( (normMesh != null)? normMesh.bounds.center : skinMesh.bounds.center);
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

        /****************************************************
        * External Prafab Setup
        * *************************************************/

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

            objectToPlace = item;
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
        public void SetRotaionMode(RotationMode rm)
        {
            switch (rm)
            {
                case RotationMode.snap:
                    faceMe = false;
                    useContinuousRotation = false;
                    break;
                case RotationMode.facePlacer:
                    useContinuousRotation = false;
                    faceMe = true;
                    break;
                case RotationMode.continuous:
                    faceMe = false;
                    useContinuousRotation = true;
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

        //TODO: add more public functions to edit settings at runtime

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
