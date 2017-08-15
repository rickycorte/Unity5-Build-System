using UnityEngine;

using BuildSystem;

public class ExternalCalls : MonoBehaviour {

    bool hide = false;

    [SerializeField] ObjectPlacer op;
    [SerializeField] ObjectSelector os;

    [SerializeField] GUIStyle style;

    int btnHor = 200;
    int btnVert = 50;

    bool opStatus = true;
    bool osStatus = true;

    bool placeForward = false;
    bool snapRotation = true;

    private void Start()
    {
        //subscribe to events
        op.OnGhostObjectCreation += () => { Debug.Log("Created ghost"); };
        op.OnGhostObjectDestroy += () => { Debug.Log("Deleted ghost"); };
        op.OnGhostObjectPlace += () => { Debug.Log("Placed Item"); };

        os.OnItemSelect += (int i) => { Debug.Log("Selected: " + i); };
        os.OnMenuToggle += (bool val) => { Debug.Log("Selection menu is active: " + val); };
        os.OnMenuCollapse += (bool val) => { Debug.Log("Selection menu is collapsed: " + val); };
    }

    public void ToggleOP()
    {
        op.Toggle();
    }

    public void ToggleOS()
    {
        os.Toggle();
    }

    public void ToggleEnableOP()
    {
        opStatus = !opStatus;
        op.Enable(opStatus);
    }

    public void ToggleEnableOS()
    {
        osStatus = !osStatus;
        opStatus = osStatus;
        os.Enable(osStatus);
    }

    public void SwapPlaceMode()
    {
        placeForward = !placeForward;
        if (placeForward)
            op.SetPlaceMode(ObjectPlacer.PlaceMode.screenCenter);
        else
            op.SetPlaceMode(ObjectPlacer.PlaceMode.mousePos);
    }

    public void SwapRotationMode()
    {
        snapRotation = !snapRotation;
        if (snapRotation)
        {
            op.SetRotaionMode(ObjectPlacer.RotationMode.snap);
        }
        else
        {
            op.SetRotaionMode(ObjectPlacer.RotationMode.facePlacer);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            hide = !hide;
        }
    }

    //create the ui buttons
    private void OnGUI()
    {
        if (!hide)
        {
            if (GUI.Button(new Rect(Screen.width - btnHor, 0, btnHor, btnVert), "Toggle Obj Placer", style))
                ToggleOP();
            if (GUI.Button(new Rect(Screen.width - btnHor, 1 * btnVert, btnHor, btnVert), "Toggle Obj Selector", style))
                ToggleOS();
            if (GUI.Button(new Rect(Screen.width - btnHor, 2 * btnVert, btnHor, btnVert), ((opStatus) ? "Disable" : "Enable") + " Obj Placer", style))
                ToggleEnableOP();
            if (GUI.Button(new Rect(Screen.width - btnHor, 3 * btnVert, btnHor, btnVert), ((osStatus) ? "Disable" : "Enable") + " Obj Selector", style))
                ToggleEnableOS();
            if (GUI.Button(new Rect(Screen.width - btnHor, 4 * btnVert, btnHor, btnVert), "To " + ((placeForward) ? "TPS mode" : "FPS mode"), style))
                SwapPlaceMode();
            if (GUI.Button(new Rect(Screen.width - btnHor, 5 * btnVert, btnHor, btnVert), "To "+ ((snapRotation) ? "Snap Rot" : "Face me Rot"), style))
                SwapRotationMode();

            GUI.Label(new Rect(Screen.width - btnHor, Screen.height - btnVert * 3, btnHor, btnVert * 3), "Open Build Menu: E\nCollapse Menu: Q"
                +"\nPlace: Mouse 0\nRotare: Mouse 1\nHide Buttons: H", style);
        }

    }

}
