using UnityEngine;
using UnityEngine.UI;

namespace BuildSystem
{
    /// <summary>
    /// Item preview for Builder UI menu
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BuilderObjectUI : MonoBehaviour
    {

        public delegate void listner(int index);

        Button btn;

        [Header("Border Colors")]
        [SerializeField] Color selectColor = Color.green;
        [SerializeField] Color notSelectColor = new Color(0, 0, 0, 0);

        [Header("Item Properties")]
        [SerializeField] Text itemName;
        [SerializeField] Image itemImage;
        [SerializeField] Image borderImg;

        /// <summary>
        /// Set the click callback
        /// </summary>
        /// <param name="callback">Callback fired when preview is pressed</param>
        /// <param name="myIndex">Item index used for callback</param>
        public void AddButtonListner(listner callback, int myIndex)
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(
                () =>
                {
                    callback(myIndex); //call handler
                var ui = GetComponentInParent<BuilderUI>(); //select this button and deselct the old one
                if (ui != null) ui.SetSelectedItem(this,true); //select the current item (green border)
                    else Debug.LogError("Missing Builder UI to deselect old button");
                }
                );
        }

        /// <summary>
        /// Set image and text of the preview
        /// </summary>
        /// <param name="item">Item used to create set the preview</param>
        public void SetUp(BuildItem item)
        {
            if (itemName != null) itemName.text = item.Name;
            if (itemImage != null) itemImage.sprite = item.UiPicture;
            Select(false);
        }

        /// <summary>
        /// Set the color of the selection border based on the select status
        /// </summary>
        /// <param name="val">Select status</param>
        public void Select(bool val)
        {
            if (borderImg == null) return;
            if (val)
            {
                borderImg.color = selectColor;
            }
            else
            {
                borderImg.color = notSelectColor;
            }
        }

    }
}
