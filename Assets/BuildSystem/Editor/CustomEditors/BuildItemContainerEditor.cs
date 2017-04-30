using UnityEngine;
using UnityEditor;
using BuildSystem;
using UnityEditorInternal;

/// <summary>
/// Custom Editor for Build Items Container
/// </summary>
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

    /// <summary>
    /// Draw reoderable list
    /// </summary>
    public override void OnInspectorGUI()
    {
        reorderList.DoLayoutList();

        if (GUILayout.Button("Regenerate all ghosts"))
        {
            GenerateAllGhostsInList();
        }
    }

    /// <summary>
    /// Draw list header
    /// </summary>
    /// <param name="rect"></param>
    void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Build Item List:");
    }

    /// <summary>
    /// Draw list item
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index"></param>
    /// <param name="active"></param>
    /// <param name="focused"></param>
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

    /// <summary>
    /// Add item to list
    /// </summary>
    /// <param name="ls"></param>
    void AddItem(ReorderableList ls)
    {
        myListBlock.items.Add(null);
        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// (Handler) Remove item from list
    /// </summary>
    /// <param name="ls"></param>
    void RemoveItem(ReorderableList ls)
    {
        RmItem(ls.index);
    }

    /// <summary>
    /// Remove item by index
    /// </summary>
    /// <param name="index"></param>
    void RmItem(int index)
    {
        myListBlock.items.RemoveAt(index);
        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// Force ghost regeneration of every object in list
    /// </summary>
    void GenerateAllGhostsInList()
    {
        for (int i = 0; i < myListBlock.items.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Processing", "Generating ghosts: " + (i + 1) + "/" + myListBlock.items.Count, (float)i + 1 / (float)myListBlock.items.Count);
            myListBlock.items[i].CreateGhost();           
        }

        EditorUtility.ClearProgressBar();
    }

}
