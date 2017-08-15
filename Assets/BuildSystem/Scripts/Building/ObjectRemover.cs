using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using cakeslice;

namespace BuildSystem
{

    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    * DISCLAIMER
    * 
    * The outline scripts and shaders are not mine, they are provided by José Guerreiro
    * https://github.com/cakeslice/Outline-Effect
    * https://www.assetstore.unity3d.com/en/#!/content/78608
    * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */


    /// <summary>
    /// Component used to remove objects from the scene
    /// </summary>
    public class ObjectRemover : MonoBehaviour {

        public Camera cam;

        //**********************************************************************************************
        [Header("Hit Settings")]
        [Tooltip("Layers to use to find object to remove")]
        
        public LayerMask hitLayers;

        [Tooltip("Tags of objects what will be ignored even if they are part of hit layers")]
        [SerializeField] List<string> ignoredTags;

        [SerializeField] float maxHitDistance = 10;

        //**********************************************************************************************
        [Header("Behaviour settings")]
        public bool removeInScreenCenter = false;

        [Tooltip("When turned on this script will disalbe Object placer. Note: object placer must be next to this script")]
        public bool shouldDisablePlacer = true;

        //**********************************************************************************************
        [Header("Outline")]
        [Tooltip("Use ouline to display selected item")]
        public bool useOutline = true;

        public bool useCustomOutlineColor = true;

        public Color outlineColor = Color.red;

        //**********************************************************************************************
        [Header("Input")]
        public KeyCode Toggle_Key = KeyCode.R;
        public KeyCode Delete_key = KeyCode.Mouse0;


        ObjectPlacer object_placer;
        ObjectSelector object_selector;


        bool isEnabled = false;

        GameObject selected_object;

        Transform camT;

        /****************************************************
        * Public Interface
        * *************************************************/

        /// <summary>
        /// Turn on/off this script by code
        /// </summary>
        /// <param name="val">Active yes/no</param>
        public void Activate(bool val)
        {
            isEnabled = val;

            //disable placer if active and the user what to disable it automatically
            if (val && shouldDisablePlacer)
            {
                if(object_placer != null) object_placer.Toggle(false);
                if(object_selector != null) object_selector.Toggle(false);
            }

            //deselect object when disable
            if (!val) ResetSelectedObject();
        }


        /// <summary>
        /// Set the outline color of objects
        /// </summary>
        /// <param name="c">Outline color</param>
        public void SetOutlineColor(Color c)
        {
            var o = cam.GetComponent<OutlineEffect>();
            if (o != null) o.lineColor0 = c;
            else Debug.LogError("Unable to change outline color, missing component");
        }

        /// <summary>
        /// Add a tag to ignored tags
        /// </summary>
        /// <param name="tag">Tag to add</param>
        public void AddIgnoreTag(string tag)
        {
            ignoredTags.Add(tag);
        }

        /// <summary>
        /// Delete a tag from ignored tags
        /// </summary>
        /// <param name="tag">Tag to remove</param>
        public void RemoveIgnoreTag(string tag)
        {
            if (ignoredTags.Contains(tag))
                ignoredTags.Remove(tag);
        }

        /// <summary>
        /// Remove all ignored tags
        /// </summary>
        public void ResetIgnoreTags()
        {
            ignoredTags.Clear();
        }


        /****************************************************
        * Unity Functions
        * *************************************************/

        // Use this for initialization
        void Start()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) Debug.LogError("Missing Camera, please add one");

            camT = cam.transform;

            //add outline component to camera if missing
            OutlineEffect of = cam.GetComponent<OutlineEffect>();
            if(of == null)
                of = cam.gameObject.AddComponent<OutlineEffect>();

            //set outline color if the user want to
            if (useCustomOutlineColor)
                of.lineColor0 = outlineColor;

            object_placer = GetComponent<ObjectPlacer>();
            object_selector = GetComponent<ObjectSelector>();
        }


        // Update is called once per frame
        void Update()
        {

            //active remover
            if (Input.GetKeyDown(Toggle_Key))
            {
                Activate(!isEnabled);
            }

            if (isEnabled)
            {
                UpdateSelectedObject();

                if (Input.GetKeyDown(Delete_key))
                {
                    deleteSelectedObject();
                }
            }
        }

        /****************************************************
        * Remover Core Functions
        * *************************************************/

        /// <summary>
        /// Select an object to be removed if possible
        /// </summary>
        void UpdateSelectedObject()
        {
            Ray r;
            //cast ray to sceen center
            if (removeInScreenCenter)
                r = new Ray(camT.position, camT.forward);
            else // use mouse as selector
                r = cam.ScreenPointToRay(Input.mousePosition);

            //search if we hit something
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, maxHitDistance, hitLayers))
            {
                GameObject g = hit.transform.gameObject;
                //check if object should be ignored
                if (!hasIgnoreTag(g.tag))
                {
                    if (selected_object != g) // new object found, we have to reset the old one and edit th new one
                    {
                        ResetSelectedObject();
                        //add selection outline
                        g.AddComponent<Outline>();
                        //set object as selected
                        selected_object = g;
                    }
                }
            }
            else
            {
                ResetSelectedObject();
            }

        }

        /// <summary>
        /// Delete the selected object
        /// </summary>
        void deleteSelectedObject()
        {
            if (selected_object != null)
            {
                Debug.Log("Removed " + selected_object.name);
                Destroy(selected_object);
            }
                
        }

        /// <summary>
        /// Reset selected object if there is one
        /// </summary>
        void ResetSelectedObject()
        {
            if (selected_object != null)
            {
                //remove outline component to deselect
                var outline = selected_object.GetComponent<Outline>();
                if (outline != null)
                    Destroy(outline);

                //dereference object
                selected_object = null;
            }
        }

        /// <summary>
        /// Check if the object tag is in the ignored list
        /// </summary>
        /// <param name="obj_tag">Tag to check</param>
        /// <returns></returns>
        bool hasIgnoreTag(string obj_tag)
        {
            for (int i = 0; i < ignoredTags.Count; i++)
            {
                if (obj_tag == ignoredTags[i]) return true;
            }
            return false;
        }

    }
}
