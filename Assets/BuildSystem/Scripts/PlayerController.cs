using UnityEngine;

namespace BuildSystem
{
    /// <summary>
    /// Basic player controller inclued in the package.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {

        //Variables

        //max cam angle: from 360 - camAngle to camAngle
        float camAngle = 20f; // hal of the camera rotation angle

        float MoveSpeed = 7f; // move speed
        float RotSpeed = 5f; // camera rotation speed

        //Components

        Transform myTransform;
        CharacterController controller;
        //cam to rotate
        Transform cam;

        ObjectSelector selector;

        bool stopCamRotation = false;

        float vertRot;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            myTransform = transform;
            cam = GetComponentInChildren<Camera>().transform;

            selector = GetComponent<ObjectSelector>();
            if (selector != null) selector.OnMenuCollapse += BlockCamera; //block camera rotaion when selecting items from menu
        }

        private void OnDisable()
        {
            //unsubscribe to event
            selector = GetComponent<ObjectSelector>();
            if (selector != null) selector.OnMenuCollapse -= BlockCamera;
        }

        void Update()
        {
            MoveCharacter();
            if (!stopCamRotation)
            {
                RotateControllerAndCamera();
            }

        }

        /// <summary>
        /// Move the character controller
        /// </summary>
        void MoveCharacter()
        {
            //set move speed to the character
            Vector3 forward = Input.GetAxis("Vertical") * myTransform.forward;
            Vector3 right = Input.GetAxis("Horizontal") * myTransform.right;
            Vector3 speed = (forward + right) * MoveSpeed;
            controller.SimpleMove(speed);
        }

        /// <summary>
        /// Rotate the character controller and the camera
        /// </summary>
        void RotateControllerAndCamera()
        {
            //rotare the character horizontally (camera included)
            float horRot = Input.GetAxis("Mouse X") * RotSpeed;
            myTransform.Rotate(0, horRot, 0);

            //rotate only the camera vertically
            vertRot = -Input.GetAxis("Mouse Y") * RotSpeed; //by default mouse Y is inverted so we multiply for -1
            if (cam != null)
            {
                float x = cam.rotation.eulerAngles.x;
                if (x >= 360 - camAngle - 1 || x <= camAngle + 1) // add 1 to avoid blocks on approsimative values a bit higher than camAngle
                {
                    x += vertRot;
                    //lock the camera rotation
                    if (x > camAngle && x <= 180) x = camAngle;
                    if (x < 360 - camAngle && x > 180) x = 360 - camAngle;

                    cam.rotation = Quaternion.Euler(x, cam.rotation.eulerAngles.y, 0); //handle strange mouse position at the begning of the game
                }
                else cam.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Prevent camera rotation
        /// </summary>
        /// <param name="val"></param>
        public void BlockCamera(bool val)
        {
            stopCamRotation = !val;
        }

    }
}
