using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalCalls : MonoBehaviour {

    [SerializeField] ObjectPlacer op;
    [SerializeField] ObjectSelector os;

    [SerializeField] GUIStyle style;

    int btnHor = 300;
    int btnVert = 50;

    bool opStatus = true;
    bool osStatus = true;

    bool placeForward = false;

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

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width-btnHor,0,btnHor,btnVert),"Toggle Obj Placer", style))
            ToggleOP();
        if (GUI.Button(new Rect(Screen.width - btnHor, 1 * btnVert, btnHor, btnVert), "Toggle Obj Selector",style))
            ToggleOS();
        if (GUI.Button(new Rect(Screen.width - btnHor, 2 * btnVert, btnHor, btnVert), ( (opStatus)?"Disable":"Enable" )+" Object Placer", style))
            ToggleEnableOP();
        if (GUI.Button(new Rect(Screen.width - btnHor, 3 * btnVert, btnHor, btnVert), ((osStatus) ? "Disable" : "Enable") + " Object Selector", style))
            ToggleEnableOS();
        if (GUI.Button(new Rect(Screen.width - btnHor, 4 * btnVert, btnHor, btnVert), (placeForward) ? "TPS mode" : "FPS mode",style))
            SwapPlaceMode();

        GUI.Label(new Rect(Screen.width - btnHor, Screen.height - btnVert, btnHor, btnVert * 2), "Open Build Menu: E\nPlace Object: Mouse 0");

    }

}
