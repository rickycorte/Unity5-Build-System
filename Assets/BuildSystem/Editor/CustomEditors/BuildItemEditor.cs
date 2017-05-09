using UnityEngine;
using UnityEditor;
using BuildSystem;

/// <summary>
/// Custom Editor for Build Item
/// </summary>
[CustomEditor(typeof(BuildItem))]
public class BuildItemEditor : Editor {

    public override void OnInspectorGUI()
    {

        BuildItem obj = (BuildItem)target;
        EditorGUILayout.HelpBox("If you add the prefab with empty fields below, this script will help you to fill the others", MessageType.Info);
        EditorGUILayout.LabelField("Prefab To Spawn:");
        var temp = (GameObject)EditorGUILayout.ObjectField("", obj.Prefab, typeof(GameObject), false);

        //check if prefab has changed
        if (temp != obj.Prefab)
        {
            obj.Prefab = temp;

            if (temp == null) return;

            //auto setup with empty
            if (string.IsNullOrEmpty( obj.Name.Trim()))
                obj.SetAutomaticName(temp);
            if (obj.UiPicture == null)
                obj.SetAutomaticPreview(temp);
            if (obj.ghostMaterial == null)
                obj.SetAutomaticMaterial();

            obj.CreateGhost();
            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Display Name: ");
        obj.Name = EditorGUILayout.TextField(obj.Name);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Sprite To Show In UI:");
        obj.UiPicture = (Sprite)EditorGUILayout.ObjectField("", obj.UiPicture, typeof(Sprite), false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ghost material:");
        EditorGUILayout.HelpBox("If you don't want to set ghost material for every object that you create, use the tool under Build System menu", MessageType.Info);
        var mat = (Material)EditorGUILayout.ObjectField("", obj.ghostMaterial, typeof(Material), false);
        if (obj.ghostMaterial != mat)
        {
            obj.ghostMaterial = mat;
            obj.CreateGhost();
        }

        EditorUtility.SetDirty(target);

    }

}
