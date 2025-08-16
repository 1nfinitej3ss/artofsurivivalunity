using PlanetRunner;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogBox : MonoBehaviour
{
    // Configuration fields
    public string sceneName;      // Scene to load when "Yes" is clicked
    public string locationKey;    // e.g., "Hospital", "School", etc.
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

        // Ensure we have a reference to LocationToggleManager
        if (locationToggleManager == null)
        {
            locationToggleManager = LocationToggleManager.Instance;
            if (locationToggleManager == null)
            {
                Debug.LogError("LocationToggleManager not found! Make sure it exists in your scene.");
            }
        }
    }

    void Start()
    {
        // Ensure button starts hidden
        gameObject.SetActive(false);
    }

    // Method for "Yes" button click
    public void OnButton1Click()
    {
        Debug.Log($"DialogBox: OnButton1Click called for location {locationKey}");
        
        // Try to find LocationToggleManager if it's null
        if (locationToggleManager == null)
        {
            locationToggleManager = LocationToggleManager.Instance;
            if (locationToggleManager == null)
            {
                Debug.LogError("DialogBox: Could not find LocationToggleManager instance!");
                return;
            }
        }

        Debug.Log($"DialogBox: Attempting to visit location {locationKey}");
        locationToggleManager.VisitLocation(locationKey);
        locationToggleManager.DebugStates();
        
        // Optional: Check if this completed the round
        if (locationToggleManager.IsCurrentRoundComplete())
        {
            // Trigger any "all locations visited" events here
            Debug.Log("All locations have been visited!");
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