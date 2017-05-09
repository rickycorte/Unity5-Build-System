
using UnityEngine;
using UnityEditor;

namespace BuildSystem
{
    /// <summary>
    /// [EDITOR ONLY] Material assigner window
    /// </summary>
    public class ObjectGhostMaterialAssigner : EditorWindow
    {
        static int HorizontalSize = 300;
        static int VerticalSize = 100;

        Material gMat;
        bool removeOld;

        [MenuItem("Build System/Assing Material")]
        public static void ShowWindow()
        {
            //create a windows with a fixed size
            GetWindowWithRect(typeof(ObjectGhostMaterialAssigner), new Rect(Screen.width / 2, 200, HorizontalSize, VerticalSize), true, "Material Assigner");
        }


        private void OnGUI()
        {
           EditorGUILayout.LabelField("Ghost material: ");
           gMat = (Material)EditorGUILayout.ObjectField("", gMat , typeof(Material), false);

           EditorGUILayout.LabelField("Ovverride current ghost material: ");
           removeOld = EditorGUILayout.Toggle(removeOld);

            if (GUILayout.Button("Set material"))
            {
                if (gMat != null)
                    SetMaterial(gMat, removeOld);
                else Debug.LogError("Please assign a material");
            }
        }

        /// <summary>
        /// Set all BuildItem ghost materials
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="over"></param>
        void SetMaterial(Material mat, bool over)
        {
            string[] paths = AssetDatabase.FindAssets("t:BuildItem");

            for (int i = 0; i < paths.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Processing", "Setting up ghost materials " + (i + 1) + "/" + paths.Length, (float)i + 1 / (float)paths.Length);

                //load gameobject
                var bi= (BuildItem)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[i]), typeof(BuildItem));

                //set material
                if (over || bi.ghostMaterial == null)
                {
                    bi.ghostMaterial = mat;
                    //save changes
                    EditorUtility.SetDirty(bi);
                    AssetDatabase.SaveAssets();

                    //create the ghost with the new material
                    bi.CreateGhost();
                }
               
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

    }
}
