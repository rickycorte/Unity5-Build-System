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

        [Header("Settings")]
        [SerializeField] Text CollapseMenuButtonText;
        [SerializeField] bool collapseAfterSelect = false;

        /****************************************************
        * Variables & Components
        * *************************************************/

        CanvasGroup cv;
        Animator anim;

        bool isMenuCollapsed = false;

        BuilderObjectUI selectedObject;

        GridLayoutGroup grid;

        int columns = 5;

        ObjectSelector selector;

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
            SetIsCollapsed(false,!val);
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
            SetIsCollapsed(isMenuCollapsed,isMenuCollapsed);           
        }

        /// <summary>
        /// Highlight the selected item and deselect the current one
        /// </summary>
        /// <param name="itemToHightlight">Item to highlight</param>
        /// <param name="canCollapse">Can auto-collapse on click</param>
        public void SetSelectedItem(BuilderObjectUI itemToHightlight, bool canCollapse = false)
        {
            if (selectedObject != null) selectedObject.Select(false);

            itemToHightlight.Select(true);
            selectedObject = itemToHightlight;

            if (collapseAfterSelect && canCollapse)
            {
                CollapseMenu();              
            }

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
            this.selector = selector;

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
        /// <param name="val">Collapese state</para>
        /// <param name="callbackEventValue">Value passed to the selector for event</param>
        void SetIsCollapsed(bool val,bool callbackEventValue)
        {
            isMenuCollapsed = val;
            anim.SetBool("isCollapsed", val);
            if (CollapseMenuButtonText != null) CollapseMenuButtonText.text = (val) ? ">>" : "<<";
            selector.CastOnCollapseEvent(callbackEventValue);
        }

        /// <summary>
        /// Get collapse state of the menu
        /// </summary>
        /// <returns></returns>
        public bool isCollapsed()
        {
            return isMenuCollapsed;
        }

    }
}