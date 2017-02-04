using UnityEngine;

namespace BuildSystem
{
    [CreateAssetMenu(fileName = "Build Obj", menuName = "Building/Object", order = 1)]
    public class BuildItem : ScriptableObject
    {
        public string Name = "No Name";
        public Sprite UiPicture;
        public GameObject Prefab;
        public bool isComplexMesh = false;
    }
}
