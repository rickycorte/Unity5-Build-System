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


        //check if the item is valid or not
        public static bool isValid(BuildItem i)
        {
            if (i.Name == "")
                Debug.LogWarning("Build Item name is null");

            if (i.UiPicture == null)
            {
                Debug.LogError("item: " + i.name + " has null UiPicture!");
                return false;
            }

            if (i.Prefab == null)
            {
                Debug.LogError("item: " + i.name + " has null Prefab!");
                return false;
            }

            return true;
        }
    }
}
