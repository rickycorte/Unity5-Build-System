using UnityEngine;
using UnityEditor;
using BuildSystem;
using UnityEditorInternal;

[CustomEditor(typeof(ScriptableObjectContainer))]
public class ScriptableObjectContainerEditor : Editor {

    ReorderableList reorderList;

    ScriptableObjectContainer myListBlock{ get { return ((ScriptableObjectContainer)target); } }

    void OnEnable()
    {
        reorderList = new ReorderableList(myListBlock.items, typeof(ScriptableObjectToPlace),true,true,true,true);

        reorderList.drawHeaderCallback += DrawHeader;
        reorderList.drawElementCallback += DrawElement;
        reorderList.onAddCallback += AddItem;
        reorderList.onRemoveCallback += RemoveItem;
    }

    private void OnDisable()
    {
        reorderList.drawHeaderCallback -= DrawHeader;
        reorderList.drawElementCallback -= DrawElement;
        reorderList.onAddCallback -= AddItem;
        reorderList.onRemoveCallback -= RemoveItem;
    }

    public override void OnInspectorGUI()
    {
        reorderList.DoLayoutList();
    }

    void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Item List:");
    }

    void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        ScriptableObjectToPlace item = myListBlock.items[index];
        EditorGUI.BeginChangeCheck();

        Rect r = rect;
        r.width -= 20;
        myListBlock.items[index] = (ScriptableObjectToPlace) EditorGUI.ObjectField(r, item, typeof(ScriptableObjectToPlace), false);
        rect.x += r.width;
        rect.width = 20;
        if (GUI.Button(rect,"X"))
        {
            myListBlock.items.RemoveAt(index);
            EditorUtility.SetDirty(target);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }

    }

    void AddItem(ReorderableList ls)
    {
        myListBlock.items.Add(null);
        EditorUtility.SetDirty(target);
    }

    void RemoveItem(ReorderableList ls)
    {
        myListBlock.items.RemoveAt(ls.index);
        EditorUtility.SetDirty(target);
    }

}
