using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    //angolo di rotazione massimo della telecamera: da 360 - camAngle a camAngle
    //quindi la rotazione possibile della telecamera e' il doppio di camAngle
    float camAngle = 20f;

    float MoveSpeed = 7f; // velocita di camminata su tutti gli assi
    float RotSpeed = 5f; // velocita' di rotazione della camera su ambedue gli assp
    Transform myTransform;
    CharacterController controller;

    //telecamera da ruotare
    Transform cam;
    //osso del torso da far ruotare nella direzione della camera
    [SerializeField] Transform TorsoSpina;

    float vertRot;

    // Use this for initialization
    void Start ()
    {
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        cam = GetComponentInChildren<Camera>().transform;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //imposta la velocita del personaggio
        Vector3 forward = Input.GetAxis("Vertical") * myTransform.forward;
        Vector3 right = Input.GetAxis("Horizontal") * myTransform.right;
        Vector3 speed = (forward + right) * MoveSpeed;
        controller.SimpleMove(speed);

        //ruota orizzontalemtene il personaggio e di conseguenza anche la telecamera che e' un suo child
        float horRot = Input.GetAxis("Mouse X") * RotSpeed;
        myTransform.Rotate(0, horRot, 0);

        //rotazione verticale della telecamera
        vertRot = -Input.GetAxis("Mouse Y") * RotSpeed; //di default unity inverte l'asse verticale del mouse quindi per averlo diretto basta moltiplicare per -
        if (cam != null)
        {
            float x = cam.rotation.eulerAngles.x;
            if (x >= 360 - camAngle - 1 || x <= camAngle + 1) // aggiungi +1 per evitare che 30.0000000001 o cose simili blocchino la telecamera
            {
                x += vertRot;
                //blocca rotazione della camera
                if (x > camAngle && x <= 180) x = camAngle;
                if (x < 360 - camAngle && x > 180) x = 360 - camAngle;

                cam.rotation = Quaternion.Euler(x, cam.rotation.eulerAngles.y, 0);
            }
            else cam.rotation = Quaternion.identity;
        }

	}

    //chiamato dopo l'animazione
    private void LateUpdate()
    {
        if (TorsoSpina == null) return;
        //ruota il busto
        Vector3 temp = TorsoSpina.rotation.eulerAngles;
        temp.z = temp.z - cam.rotation.eulerAngles.x;
        TorsoSpina.rotation = Quaternion.Euler(temp);
    }
}
