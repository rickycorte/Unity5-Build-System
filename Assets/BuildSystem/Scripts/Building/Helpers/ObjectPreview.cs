#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading;
#endif

namespace BuildSystem
{
    /// <summary>
    /// [EDITOR ONLY] Helper class to create and save automatic object previews
    /// </summary>
    public sealed class ObjectPreview
    {
#if UNITY_EDITOR

        public static readonly string savePath = "Assets/BuildSystem/Data/Previews";

        /// <summary>
        /// [EDITOR ONLY] Create and save a preview for an object
        /// </summary>
        /// <param name="g">Object to use to create preview</param>
        public static string CreateAndSaveAssetPreview(GameObject g)
        {
            if (g == null)
            {
                Debug.LogError("Can't create preview for null object");
                return "";
            }

            Texture2D preview = AssetPreview.GetAssetPreview(g);

            //wait untill unity loads preview
            int tm = 0;
            while (preview == null)
            {
                Thread.Sleep(100);
                preview = AssetPreview.GetAssetPreview(g);
                tm += 100;
                if (tm >= 3000) //3 sec countdown
                    break;
            }

            if (preview == null)
            {
                Debug.LogError("Unable to create preview for object: " + g.name);
                return "";
            }

            CreateSaveFolder();

            //encode to png and then save to assets
            var bytes = preview.EncodeToPNG();
            string name = g.name + ".png";
            string savePos = savePath + "/" + name;

            if (File.Exists(name)) File.Delete(name);
            File.WriteAllBytes(savePos, bytes);
            Debug.Log("Saved preview: " + name);

            //refresh assets
            AssetDatabase.Refresh();


            //change from texture to sprite
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(savePos);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer);
            AssetDatabase.ImportAsset(savePos);

            AssetDatabase.Refresh();

            return savePos;

        }

        /// <summary>
        /// [EDITOR ONLY] Check if the save directory exitst. If no creates it
        /// </summary>
        static void CreateSaveFolder()
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log("Created directory: " + savePath);
                Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }
        }

#endif
    }
}
