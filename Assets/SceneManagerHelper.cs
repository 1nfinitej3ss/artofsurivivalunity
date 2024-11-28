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

    public void LoadSubScene(string sceneName) 
    {
        // Store player reference before loading sub scene
        playerInstance = GameObject.FindGameObjectWithTag(Const.PLAYER);
        if (playerInstance != null)
        {
            DontDestroyOnLoad(playerInstance);
            playerInstance.SetActive(false);
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
                playerController.OnMoveLeftButtonUp();
                playerController.OnMoveRightButtonUp();
                
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