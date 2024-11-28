using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class returnToMain : MonoBehaviour
{
    // Name of the scene you want to load
    public string sceneName;
    public GameObject gameOverCanvas;
    public GameObject nonGameOverCanvas;

    // Use this for initialization
    void Start()
    {
        // Reset Time
        Time.timeScale = 1f;

        if (SceneManager.GetActiveScene().name == "GameOver")
        {
            GameOverInitialization();
        }

        // Start the coroutine
        StartCoroutine(LoadSceneAfterDelay());
    }

    // Coroutine to load the scene after a delay
    IEnumerator LoadSceneAfterDelay()
    {
        DestroyPlayer();

        // Wait for 5 seconds
        yield return new WaitForSeconds(5);

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    private void DestroyPlayer()
    {
        // Destroy Player
        Destroy(PlayerState.Instance.gameObject);
        Debug.Log("Player has been destoyed !");
    }

    private void GameOverInitialization()
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
}
