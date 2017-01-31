using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotHelper : MonoBehaviour {

    public void DeletePivot()
    {
        Transform t = GetComponentsInChildren<Transform>()[1];
        t.parent = null;
        Destroy(this.gameObject);
    }

}
