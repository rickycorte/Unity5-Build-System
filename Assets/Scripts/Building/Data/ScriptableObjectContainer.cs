using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Build Objs Container", menuName = "Building/Container", order = 1)]
public class ScriptableObjectContainer : ScriptableObject {

    public List<ScriptableObjectToPlace> items;
}
