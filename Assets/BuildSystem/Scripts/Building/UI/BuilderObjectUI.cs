using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuildSystem
{

    [RequireComponent(typeof(Button))]
    public class BuilderObjectUI : MonoBehaviour
    {

        public delegate void listner(int index);

        Button btn;

        [Header("Border Colors")]
        [SerializeField]
        Color selectColor = Color.green;
        [SerializeField]
        Color notSelectColor = new Color(0, 0, 0, 0);

        [Header("Item Properties")]
        [SerializeField]
        Text itemName;
        [SerializeField]
        Image itemImage;
        [SerializeField]
        Image borderImg;

        //set the click callback
        public void AddButtonListner(listner ls, int myIndex)
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(
                () =>
                {
                    ls(myIndex); // call handler
                var ui = GetComponentInParent<BuilderUI>(); // select this button and deselct the old one
                if (ui != null) ui.SetSelectedItem(this);
                    else Debug.LogError("Missing Builder UI to deselect old button");
                }
                );
        }

        //set image and text
        public void SetUp(ScriptableObjectToPlace o)
        {
            if (itemName != null) itemName.text = o.Name;
            if (itemImage != null) itemImage.sprite = o.UiPicture;
            Select(false);
        }

        //set the color of the border image based on value
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
