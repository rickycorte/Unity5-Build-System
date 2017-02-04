using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "Build Obj", menuName = "Building/Object", order = 1)]
    public class ScriptableObjectToPlace : ScriptableObject
    {

        public string Name = "No Name";
        public Sprite UiPicture;
        public GameObject Prefab;
        public bool isComplexMesh = false;

    }
}
