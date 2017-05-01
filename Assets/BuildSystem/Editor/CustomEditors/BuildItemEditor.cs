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

        EditorGUILayout.LabelField("Display Name: ");
        obj.Name = EditorGUILayout.TextField(obj.Name);

        EditorGUILayout.LabelField("Prefab To Spawn:");
        var temp = (GameObject)EditorGUILayout.ObjectField("", obj.Prefab, typeof(GameObject), false);

        //check if prefab has changed
        if (temp != obj.Prefab)
        {
            obj.Prefab = temp;
            obj.CreateGhost();
        }

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
