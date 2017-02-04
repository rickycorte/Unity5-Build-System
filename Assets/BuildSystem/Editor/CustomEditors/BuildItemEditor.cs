using UnityEngine;
using UnityEditor;
using BuildSystem;

[CustomEditor(typeof(BuildItem))]
public class BuildItemEditor : Editor {

    public override void OnInspectorGUI()
    {

        BuildItem obj = (BuildItem)target;

        EditorGUILayout.LabelField("Display Name: ");
        obj.Name = EditorGUILayout.TextField(obj.Name);

        EditorGUILayout.LabelField("Prefab To Spawn:");
        obj.Prefab = (GameObject)EditorGUILayout.ObjectField("", obj.Prefab, typeof(GameObject), false);

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

        EditorUtility.SetDirty(target);

    }

}
