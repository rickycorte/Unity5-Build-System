using UnityEngine;
using UnityEditor;
using System.IO;

namespace BuildSystem
{
    /// <summary>
    /// Helper window that creates png previews of prefabs in low resolution using Unity asset preview
    /// </summary>
    public class ObjectPreviewWindow : EditorWindow
    {

        GameObject obj;
        string savePath = "Assets/Data/Previews";

        Texture2D currentPreview;

        static int HorizontalSize = 300;
        static int VerticalSize = 510;
        int TextureMargin = 5;

        [MenuItem("Builder System/Create Object Preview")]
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

            GUILayout.Label("Save path: " + savePath);

            GUILayout.Space(10);

            //button to create preview
            if (GUILayout.Button("Create Preview"))
            {
                SavePreview();
            }
            EditorGUILayout.HelpBox("If there just an asset with the same name, it will be overwriden!", MessageType.Warning);

            //show texture preview before save
            if (currentPreview != null)
            {
                EditorGUI.DrawPreviewTexture(new Rect(TextureMargin, VerticalSize - HorizontalSize - TextureMargin, HorizontalSize - (2 * TextureMargin), HorizontalSize), currentPreview);
            }
        }


        /// <summary>
        /// Save the current preview to hdd
        /// </summary>
        void SavePreview()
        {
            if (currentPreview == null)
            {
                EditorUtility.DisplayDialog("Error", "First assign an object to create a preview!", "ok");
                return;
            }
            CreteSaveFolder();

            //encode to png and then save to assets
            var bytes = currentPreview.EncodeToPNG();
            string name = obj.name + ".png";
            if (File.Exists(name)) File.Delete(name);
            File.WriteAllBytes(savePath + "/" + name, bytes);
            Debug.Log("Saved preview: " + name);

            //refresh assets
            AssetDatabase.Refresh();

        }

        /// <summary>
        /// Create the image preview
        /// </summary>
        void CreatePreview()
        {
            currentPreview = AssetPreview.GetAssetPreview(obj);
        }

        /// <summary>
        /// Check if the save directory exitst. If no creates it
        /// </summary>
        void CreteSaveFolder()
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log("Created directory: "+savePath);
                Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }
        }
    }
}
