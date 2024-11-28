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
            Debug.Log("Delay complete - StartGame set to true");
        }

        public void DoSetFinishLevel() {
            StartGame = false;
            gameFinished = true;
            FinishDialog.SetActive(true);
        }

        public void DoSetGameOverLevel() {
            StartGame = false;
            gameFinished = true;
            GameOverDialog.SetActive(true);

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

        void Update() {
            if (gameFinished) {
                bool doRestartScene = false;

                // If the game is finished or game over. After click space or tap restart scene
                if (Input.GetButtonDown("Jump")) {
                    doRestartScene = true;
                }

                if (Input.touchCount > 0) {
                    foreach (Touch t in Input.touches) {
                        if (t.phase == TouchPhase.Began) {
                            doRestartScene = true;
                        }
                    }
                }

                if (doRestartScene) {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }

        public void OnSceneChanged() {
            PlayerState playerState = PlayerState.Instance;
            if (playerState != null)
            {
                if (playerState.playerController != null)
                {
                    playerState.playerController.enabled = false;
                    playerState.playerController.SaveSpawnLocation();
                }

                if (playerState.playerRb != null)
                {
                    playerState.playerRb.gravityScale = 0;
                }

                foreach (ProgressBar bar in progressBar) {
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
            if (cursorTexture != null)
            {
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
                StartCoroutine(WaitAfterStartForSeconds());
        // Reconnect player input handlers
        ReconnectPlayerInputHandlers();                
            }
        }

        private void ReconnectPlayerInputHandlers()
        {
            if (PlayerState.Instance != null && PlayerState.Instance.playerController != null)
            {
                var playerController = PlayerState.Instance.playerController;
                playerController.OnMoveLeftButtonUp();
                playerController.OnMoveLeftButtonDown();
                playerController.OnMoveRightButtonUp();
                playerController.OnMoveRightButtonDown();
            }
            else
            {
                Debug.LogError("PlayerState or PlayerController not found when reconnecting input handlers");
            }
        }        
        
    }
}