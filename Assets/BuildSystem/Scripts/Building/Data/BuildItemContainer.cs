using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    [CreateAssetMenu(fileName = "Build Objs Container", menuName = "Building/Container", order = 1)]
    public class BuildItemContainer : ScriptableObject
    {
        public List<BuildItem> items;
    }
}
