using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBackgroundController : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Camera>().backgroundColor = Color.black;
    }
} 