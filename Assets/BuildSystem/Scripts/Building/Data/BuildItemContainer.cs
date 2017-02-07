using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// List of Items to use in Object Selector
    /// </summary>
    [CreateAssetMenu(fileName = "BuildItemContainer", menuName = "Building/Container", order = 1)]
    public class BuildItemContainer : ScriptableObject
    {
        public List<BuildItem> items = new List<BuildItem>();

        /// <summary>
        /// Checks if the container is valid
        /// </summary>
        /// <returns></returns>
        public bool isValid()
        {
            return items.Count > 0;
        }

    }
}
