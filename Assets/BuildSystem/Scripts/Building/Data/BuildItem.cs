using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// Item to use in Build Item Container
    /// </summary>
    [CreateAssetMenu(fileName = "BuildItem", menuName = "Building/Item", order = 1)]
    public class BuildItem : ScriptableObject
    {
        public string Name = "No Name";
        public Sprite UiPicture;
        public GameObject Prefab;
        public bool isComplexMesh = false;


        /// <summary>
        /// Check if a Build Item is valid or not
        /// </summary>
        /// <returns></returns>
        public bool isValid()
        {
            if (Name == "")
                Debug.LogWarning("Build Item name is null");

            if (UiPicture == null)
            {
                Debug.LogError("item: " + name + " has null UiPicture!");
                return false;
            }

            if (Prefab == null)
            {
                Debug.LogError("item: " + name + " has null Prefab!");
                return false;
            }

            return true;
        }
    }
}
