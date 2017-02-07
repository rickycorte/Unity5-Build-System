using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// Helper class that handle the fake pivot deletion
    /// </summary>
    public class PivotHelper : MonoBehaviour
    {

        /// <summary>
        /// Remove the fake pivot and leave the original object in the scene
        /// </summary>
        public void DeletePivot()
        {
            Transform t = GetComponentsInChildren<Transform>()[1];
            t.parent = null;
            Destroy(this.gameObject);
        }

    }
}
