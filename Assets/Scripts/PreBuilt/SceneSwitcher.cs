using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {
    // The panel that will pop up asking the user if they want to switch scenes
    public GameObject sceneSwitchPanel;
    
    // String to store the name of the scene to load
    private string sceneToLoad;

    // Make sure the scene switch panel is turned off by default
    void Start() {
        sceneSwitchPanel.SetActive(false);
    }

    // Call this method when the player intersects a GameObject with the SceneTrigger script
    public void PromptSceneSwitch(string sceneName) {
        // Store the name of the scene to load
        sceneToLoad = sceneName;

        // Activate the panel
        sceneSwitchPanel.SetActive(true);
    }

    // Call this method when the "Yes" button is clicked
    public void OnConfirmSwitchScene() {
        SceneManager.LoadScene(sceneToLoad);
    }

    // Call this method when the "No" button is clicked
    public void OnDenySwitchScene() {
        // Deactivate the panel
        sceneSwitchPanel.SetActive(false);
    }
}
