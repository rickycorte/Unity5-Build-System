using UnityEngine;
using UnityEditor;
using System.IO;

namespace BuildSystem
{

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
            //EditorWindow.GetWindow(typeof(ObjectPreviewWindow),true,"Object Preview");
            GetWindowWithRect(typeof(ObjectPreviewWindow), new Rect(Screen.width / 2, 200, HorizontalSize, VerticalSize), true, "Object Preview Generator");
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Previews generated with this window are in low quality because they are based on Unity asset previews.", MessageType.Warning);
            EditorGUILayout.HelpBox("To create your preview drag/select an object to use.", MessageType.Info);

            obj = (GameObject)EditorGUILayout.ObjectField("Object:", obj, typeof(GameObject), false);
            if (obj != null) CreatePreview();

            GUILayout.Label("Save path: " + savePath);

            GUILayout.Space(10);

            if (GUILayout.Button("Create Preview"))
            {
                SavePreview();
            }
            EditorGUILayout.HelpBox("If there just an asset with the same name, it will be overwriden!", MessageType.Warning);

            if (currentPreview != null)
            {
                EditorGUI.DrawPreviewTexture(new Rect(TextureMargin, VerticalSize - HorizontalSize - TextureMargin, HorizontalSize - (2 * TextureMargin), HorizontalSize), currentPreview);
            }
        }


        //save the current preview to hdd
        void SavePreview()
        {
            if (currentPreview == null)
            {
                EditorUtility.DisplayDialog("Error", "Fist assign an object to create a preview!", "ok");
                return;
            }
            CreteSaveFolder();

            var bytes = currentPreview.EncodeToPNG();
            string name = obj.name + ".png";
            if (File.Exists(name)) File.Delete(name);
            File.WriteAllBytes(savePath + "/" + name, bytes);
            Debug.Log("Saved preview: " + name);

            AssetDatabase.Refresh();

        }

        //Create the image preview
        void CreatePreview()
        {
            currentPreview = AssetPreview.GetAssetPreview(obj);
        }

        //check if the save directory exitst. If no create it
        void CreteSaveFolder()
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log("Creating dir");
                Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }
        }
    }
}
