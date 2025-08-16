using UnityEngine;

public class BackupCursorSystem : MonoBehaviour
{
    private void Start()
    {
        // Wait a frame to let other systems initialize
        StartCoroutine(EnableCursorNextFrame());
    }

    private System.Collections.IEnumerator EnableCursorNextFrame()
    {
        yield return null;
        
        // If cursor is not visible, enable it
        if (!Cursor.visible)
        {
            Debug.Log("BackupCursorSystem: Enabling cursor visibility");
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
} 