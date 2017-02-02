using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{

    [CreateAssetMenu(fileName = "Build Obj", menuName = "Building/Object", order = 1)]
    public class ScriptableObjectToPlace : ScriptableObject
    {

        public uint Id = 0; //this is not really used by the place system, btw it can be used by you
        public string Name = "No Name";
        public Sprite UiPicture;
        public GameObject Prefab;

    }
}
