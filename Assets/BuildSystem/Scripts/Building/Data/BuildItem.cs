using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

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

        [System.Obsolete("This option no longer impact placer behaviuor")]
        public bool isComplexMesh = false;

        GameObject ghostCache;

        const string cachePath = "Assets/BuildSystem/Cache";

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

            if (ghostCache == null)
            {
                Debug.LogError("item: "+ name +" is no cached ghost plese regenerate it");
            }

            return true;
        }

#if UNITY_EDITOR


        public Material ghostMaterial;

        public void CreateGhost()
        {
            DeleteOldGhost();

            if (Prefab == null) return;

            CreateFolder(cachePath);

            //make a copy of the prefab
            GameObject g = Instantiate(Prefab);

            RemoveAllExceptMeshes(g);

            ReplaceMaterials(g);

            //save the prefab in cache folder
            ghostCache = PrefabUtility.CreatePrefab(cachePath +"/"+ ( (name == "") ? Prefab.name + "_ghost" : name + "_ghost" )+ ".prefab", g);

            //delete the copy
            DestroyImmediate(g);

        }

        public void DeleteOldGhost()
        {
            if (ghostCache == null) return;

            AssetDatabase.DeleteAsset(cachePath + "/" + ghostCache.name);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Remove all components except rendering ones
        /// </summary>
        /// <param name="g"></param>
        void RemoveAllExceptMeshes(GameObject g)
        {
            var comps = g.GetComponentsInChildren<Component>();

           for (int i = 0; i < comps.Length; i++)
           {
                //skip rendering components
                if (comps[i] is MeshRenderer || comps[i] is SkinnedMeshRenderer ||
                    comps[i] is Transform || comps[i] is MeshFilter /*|| comps[i] is TextMesh*/ )
                {
                    continue;
                }

                DestroyImmediate(comps[i]);
           }
            
        }

        /// <summary>
        /// Replace current materials with ghost one
        /// </summary>
        void ReplaceMaterials(GameObject g)
        {
            if (ghostMaterial == null) return; // do nothing if no material is set

            // replace mesh renderes materials
            foreach (var mr in g.GetComponentsInChildren<MeshRenderer>())
            {
                mr.sharedMaterials = createMarArr(mr.sharedMaterials.Length);
            }

            // replace mesh renderes materials
            foreach (var mr in g.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                mr.sharedMaterials = createMarArr(mr.sharedMaterials.Length);
            }

        }

        /// <summary>
        /// Create an array of materials to replace the current ones
        /// </summary>
        Material[] createMarArr(int cout)
        {
            Material[] mat = new Material[cout];
            for (int i = 0; i < mat.Length; i++)
            {
                mat[i] = ghostMaterial;
            }

            return mat;
        }


        /// <summary>
        /// Create check folder
        /// </summary>
        void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.Log("Created directory: " + path);
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();

                PrefabUtility.CreatePrefab(cachePath + "/_DONT_TOUCH_THIS_FOLDER.prefab", new GameObject());
            }
        }

#endif


    }
}
