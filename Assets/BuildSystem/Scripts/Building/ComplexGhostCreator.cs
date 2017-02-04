using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{

    public class ComplexGhostCreator : MonoBehaviour
    {

        /****************************************************
        * Variables
        * *************************************************/

        List<Material[]> oldMats = new List<Material[]>();

        MeshRenderer[] meshRenderers;

        UnityEngine.Rendering.ShadowCastingMode[] oldShadows;


        /****************************************************
        * Ghost Creation/Destruction
        * *************************************************/

        //create a complex ghost (more meshes)
        public void CreateComplexGhost(Transform objRoot, Material ghostMat)
        {
            if (objRoot == null || ghostMat == null)
            {
                Debug.LogError("CreateComplexGhost can't have null paramteres! Aborting creation.");
                return;
            }

            meshRenderers = objRoot.GetComponentsInChildren<MeshRenderer>(); // get all renderers
            //renders will be keep as a cache to avoid multiple getComponents
            oldShadows = new UnityEngine.Rendering.ShadowCastingMode[meshRenderers.Length]; // create an array to store shadow settings

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                Material[] mats = meshRenderers[i].materials; // get current material
                oldMats.Add(mats); //save them
                meshRenderers[i].materials = ghostMatArray(mats.Length, ghostMat); // replace materials with ghost

                oldShadows[i] = meshRenderers[i].shadowCastingMode; //recover old shadows
                meshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // remove shadows
            }

           // Debug.Log("Created complex ghost");
        }


        //reapply old materials to the renderes cached after create
        public void RemoveComplexGhost()
        {
            if (meshRenderers == null) return;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].materials = oldMats[i]; // apply old material
                meshRenderers[i].shadowCastingMode = oldShadows[i]; // apply old shadows
            }
            ClearCache();

            //Debug.Log("Removed complex ghost");
        }

        //clear chached values
        public void ClearCache()
        {           
            oldMats.Clear();
            oldShadows = null;
        }

        /****************************************************
        * Helpers
        * *************************************************/

        //create an array of ghost materials
        Material[] ghostMatArray(int size, Material ghostMaterial)
        {
            Material[] ghosts = new Material[size];
            for (int i = 0; i < ghosts.Length; i++)
            {
                ghosts[i] = ghostMaterial;
            }

            return ghosts;
        }

    }
}
