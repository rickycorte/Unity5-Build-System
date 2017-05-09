using UnityEngine;
using UnityEditor;
using System.IO;

namespace BuildSystem
{
    /// <summary>
    /// [EDITOR ONLY] Helper window that creates png previews of prefabs in low resolution using Unity asset preview
    /// </summary>
    public class ObjectPreviewWindow : EditorWindow
    {

        GameObject obj;

        Texture2D currentPreview;

        static int HorizontalSize = 300;
        static int VerticalSize = 510;
        int TextureMargin = 5;

        [MenuItem("Build System/Create Object Preview")]
        public static void ShowWindow()
        {
            //create a windows with a fixed size
            GetWindowWithRect(typeof(ObjectPreviewWindow), new Rect(Screen.width / 2, 200, HorizontalSize, VerticalSize), true, "Object Preview Generator");
        }

        private void OnGUI()
        {
            //header help boxes
            EditorGUILayout.HelpBox("Previews generated with this window are in low quality because they are based on Unity asset previews.", MessageType.Warning);
            EditorGUILayout.HelpBox("To create your preview drag/select an object to use.", MessageType.Info);

            //create a field that accept only asset objects
            obj = (GameObject)EditorGUILayout.ObjectField("Object:", obj, typeof(GameObject), false);
            if (obj != null) CreatePreview();

            GUILayout.Label("Save path: " + ObjectPreview.savePath);

            GUILayout.Space(10);

            //button to create preview
            if (GUILayout.Button("Create Preview"))
            {
                ObjectPreview.CreateAndSaveAssetPreview(obj);
            }
            EditorGUILayout.HelpBox("If there just an asset with the same name, it will be overwriden!", MessageType.Warning);

            //show texture preview before save
            if (currentPreview != null)
            {
                EditorGUI.DrawPreviewTexture(new Rect(TextureMargin, VerticalSize - HorizontalSize - TextureMargin, HorizontalSize - (2 * TextureMargin), HorizontalSize), currentPreview);
            }
        }


        /// <summary>
        /// Create the image preview
        /// </summary>
        void CreatePreview()
        {
            currentPreview = AssetPreview.GetAssetPreview(obj);
        }




    }
}
