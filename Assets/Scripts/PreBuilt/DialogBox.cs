using PlanetRunner;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogBox : MonoBehaviour
{
    public string sceneName;  // The name of the scene to load
    public string locationKey;  // The PlayerPrefs key for the location
    public GameController gameController;
    public LocationToggleManager locationToggleManager;

    private SceneManagerHelper sceneManager;

    private void Awake()
    {
        // Find the SceneManagerHelper instance
        sceneManager = SceneManagerHelper.Instance;
        if (sceneManager == null)
        {
            Debug.LogError("SceneManagerHelper not found! Make sure it exists in your start scene.");
        }
    }

    // Method for "Yes" button click
    public void OnButton1Click()
    {
        // Mark the location as visited
        if (locationToggleManager != null)
        {
            locationToggleManager.VisitLocation(locationKey);
        }
        else
        {
            Debug.LogWarning("LocationToggleManager is not assigned!");
        }

        // Update game controller state
        if (gameController != null)
        {
            gameController.OnSceneChanged();
        }
        else
        {
            Debug.LogWarning("GameController is not assigned!");
        }

        // Load the sub-scene using SceneManagerHelper
        if (sceneManager != null)
        {
            sceneManager.LoadSubScene(sceneName);
        }
        else
        {
            // Fallback to direct scene loading if SceneManagerHelper is not available
            Debug.LogWarning("SceneManagerHelper not found, falling back to direct scene loading");
            SceneManager.LoadScene(sceneName);
        }

        // Hide the dialog box
        gameObject.SetActive(false);
    }

    // Method for "No" button click
    public void OnButton2Click()
    {
        gameObject.SetActive(false);
    }

    // Method to return to main scene
    public void ReturnToMain()
    {
        if (sceneManager != null)
        {
            sceneManager.ReturnToMain();
        }
        else
        {
            Debug.LogWarning("SceneManagerHelper not found, falling back to direct scene loading");
            SceneManager.LoadScene("main");
        }
    }

    // Helper method to setup the dialog box with new parameters
    public void Setup(string newSceneName, string newLocationKey)
    {
        sceneName = newSceneName;
        locationKey = newLocationKey;
    }
}