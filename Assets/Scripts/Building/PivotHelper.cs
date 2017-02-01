using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotHelper : MonoBehaviour {

    //remove the fake pivot and leave the original object in the scene
    public void DeletePivot()
    {
        Transform t = GetComponentsInChildren<Transform>()[1];
        t.parent = null;
        Destroy(this.gameObject);
    }

}
