﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuildSystem
{
    //If Enter button moves the object selection, edit the Submit button in Edit>Project Settings>Input

    /// <summary>
    /// Build item selection UI menu
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public class BuilderUI : MonoBehaviour, IItemSelectionUI
    {


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

        GridLayoutGroup grid;

        int columns = 5;

        /****************************************************
        * initialization
        * *************************************************/

        void Start()
        {
            cv = GetComponent<CanvasGroup>();
            anim = GetComponent<Animator>();
            cv.interactable = false;
            cv.blocksRaycasts = false;
            cv.alpha = 0;

            if (objPrefab == null) Debug.LogError("Missing objPrefab, please assign it!");
            if (ButtonsParent == null) Debug.LogError("Missing ButtonsParent, please assign it!");

            grid = ButtonsParent.GetComponent<GridLayoutGroup>();
            SetUpGrid();

        }

        /// <summary>
        /// Set the grid item size to best fit the UI
        /// </summary>
        void SetUpGrid()
        {
            if (grid == null) return;

            Vector2 dim = GetComponent<RectTransform>().sizeDelta; //get menu size
            float x = dim.x - grid.spacing.x * columns - grid.padding.right - grid.padding.left; //remove margins and padding
            float size = x / columns;
            //Debug.Log("Canvas is: " + dim.x + " available: " + x + " cell size: " + size);
            grid.cellSize = new Vector2(size, size);
        }

        /****************************************************
        * Activation
        * *************************************************/

        /// <summary>
        /// Toggle menu
        /// </summary>
        public void ToggleMenu()
        {
            ToggleMenu(!cv.blocksRaycasts);
        }

        /// <summary>
        /// Set menu active status
        /// </summary>
        /// <param name="val">Is active</param>
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

        /// <summary>
        /// Collapse the menu but not toggle it so it can be re-opened
        /// </summary>
        public void CollapseMenu()
        {
            isMenuCollapsed = !isMenuCollapsed;
            SetIsCollapsed(isMenuCollapsed);
        }

        /// <summary>
        /// Highlight the selected item and deselect the current one
        /// </summary>
        /// <param name="itemToHightlight">Item to highlight</param>
        public void SetSelectedItem(BuilderObjectUI itemToHightlight)
        {
            if (selectedObject != null) selectedObject.Select(false);

            itemToHightlight.Select(true);
            selectedObject = itemToHightlight;

        }


        /****************************************************
        * UI generation
        * *************************************************/

        /// <summary>
        /// Create the buttons for all the elements
        /// </summary>
        /// <param name="container">Item list</param>
        /// <param name="selector">Object Selector Script referenct for callbacks</param>
        public void Populatemenu(BuildItemContainer container, ObjectSelector selector)
        {

            for (int i = 0; i < container.items.Count; i++)
            {
                var item = Instantiate(objPrefab, ButtonsParent).GetComponent<BuilderObjectUI>();
                //reset position and scale of the instantiated item
                RectTransform rt = item.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                //setup content
                item.SetUp(container.items[i]);
                item.AddButtonListner(selector.UseItem, i);

                if (i == 0) //select the first button
                {
                    SetSelectedItem(item);
                }
            }
        }

        /// <summary>
        /// Collapse the menu without disabling it
        /// </summary>
        /// <param name="val">Collapese state</param>
        void SetIsCollapsed(bool val)
        {
            isMenuCollapsed = val;
            anim.SetBool("isCollapsed", val);
            if (CollapseMenuButtonText != null) CollapseMenuButtonText.text = (val) ? ">>" : "<<";
        }
    }
}
