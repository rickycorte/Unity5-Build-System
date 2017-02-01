using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Se con invio viene premuto il bottone successivo basta andare sull oggetto EventSystem > Standalone Input Module e rimuovere Submit Button

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Animator))]
public class BuilderUI : MonoBehaviour {


    /****************************************************
    * Editor Interface
    * *************************************************/

    [Header("References")]

    [Tooltip("Parent of all the buttons")]
    [SerializeField] Transform ButtonsParent;

    [Tooltip("Button prefab to show an item")]
    [SerializeField] BuilderObjectUI objPrefab;

    [SerializeField] Text CollapseMenuButtonText;

    /****************************************************
    * Variables & Components
    * *************************************************/

    CanvasGroup cv;
    Animator anim;

    bool isMenuCollapsed = false;

    BuilderObjectUI selectedObject;

    /****************************************************
    * initialization
    * *************************************************/

    // Use this for initialization
    void Start () {
        cv = GetComponent<CanvasGroup>();
        anim = GetComponent<Animator>();
        cv.interactable = false;
        cv.blocksRaycasts = false;
        cv.alpha = 0;

        if (objPrefab == null) Debug.LogError("Missing objPrefab, please assign it!");
        if (ButtonsParent == null) Debug.LogError("Missing ButtonsParent, please assign it!");
    }

    /****************************************************
    * Activation
    * *************************************************/

    //apri/chiudi il menu
    public void ToggleMenu()
    {
        ToggleMenu(!cv.blocksRaycasts);
    }

    public void ToggleMenu(bool val)
    {
        anim.SetBool("isOpen", val);
        cv.interactable = val;
        cv.blocksRaycasts = val;
        SetIsCollapsed(false);
    }



    /****************************************************
    * Extrernal Actions
    * *************************************************/

    //collape the menu but not toggle it so ic can be reopened
    public void CollapseMenu()
    {
        isMenuCollapsed = !isMenuCollapsed;
        SetIsCollapsed(isMenuCollapsed);
    }

    //highlight the selected item and deselect the current one
    public void SetSelectedItem(BuilderObjectUI bo)
    {
        if (selectedObject != null) selectedObject.Select(false);

        bo.Select(true);
        selectedObject = bo;
         
    }


    /****************************************************
    * UI generation
    * *************************************************/

    //crea tutti i bottoni per selezionare i vari oggetti
    public void Populatemenu(ScriptableObjectContainer container, ObjectSelector selector)
    {
        for (int i = 0; i < container.items.Count; i++)
        {
            var item = Instantiate(objPrefab,ButtonsParent).GetComponent<BuilderObjectUI>();
            item.SetUp(container.items[i]);
            item.AddButtonListner(selector.UseItem, i);

            if (i == 0) // select the first button
            {
                SetSelectedItem(item);
            }
        }
    }

    //collapse the menu without disabling it
    void SetIsCollapsed(bool val)
    {
        isMenuCollapsed = val;
        anim.SetBool("isCollapsed", val);
        if (CollapseMenuButtonText != null) CollapseMenuButtonText.text = (val) ? ">>" : "<<";
    }
}
