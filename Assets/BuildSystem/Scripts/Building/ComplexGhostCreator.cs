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

        MeshRenderer[] renders;

        UnityEngine.Rendering.ShadowCastingMode[] oldShadows;


        /****************************************************
        * GhostCreation
        * *************************************************/

        //create a complex ghost (more meshes)
        public void CreateComplexGhost(Transform objRoot, Material ghostMat)
        {
            if (objRoot == null || ghostMat == null) return;

            renders = objRoot.GetComponentsInChildren<MeshRenderer>(); // get all renderers
            oldShadows = new UnityEngine.Rendering.ShadowCastingMode[renders.Length];
            for (int i = 0; i < renders.Length; i++)
            {
                Material[] mats = renders[i].materials; // get current material
                oldMats.Add(mats); // seve them
                renders[i].materials = ghostMatArray(mats.Length, ghostMat); // replace materials with ghost

                oldShadows[i] = renders[i].shadowCastingMode; //recover old shadows
                renders[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // remove shadows
            }

            Debug.Log("Created complex ghost");

        }


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

        //reapply old materials to the renderes cached after create
        public void RemoveComplexGhost()
        {
            if (renders == null) return;
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].materials = oldMats[i]; // apply old material
                renders[i].shadowCastingMode = oldShadows[i]; // apply old shadows
            }
            oldMats.Clear();

            Debug.Log("Removed complex ghost");
        }

    }
}
