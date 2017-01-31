using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuilderObjectUI : MonoBehaviour {

    public delegate void listner(int index);

    Button btn;

    [Header("Item Properties")]
    [SerializeField] Text itemName;
    [SerializeField] Image itemImage;

    //imposta il callback da chimare quando premuto
    public void AddButtonListner(listner ls, int myIndex)
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(()=> { ls(myIndex); });     
    }

    //imposta immagine e testo
    public void SetUp(ScriptableObjectToPlace o)
    {
        if (itemName != null) itemName.text = o.Name;
        if (itemImage != null) itemImage.sprite = o.UiPicture;
    }

}
