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
        public Transform DeletePivot()
        {
            Transform t = GetComponentsInChildren<Transform>()[1];
            t.parent = null;
            Destroy(this.gameObject,0.1f); // delay self destroy to make sure return runs

            return t;
        }

    }
}
