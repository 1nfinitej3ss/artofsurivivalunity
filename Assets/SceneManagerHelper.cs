using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using PlanetRunner;

public class SceneManagerHelper : MonoBehaviour 
{
    public static SceneManagerHelper Instance { get; private set; }
    private GameObject playerInstance;

    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Debug controls for testing
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (PlayerState.Instance != null)
            {
                // Set time to 0 to trigger game over
                PlayerState.Instance.SetTime(0f, 0f, 0);
                // Trigger game over directly
                PlayerState.Instance.TriggerGameOver();
                Debug.Log("Player state reset via debug command");
            }
            LoadGameOver();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            LoadGameVictory();
        }
    }

    public void LoadGameOver()
    {
        // Store player reference before loading game over scene
        playerInstance = GameObject.FindGameObjectWithTag(Const.PLAYER);
        if (playerInstance != null)
        {
            playerInstance.SetActive(false);
        }

        SceneManager.LoadScene("GameOver");
        Debug.Log("Loading GameOver scene via debug command");
    }

    public void LoadGameVictory()
    {
        // Store player reference before loading victory scene
        playerInstance = GameObject.FindGameObjectWithTag(Const.PLAYER);
        if (playerInstance != null)
        {
            playerInstance.SetActive(false);
        }

        SceneManager.LoadScene("GameVictory");
        Debug.Log("Loading GameVictory scene via debug command");
    }

    public void LoadSubScene(string sceneName) 
    {
        // Store player reference before loading sub scene
        playerInstance = GameObject.FindGameObjectWithTag(Const.PLAYER);
        if (playerInstance != null)
        {
            DontDestroyOnLoad(playerInstance);
            playerInstance.SetActive(false);
        }

        // Ensure LocationToggleManager persists
        var toggleManager = LocationToggleManager.Instance;
        if (toggleManager != null)
        {
            DontDestroyOnLoad(toggleManager.gameObject);
        }
        
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToMain() 
    {
        StartCoroutine(LoadMainScene());
    }

    private IEnumerator LoadMainScene() 
    {
        // Load the main scene
        SceneManager.LoadScene("main");

        // Wait for scene to load
        yield return new WaitForSeconds(0.2f);

        if (playerInstance != null)
        {
            playerInstance.SetActive(true);
            
            // Reset player components
            var playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
                Debug.Log("[SCENE] Player controller enabled");
                
                // Let the PlayerController handle its own initialization
                playerController.ReconnectUIButtons();
                
                // Find planet and set look transform
                var planet = GameObject.FindGameObjectWithTag("Planet");
                if (planet != null)
                {
                    playerController.SetLookTransform(planet.transform);
                    Debug.Log("Player reconnected to planet");
                }
            }

            // Find GameController
            var gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                DontDestroyOnLoad(gameController.gameObject);
                Debug.Log("GameController found and preserved");
            }
            else
            {
                Debug.LogError("GameController not found after loading main scene");
            }
        }
        else
        {
            Debug.LogError("Player instance lost during scene transition!");
        }
    }
}