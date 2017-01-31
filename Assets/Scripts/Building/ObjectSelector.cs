using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectPlacer))]
public class ObjectSelector : MonoBehaviour {

    /****************************************************
    * Editor Interface
    * *************************************************/

    [Header("Containers")]

    [Tooltip("List of spawnable objects")]
    [SerializeField] ScriptableObjectContainer objContainer;

    [Header("UI")]

    [Tooltip("UI menu that you want to use to display the spawnable objects")]
    [SerializeField] GameObject BuilderMenuPrefab;


    /****************************************************
    * Variables & Components
    * *************************************************/

    ObjectPlacer objPlacer;
    BuilderUI builderUI;

    KeyCode activeKey;

    bool isActive = true;

    bool isOpen = false;

    /****************************************************
    * Initialization
    * *************************************************/

    private void Start()
    {
        objPlacer = GetComponent<ObjectPlacer>();
        activeKey = objPlacer.TOGGLEKEY;
        if (BuilderMenuPrefab == null) Debug.LogError("Missing BuilderMenuPrefab, please assign it!");
        builderUI = Instantiate(BuilderMenuPrefab).GetComponentInChildren<BuilderUI>();
        builderUI.Populatemenu(objContainer, this);

        objPlacer.SetObjectToPlaceNOGHOST(objContainer.items[0].Prefab); // imposta come oggetto di default il primo
    }

    /****************************************************
    * Activation
    * *************************************************/

    //Active the script input handler
    public void Enable(bool val)
    {
        isActive = val;
        Toggle(false);
        GetComponent<ObjectPlacer>().Enable(isActive);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        ToggleUI(isOpen);
        objPlacer.Toggle(isOpen);
    }

    public void Toggle(bool val)
    {
        isOpen = val;
        ToggleUI(isOpen);
        objPlacer.Toggle(isOpen);
    }

    /****************************************************
    * Input
    * *************************************************/

    private void Update()
    {
        if (Input.GetKeyDown(activeKey) && isActive)
        {
            ToggleUI();
            isOpen = !isOpen;
        }
    }

    private void FixedUpdate()
    {
        if (isOpen)
        {
            isOnUI();
        }
    }

    /****************************************************
    * UI Control
    * *************************************************/

    //controlla dove e' il puntatore
    void isOnUI()
    {
        objPlacer.SetIsMouseNotOnUI(
          !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() // controlla che il mouse non sia sopra a dell'ui
        );

    }

    //attiva o meno l'ui assegnata da inspector
    void ToggleUI()
    {
        if (builderUI != null)
        {
            builderUI.ToggleMenu();
        }
        else Debug.LogError("Missing UI for ObjectSelector!");
    }

    //attiva o meno l'ui assegnata da inspector
    void ToggleUI(bool val)
    {
        if (builderUI != null)
        {
            builderUI.ToggleMenu(val);
        }
        else Debug.LogError("Missing UI for ObjectSelector!");
    }

    /****************************************************
    * GObject Placer Setup
    * *************************************************/

    //callback dell ui per impostare l 'item
    public void UseItem(int index)
    {
        if (index >= 0  && index < objContainer.items.Count)
        {
            objPlacer.SetObjectToPlace(objContainer.items[index].Prefab);
        }
        else Debug.LogError("No item for index: " + index);
    }


}
