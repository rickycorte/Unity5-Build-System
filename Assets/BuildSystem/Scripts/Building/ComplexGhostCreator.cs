using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildSystem
{

    public class ComplexGhostCreator : MonoBehaviour
    {

        List<Material[]> oldMats = new List<Material[]>();

        MeshRenderer[] renders;

        //applica il ghost a un oggetto composto da piu renderer
        public void CreateComplexGhost(Transform objRoot, Material ghostMat)
        {
            if (objRoot == null || ghostMat == null) return;

            renders = objRoot.GetComponentsInChildren<MeshRenderer>(); // recupera tutti i renderer
            for (int i = 0; i < renders.Length; i++)
            {
                Material[] mats = renders[i].materials; // recupra i vecchi materiali
                oldMats.Add(mats); // salva i vecchi materiali
                renders[i].materials = ghostMatArray(mats.Length, ghostMat); // imposta i nuovi materiali da ghost

                renders[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            Debug.Log("Created complex ghost");

        }


        //crea un array di materiali ghost da usare su un oggetto
        Material[] ghostMatArray(int size, Material ghostMaterial)
        {
            Material[] ghosts = new Material[size];
            for (int i = 0; i < ghosts.Length; i++)
            {
                ghosts[i] = ghostMaterial;
            }

            return ghosts;
        }

        //riapplica i materiali all'oggetto. NOTA: l'oggetto deve essere il medesimo passato alla funzione CreateComplexGhost
        public void RemoveComplexGhost()
        {
            if (renders == null) return;
            for (int i = 0; i < renders.Length; i++)
            {
                renders[i].materials = oldMats[i]; // applica i vecchi materiali
                renders[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            oldMats.Clear();

            Debug.Log("Removed complex ghost");
        }

    }
}
