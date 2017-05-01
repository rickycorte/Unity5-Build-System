using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// Helper class used to create complex ghost objects (Multiple mesh renderers)
    /// </summary>
    [System.Obsolete("This class is no longer used in Build System")]
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

        /// <summary>
        /// Create a complex ghost (more meshes)
        /// </summary>
        /// <param name="objRoot">Parent of all the renderers</param>
        /// <param name="ghostMat">Material to apply to all renderers</param>
        public void CreateComplexGhost(Transform objRoot, Material ghostMat)
        {
            if (ghostMat == null) return; //don't do anything, maybe you don't want a ghost object
            if (objRoot == null)
            {
                Debug.LogError("Please send a valid object to perform the operation! Aborting creation.");
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


        /// <summary>
        /// Apply old materials to the renderes cached after create
        /// </summary>
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

        /// <summary>
        /// Clear chached values
        /// </summary>
        public void ClearCache()
        {           
            oldMats.Clear();
            oldShadows = null;
        }

        /****************************************************
        * Helpers
        * *************************************************/

        /// <summary>
        /// create an array of ghost materials
        /// </summary>
        /// <param name="size">Lenght of the array</param>
        /// <param name="ghostMaterial">Material to use in the array</param>
        /// <returns></returns>
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
