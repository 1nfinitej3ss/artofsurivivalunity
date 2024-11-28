using UnityEngine;

public class FieldOfViewCheck : MonoBehaviour
{
    public Camera camera; 
    public GameObject canvasGameObject;

    // Update is called once per frame
    void Update()
    {
        if (camera.fieldOfView == 15)
        {
            canvasGameObject.SetActive(true);
        }
        else
        {
            canvasGameObject.SetActive(false);
        }
    }
}
