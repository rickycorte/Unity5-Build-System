﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{

    [RequireComponent(typeof(ObjectPlacer))]
    public class ObjectSelector : MonoBehaviour
    {

        /****************************************************
        * Editor Interface
        * *************************************************/

        [Header("Containers")]

        [Tooltip("List of spawnable objects")]
        [SerializeField] BuildItemContainer buildObjectList;

        [Header("UI")]

        [Tooltip("UI menu that you want to use to display the spawnable objects")]
        [SerializeField]
        GameObject BuilderMenuPrefab;

        [Header("Input Settings")]

        [SerializeField]
        KeyCode CollapseMenuKey = KeyCode.None;


        /****************************************************
        * Variables & Components
        * *************************************************/

        ObjectPlacer objPlacer;
        IItemSelectionUI builderUI;

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
            if (BuilderMenuPrefab == null)
            {
                Debug.LogError("Missing BuilderMenuPrefab, please assign it!");
                return;
            }
            if (buildObjectList == null)
            {
                Debug.LogError("Missing buildObjectList, please assign it!");
                return;
            }

            
            builderUI = Instantiate(BuilderMenuPrefab).GetComponentInChildren<IItemSelectionUI>(); //create the ui
            if (builderUI == null)
            {
                Debug.LogError("Please make sure that the UI prefab has one script that implements: IItemSlectionUI interface");
                return;
            }

            builderUI.Populatemenu(buildObjectList, this); //populate it

            if (!buildObjectList.isValid())
            {
                Debug.LogError("Please add some some Items to list");
                return;
            }

            objPlacer.SetObjectToPlace(buildObjectList.items[0]); //use the first item as default and set it
        }

        /****************************************************
        * Activation
        * *************************************************/

        /// <summary>
        /// Active the script input handler
        /// </summary>
        /// <param name="val">Is active</param>
        public void Enable(bool val)
        {
            isActive = val;
            Toggle(false);
            GetComponent<ObjectPlacer>().Enable(isActive);
        }

        /// <summary>
        /// Toggle Object Selector and Object Placer
        /// </summary>
        public void Toggle()
        {
            isOpen = !isOpen;
            ToggleUI(isOpen);
            objPlacer.Toggle(isOpen);
        }

        /// <summary>
        /// Set active state of Toggle Object Selector and Object Placer
        /// </summary>
        /// <param name="val"></param>
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
            //handle menu open button press
            if (Input.GetKeyDown(activeKey) && isActive)
            {
                ToggleUI();
                isOpen = !isOpen;
            }
            //handle collpase button press
            if (Input.GetKeyDown(CollapseMenuKey) && isActive)
            {
                if (builderUI != null) builderUI.CollapseMenu();
            }
        }

        private void FixedUpdate()
        {
            if (isOpen)
            {
                objPlacer.SetIsMouseNotOnUI( !isMouseOnUI() );
            }
        }

        /****************************************************
        * UI Control
        * *************************************************/

        /// <summary>
        /// Check if the mouse is over UI or not
        /// </summary>
        /// <returns></returns>
        bool isMouseOnUI()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                Debug.LogError("Please create an eventSytem in the scene!");
                return true;
            }

            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); // controlla che il mouse non sia sopra a dell'ui
        }

        /// <summary>
        /// Toggle Object Selection UI menu
        /// </summary>
        void ToggleUI()
        {
            if (builderUI != null)
            {
                builderUI.ToggleMenu();
            }
            else Debug.LogError("Missing UI for ObjectSelector!");
        }

        /// <summary>
        /// Set active state of Object Selection UI menu
        /// </summary>
        /// <param name="val">Is active</param>
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

        /// <summary>
        /// UI callback to set the desired item in Object Placer
        /// </summary>
        /// <param name="index">Index of the item to select</param>
        public void UseItem(int index)
        {
            if (index >= 0 && index < buildObjectList.items.Count)
            {
                objPlacer.SetObjectToPlaceAndCreateGhost(buildObjectList.items[index]);
            }
            else Debug.LogError("No item for index: " + index);
        }


    }
}
