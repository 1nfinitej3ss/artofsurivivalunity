using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlanetRunner;

public class ReturnToMain : MonoBehaviour
{
    // Name of the scene you want to load
    public string sceneName;
    public GameObject gameOverCanvas;
    public GameObject nonGameOverCanvas;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Reset Time
        Time.timeScale = 1f;

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "GameOver" || currentScene == "GameVictory")
        {
            GameOverInitialization();
        }
    }

    // Called by the restart button
    public void RestartGame()
    {
        // Reset Time
        Time.timeScale = 1f;

        try
        {
            // First disable any active controllers to prevent updates during cleanup
            var gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.enabled = false;
            }

            var playerState = PlayerState.Instance;
            if (playerState != null)
            {
                // Reset all player values to default before destroying
                playerState.ResetAllValues();
                playerState.enabled = false;
            }

            // Clear all progress bars and UI subscriptions first
            var progressBars = FindObjectsOfType<ProgressBar>();
            foreach (var bar in progressBars)
            {
                if (bar != null)
                {
                    bar.UnsubsribeUI();
                }
            }

            // Find and destroy ALL DontDestroyOnLoad objects
            GameObject[] persistentObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in persistentObjects)
            {
                if (obj.scene.name == "DontDestroyOnLoad")
                {
                    Destroy(obj);
                    Debug.Log($"Destroyed persistent object: {obj.name}");
                }
            }

            // Reset all saved data
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save(); // Force save the deletion
            Debug.Log("All PlayerPrefs deleted and saved");

            // Load the start scene
            SceneManager.LoadScene("StartScene");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during restart: {e.Message}");
            // Attempt to load start scene even if cleanup fails
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene("StartScene");
        }
    }

    private void GameOverInitialization()
    {
        // Only handle canvas toggling for game over scene
        if (SceneManager.GetActiveScene().name == "GameOver")
        {
            if (PlayerPrefs.HasKey("SpecialGameOver"))
            {
                nonGameOverCanvas.SetActive(true);
                gameOverCanvas.SetActive(false);
            }
            else
            {
                gameOverCanvas.SetActive(true);
                nonGameOverCanvas.SetActive(false);
            }
        }

        if (scoreText != null)
        {
            // Get scores from PlayerPrefs instead of PlayerState
            string scoreDisplay = "Final Scores:\n";
            scoreDisplay += $"Money: {PlayerPrefs.GetInt("FinalMoney", 0)}\n";
            scoreDisplay += $"Career: {PlayerPrefs.GetInt("FinalCareer", 0)}\n";
            scoreDisplay += $"Energy: {PlayerPrefs.GetInt("FinalEnergy", 0)}\n";
            scoreDisplay += $"Creativity: {PlayerPrefs.GetInt("FinalCreativity", 0)}\n";
            scoreDisplay += $"Time: {PlayerPrefs.GetInt("FinalTime", 0)}";
            
            scoreText.text = scoreDisplay;
        }
    }
}
