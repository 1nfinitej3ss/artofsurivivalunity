using PlanetRunner;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    public void OnPlayBtnClicked()
    {
        Debug.Log("Game Start!");
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // Reset game
        PlayerPrefs.DeleteAll();
        Debug.LogWarning("Game Has Been Reset - All Player Prefs Deleted");

        // Load main scene
        SceneManager.LoadScene("main");

        // Wait for scene to load
        yield return new WaitForSeconds(0.2f);

        // Find GameController in the new scene
        var gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            // Ensure it won't be destroyed
            DontDestroyOnLoad(gameController.gameObject);
            Debug.Log("GameController found and preserved");
        }
        else
        {
            Debug.LogError("GameController not found in main scene!");
        }

        // Add additional debug info
        Debug.Log($"Scene setup complete. StartGame: {(gameController != null ? gameController.StartGame.ToString() : "N/A")}");
    }
}