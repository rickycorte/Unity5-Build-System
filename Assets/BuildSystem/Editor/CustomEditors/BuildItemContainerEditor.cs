using UnityEngine;
using UnityEditor;
using BuildSystem;
using UnityEditorInternal;

[CustomEditor(typeof(BuildItemContainer))]
public class BuildItemContainerEditor : Editor {

    ReorderableList reorderList;

    BuildItemContainer myListBlock{ get { return ((BuildItemContainer)target); } }

    void OnEnable()
    {
        // create reoderable list and setup handlers
        reorderList = new ReorderableList(myListBlock.items, typeof(BuildItem),true,true,true,true);

        reorderList.drawHeaderCallback += DrawHeader;
        reorderList.drawElementCallback += DrawElement;
        reorderList.onAddCallback += AddItem;
        reorderList.onRemoveCallback += RemoveItem;
    }

    private void OnDisable()
    {
        //remove handlers
        reorderList.drawHeaderCallback -= DrawHeader;
        reorderList.drawElementCallback -= DrawElement;
        reorderList.onAddCallback -= AddItem;
        reorderList.onRemoveCallback -= RemoveItem;
    }

    //draw reoderable list
    public override void OnInspectorGUI()
    {
        reorderList.DoLayoutList();
    }

    //draw list header
    void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Build Item List:");
    }

    //draw list item
    void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        EditorGUI.BeginChangeCheck();

        //draw object selection 
        Rect r = rect;
        r.width -= 20; // remove the size of the X button
        myListBlock.items[index] = (BuildItem) EditorGUI.ObjectField(r, myListBlock.items[index], typeof(BuildItem), false);

        //draw X button to fast remove item
        rect.x += r.width; // move to end of line
        rect.width = 20;
        if (GUI.Button(rect,"X"))
        {
            RmItem(index);
        }

        //save changes
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }

    }

    //add item to list
    void AddItem(ReorderableList ls)
    {
        myListBlock.items.Add(null);
        EditorUtility.SetDirty(target);
    }

    //remove item from list (Handler)
    void RemoveItem(ReorderableList ls)
    {
        RmItem(ls.index);
    }

    //remove item by index
    void RmItem(int index)
    {
        myListBlock.items.RemoveAt(index);
        EditorUtility.SetDirty(target);
    }

}
