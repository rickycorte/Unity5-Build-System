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

        EditorGUILayout.HelpBox("Complex Mesh indicate that the prefab is composed by multiple Meshes.\n"
            +"Note: ObjectPlacer will use the first MeshRenderer to align and place the prefab on the surface",MessageType.Info);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("isComplexMesh: ");
        obj.isComplexMesh = EditorGUILayout.Toggle(obj.isComplexMesh);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Ghost material:");
        EditorGUILayout.HelpBox("If you don't want to set ghost material for every object that you create, use the tool under Build System menu", MessageType.Info);
        obj.ghostMaterial = (Material)EditorGUILayout.ObjectField("", obj.ghostMaterial, typeof(Material), false);

        EditorUtility.SetDirty(target);

    }

}
