using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Build Obj",menuName = "Building/Object",order = 1)]
public class ScriptableObjectToPlace : ScriptableObject {

    public uint Id = 0; // viene usato per identificare l'oggetto nel container
    public string Name = "No Name";
    public Sprite UiPicture;
    public GameObject Prefab;
   
}
