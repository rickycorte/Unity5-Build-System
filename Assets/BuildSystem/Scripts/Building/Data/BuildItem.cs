using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
#endif

namespace BuildSystem
{
    /// <summary>
    /// Item to use in Build Item Container
    /// </summary>
    [CreateAssetMenu(fileName = "BuildItem", menuName = "Building/Item", order = 1)]
    public class BuildItem : ScriptableObject
    {
        public string Name = "";
        public Sprite UiPicture;
        public GameObject Prefab;
        public GameObject ghostCache;

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
                Debug.LogError("item: "+ name +" has no cached ghost please regenerate it");
            }

            return true;
        }

#if UNITY_EDITOR

        const string cachePath = "Assets/BuildSystem/Data/Cache";

        public Material ghostMaterial;

        /// <summary>
        /// [EDITOR ONLY] Create ghost object and save it to disk
        /// </summary>
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
            ghostCache = PrefabUtility.CreatePrefab(cachePath +"/"+ Prefab.name + "_ghost.prefab", g);

            //delete the copy
            DestroyImmediate(g);

            //save new reference
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

        }

        public void DeleteOldGhost()
        {
            if (ghostCache == null) return;

            AssetDatabase.DeleteAsset(cachePath + "/" + ghostCache.name);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// [EDITOR ONLY] Remove all components except rendering ones
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
        /// [EDITOR ONLY] Replace current materials with ghost one
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
        /// [EDITOR ONLY] Create an array of materials to replace the current ones
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
        /// [EDITOR ONLY] Create check folder
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

        /// <summary>
        /// [EDITOR ONLY] Gnerate an automatic name based on prefab name and assign it to item
        /// </summary>
        /// <param name="g">Object to use</param>
        public void SetAutomaticName(GameObject g)
        {
            string res = g.name;

            //search where there are upper letters with lower ones and put space there
            List<int> breakPos = new List<int>();
            //find parts line nP cR ...
            for (int i = 1; i < res.Length; i++)
            {
                if (char.IsUpper(res[i]) && char.IsLower(res[i - 1])) // controllare sto coso che non va
                {
                    breakPos.Add(i);
                }
            }
            //insert spaces
            for (int i = 0; i < breakPos.Count; i++)
            {
                res = res.Insert(breakPos[i], " ");
            }

            res = res.Replace('_', ' ');

            Name = res;

            //save new reference
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Name set to: " + res);

        }

        /// <summary>
        /// [EDITOR ONLY] Generate automatic preview and assign it to item
        /// </summary>
        /// <param name="g">Object to use for preview</param>
        public void SetAutomaticPreview(GameObject g)
        {
            string path = ObjectPreview.CreateAndSaveAssetPreview(g);
            if (path == "") return;

            UiPicture = (Sprite)AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));

            //save new reference
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Preview Set to " + path );
        }
    
        /// <summary>
        /// [EDITOR ONLY] Set an automatic material to the item
        /// </summary>
        public void SetAutomaticMaterial()
        {

            ghostMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/BuildSystem/Materials/GhostObjectMaterial.mat", typeof(Material));
            //save new reference
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log("Default Material set");
        }

#endif


    }
}
