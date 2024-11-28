using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadRestart : MonoBehaviour {
    public string startSceneName;

    public void RestartGame() {
        Time.timeScale = 1f;

        // Find and destroy all DontDestroyOnLoad objects
        GameObject[] persistentObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in persistentObjects) {
            Destroy(obj);
        }

        // Also find and destroy any other persistent objects
        var sceneManager = FindObjectOfType<SceneManagerHelper>();
        if (sceneManager != null) {
            Destroy(sceneManager.gameObject);
        }

        // Load the start scene
        SceneManager.LoadScene(startSceneName);
    }
}