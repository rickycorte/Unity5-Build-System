using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour {

    /****************************************************
    * Editor Interface
    * *************************************************/

    [Header("Objects Settings")]

    [Tooltip("Fill this only if you don't use ObjectSelector!")]
    [SerializeField] GameObject objectToPlace;

    [Tooltip("This material will be applied to the object when it is not placed!")]
    [SerializeField] Material ghostMaterial;

    [Header("Place Settings")] // -------------------------------------------------

    [Tooltip("Camera used to raycast and find place position, if empty the script will try to use the main camera")]
    [SerializeField] Camera cam;

    [Tooltip("Layers that this script will use to get hit points to place objects")]
    [SerializeField] LayerMask GroundLayer;

    [SerializeField] float maxPlaceDistance = 10f;

    [SerializeField] bool placeInScreenCenter = false;

    [Tooltip("Face the object to the player, THIS BLOCKS SNAP ROTAION!")]
    [SerializeField] bool faceMe = false;

    [SerializeField] float snapRotationAngle = 45;

    [Header("Input Settings")] // -------------------------------------------------

    [Tooltip("Key to press to enable the builder mode. This is also used by ObjectSelector")]
    [SerializeField] KeyCode ToggleKey = KeyCode.E;

    [Tooltip("Key to press to place a item in the scene")]
    [SerializeField] KeyCode PlaceKey = KeyCode.Mouse0;

    [Tooltip("Key to press rotate (forward) the object based on snapRotaionDeg")]
    [SerializeField] KeyCode PositiveRotateKey = KeyCode.Mouse1;

    [Tooltip("Key to press rotate (backward) the object based on snapRotaionDeg")]
    [SerializeField] KeyCode NegativeRotateKey = KeyCode.None;

    /****************************************************
    * Public variables
    * *************************************************/
    public enum PlaceMode { mousePos, screenCenter};

    public enum RotaionMode { snap, facePlacer };

    public KeyCode TOGGLEKEY { get { return ToggleKey; } }

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

    float ObjectSnapCurrentRotaion = 0;


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
        if(cam == null)
           cam = Camera.main;
        myTransform = transform;

        if (ghostMaterial == null) Debug.LogError("Missing ghostMaterial, please assign it!");
        if(cam == null) Debug.LogError("Missing cam, please assign it!");
    }

    /****************************************************
    * Input
    * *************************************************/

    void Update()
    {
        //toggle contruction mode
        if (Input.GetKeyDown(ToggleKey) && isActive)
        {
            Toggle();
        }

        //place the object
        if(canPlace)
        {
            if(Input.GetKeyDown(PlaceKey) && mouseIsNotOnUI)
               PlaceGhostObject();

            if (!faceMe)
            {

                if (Input.GetKeyDown(PositiveRotateKey))
                    SnapRotation(+1); // positive rotation

                if (Input.GetKeyDown(NegativeRotateKey))
                    SnapRotation(-1); // negative rotation
            }
        }



    }


    /****************************************************
    * Ghost Obj Position update
    * *************************************************/

    private void FixedUpdate()
    {
        if(canPlace)
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
        ghostObjInstance = Instantiate(objectToPlace,myTransform.position,Quaternion.identity).GetComponent<Transform>();
        
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
        ObjectSnapCurrentRotaion = 0;

        //check where is the pivot, if is notin the base create one
        usingFakePivot = false;
        bool objPivotIsBase = CheckIfObjectPivotIsCenter();
        Debug.Log("Pivot is base: " + objPivotIsBase);
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

        if (Physics.Raycast(r, out hit, maxPlaceDistance, GroundLayer))
        {
            Vector3 pos = hit.point;
            ghostObjInstance.position = pos;
            AllignGhostToSurface(hit.normal);
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
    void AllignGhostToSurface(Vector3 hitNormal)
    {
        if (ghostObjInstance == null) return;

        ghostObjInstance.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(new Vector3(0, ObjectSnapCurrentRotaion, 0)) ;

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
        if (delta.magnitude >= pivotMargin && delta.y<0) //delta.y<0 fix issues that not centerd pivots above the object center were taken as base pivots
        {
            return true;
        }
        else return false;       
    }

    //create a pivot parent to better place the object
    void CreateBasePivot()
    {
        GameObject pivotG = new GameObject("Temp_Ghost_Pivot_Parent"); // create parent
        pivotG.AddComponent<PivotHelper>(); // add helper class to remove the pivot

        Vector3 meshCenter = ghostRenderer.bounds.extents;
        meshCenter.x = 0; meshCenter.z = 0;
        Transform pivotT = pivotG.transform;
        ghostObjInstance.SetParent(pivotT); // set the current objet as child 
        ghostObjInstance.localPosition = meshCenter; // move the object up to make the parent the base pivot
        ghostObjInstance = pivotT;
        usingFakePivot = true;
    }


    //rotate ghost object to face the placer
    void RotateGhostToFaceMe()
    {
        Vector3 dir = myTransform.position - ghostObjInstance.position;
        ObjectSnapCurrentRotaion = Quaternion.LookRotation(dir.normalized).eulerAngles.y;
    }

    //snap rotation of the object
    void SnapRotation(int mult)
    {
        ObjectSnapCurrentRotaion += mult*snapRotationAngle;
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
        var bodies = ghostObjInstance.GetComponentsInChildren<Rigidbody>();
        if(val == false)
           bodiesPrevState = new bool[bodies.Length];

        for (int i = 0; i < bodies.Length; i++)
        {
            if (val == false)
            {
                bodiesPrevState[i] = bodies[i].isKinematic; //save the old state for the body reset
                bodies[i].isKinematic = !val;
            }
            else
            {
                bodies[i].isKinematic = bodiesPrevState[i]; //set the old body state
            }
        }
    }


    //set all materials to ghost
    void EnableGhostMaterials()
    {
        if (ghostRenderer == null) return;
        oldMaterials = ghostRenderer.materials;
        //create a list of ghost materials to apply
        Material[] ghosts = new Material[ghostRenderer.materials.Length];
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i] = ghostMaterial;
        }
        ghostRenderer.materials = ghosts;
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
