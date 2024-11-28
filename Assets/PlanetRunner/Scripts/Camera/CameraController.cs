using UnityEngine;

namespace PlanetRunner {
    public class CameraController : MonoBehaviour {

        private Transform player;
        public float sensitivity = 5.0f; // rate of increase
        private Camera cam;  // add a Camera field

        // Public property to access the camera's field of view
        public float CameraFieldOfView => cam.fieldOfView;

        void Start() {
            cam = GetComponent<Camera>();  // get the Camera component
        }

        void Update () {

            FindPlayer ();

            if (player != null) {
                // Follow the player
                transform.position = new Vector3 (player.position.x, player.position.y, -40);
                
                // calculate the new FOV based on mouse scroll and sensitivity
                cam.fieldOfView += Input.mouseScrollDelta.y * sensitivity;

                // Clamp the FOV to stay within certain limits, such as between 15 and 90
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 15f, 90f);
            }
        }

        private void FindPlayer() {

            if (player == null) {
                GameObject p = GameObject.FindGameObjectWithTag (Const.PLAYER);

                if (p != null) {
                    player = p.transform;
                }
            }
        }
    }
}
