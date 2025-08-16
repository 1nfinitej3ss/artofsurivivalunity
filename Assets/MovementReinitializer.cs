using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using PlanetRunner;

public class MovementReinitializer : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "main")
        {
            StartCoroutine(ReinitializeMovement());
        }
    }

    private IEnumerator ReinitializeMovement()
    {
        yield return null;  // Wait one frame
        
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetMovementState();
            playerController.ReconnectUIButtons();
            Debug.Log("Movement reinitialized by MovementReinitializer");
        }
    }
}