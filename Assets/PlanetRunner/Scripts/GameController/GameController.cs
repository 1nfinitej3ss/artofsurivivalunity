using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

namespace PlanetRunner {
    public class GameController : MonoBehaviour {

        [Tooltip("Wait X seconds on start to run")]
        public float WaitSecondsOnStart = 1.5f;

        public GameObject FinishDialog;
        public GameObject GameOverDialog;

        [Tooltip("To Unsubscribe UI Method from PlayerState. Reference all 5 progress bars here")]
        public ProgressBar[] progressBar;

        [HideInInspector]
        public bool StartGame = false;

        private bool gameFinished = false;

        // Cursor settings
        public Texture2D cursorTexture;
        public Vector2 hotSpot = Vector2.zero;
        public CursorMode cursorMode = CursorMode.Auto;

        void Start()
        {
            Debug.Log("GameController Start method beginning...");

            // Set the custom cursor
            SetCustomCursor();

            if (PlayerState.Instance != null)
            {
                // Ensure the PlayerController reference is set in PlayerState
                PlayerState.Instance.playerController = FindObjectOfType<PlayerController>();
                if (PlayerState.Instance.playerController != null)
                {
                    PlayerState.Instance.playerController.enabled = true;
                    PlayerState.Instance.playerController.SetSpawnLocation();
                    PlayerState.Instance.gameController = this;
                    Debug.Log("PlayerState and Controller initialized");
                }
                else
                {
                    Debug.LogError("PlayerController not found in PlayerState");
                }
            }
            else
            {
                Debug.LogError("PlayerState not found in Start");
            }

            StartCoroutine(WaitAfterStartForSeconds());
            Debug.Log("GameController Start method complete");
        }

        private IEnumerator WaitAfterStartForSeconds() 
        {
            Debug.Log($"Starting delay of {WaitSecondsOnStart} seconds before setting StartGame to true");
            yield return new WaitForSeconds(WaitSecondsOnStart);
            StartGame = true;
            Debug.Log($"Delay complete - StartGame set to true. Player enabled: {PlayerState.Instance?.playerController?.enabled}");
            
            // Double-check player controller state
            if (PlayerState.Instance?.playerController != null)
            {
                PlayerState.Instance.playerController.enabled = true;
                PlayerState.Instance.playerController.ReconnectUIButtons();
                Debug.Log("Player controller re-enabled and buttons reconnected after game start");
            }
        }

        public void DoSetFinishLevel() {
            StartGame = false;
            gameFinished = true;
            FinishDialog.SetActive(true);
        }

        public void DoSetGameOverLevel() {
            StartGame = false;
            gameFinished = true;
            
            // Save the current state before transitioning
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.SaveFinalScores();
            }
            
            // Load the GameOver scene instead of just showing a dialog
            SceneManager.LoadScene("GameOver");
            
            // Clean up the player object
            GameObject player = GameObject.FindGameObjectWithTag(Const.PLAYER);
            if (player != null) {
                Destroy(player);
            }
        }

        private static GameController _instance;
        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameController>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameController");
                        _instance = go.AddComponent<GameController>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!StartGame || gameFinished)
                return;

            if (PlayerState.Instance != null)
            {
                string[] statsToCheck = { "Money", "Career", "Energy", "Creativity", "Time" };
                foreach (var stat in statsToCheck)
                {
                    float value = PlayerState.Instance.GetPlayerValue(stat);
                    if (value <= 20) // Log when getting close to zero
                    {
                        Debug.LogWarning($"[TimeCheck] {stat} is getting low: {value}");
                    }
                    if (value <= 0)
                    {
                        Debug.LogWarning($"[TimeCheck] {stat} has hit {value} - Setting game over pending");
                        PlayerState.Instance.SetGameOverPending(true);
                        return;
                    }
                }
            }
        }

        public void OnSceneChanged()
        {
            StartGame = false;
            PlayerState playerState = PlayerState.Instance;
            if (playerState != null)
            {
                if (playerState.playerController != null)
                {
                    playerState.playerController.enabled = false;
                    playerState.playerController.SaveSpawnLocation();
                    playerState.playerController.ResetMovementState();
                }

                if (playerState.playerRb != null)
                {
                    playerState.playerRb.gravityScale = 0;
                }

                foreach (ProgressBar bar in progressBar)
                {
                    if (bar != null)
                    {
                        bar.UnsubsribeUI();
                    }
                }
            }
            else
            {
                Debug.LogError("PlayerState not found during scene change");
            }
        }

        private void SetCustomCursor() {
            // Only set custom cursor if CursorManager is not present
            if (FindObjectOfType<CursorManager>() == null && cursorTexture != null)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "main")
            {
                Debug.Log("Main scene loaded in GameController");
                StartGame = false;
                
                // Wait a frame to ensure all objects are properly initialized
                StartCoroutine(SetupMainScene());
            }
        }

        private IEnumerator SetupMainScene()
        {
            yield return null;  // Wait one frame

            // Find and setup player first
            var player = GameObject.FindGameObjectWithTag(Const.PLAYER);
            if (player != null)
            {
                var playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // Enable the controller
                    playerController.enabled = true;
                    
                    // Reset state
                    playerController.ResetMovementState();
                    
                    // Reconnect UI
                    playerController.ReconnectUIButtons();
                    
                    // Update PlayerState
                    if (PlayerState.Instance != null)
                    {
                        PlayerState.Instance.playerController = playerController;
                        PlayerState.Instance.gameController = this;
                    }
                    
                    Debug.Log("Player controller reset and reconnected");
                }
                else
                {
                    Debug.LogError("PlayerController component not found on player object");
                }
            }
            else
            {
                Debug.LogError("Player object not found in scene");
            }
            
            // Start the game after setup is complete
            StartCoroutine(WaitAfterStartForSeconds());
        }

        private void ReconnectPlayerInputHandlers()
        {
            if (PlayerState.Instance != null && PlayerState.Instance.playerController != null)
            {
                var playerController = PlayerState.Instance.playerController;
                // Reset movement state
                playerController.OnMoveLeftButtonUp();
                playerController.OnMoveRightButtonUp();
                // Reconnect UI buttons
                playerController.ReconnectUIButtons();
            }
            else
            {
                Debug.LogError("PlayerState or PlayerController not found when reconnecting input handlers");
            }
        }        
        
    }
}