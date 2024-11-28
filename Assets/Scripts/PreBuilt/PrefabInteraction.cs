using UnityEngine;
using PlanetRunner;
using UnityEngine.SceneManagement; // Replace "PlanetRunner" with the actual namespace of CameraController

public class PrefabInteraction : MonoBehaviour
{
    public string dialogBoxTag; // Add this line

    private GameObject dialogBox;

    void Start()
    {
        //Debug.Log("PrefabInteraction script has started.");  // Additional debug statement

        dialogBox = GameObject.FindGameObjectWithTag(dialogBoxTag); // And change this line
        if (dialogBox != null)
        {
            //Debug.Log(dialogBox.name + " Dialog Box found successfully!");
            dialogBox.SetActive(false); // ensure the dialog box is deactivated at start
        }
        else
        {
            //Debug.Log(dialogBox.name + " Dialog Box not found, check if the tag is correct.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.DrawLine(transform.position, other.transform.position, Color.red, 2.0f);

        //if (other.gameObject.CompareTag("Player"))
        // if (other.gameObject.CompareTag("Player") && Camera.main.GetComponent<CameraController>().CameraFieldOfView > 15f)
        if (other.gameObject.CompareTag("Player"))
        {
            //ebug.Log("Player entered trigger area.");
            dialogBox.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Debug.Log("Player left trigger area.");
            dialogBox.SetActive(false);
        }
    }
}
