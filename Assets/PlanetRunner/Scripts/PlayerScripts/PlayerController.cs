using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PlanetRunner {
    public class PlayerController : MonoBehaviour {
        [Tooltip("Enables Auto-Move")]
        public bool EnableAutoMove = false;
        [Tooltip("If Auto-Move is enabled, select the direction")]
        public bool AutoMoveRight = true;

        private Transform LookTransform;

        public float jumpForce = 7.0f;

        [Tooltip("Enables DoubleJump. Default is SingleJump")]
        public bool EnableDoubleJump;
        public float doubleJumpForce = 5.0f;

        public float speed = 3.0f;
        public float maxVelocityChange = 10.0f;

        public bool EnableGravitation = false;
        public float gravity = 2.0f;
        [Tooltip("Local gravity: How much local gravitation if the player walks on the planet. More speed needs more local gravity")]
        public float localGravity = 9.8f;

        public float mass = 1.0f;

        [HideInInspector]
        public bool facingRight = true;

        public Transform GroundCheck;

        [HideInInspector]
        public bool grounded = false;

        public LayerMask whatIsGround;

        private bool DoJump = false;
        private bool DoDoubleJump = false;

        [HideInInspector]
        public System.Guid lastPlanetID;
        private System.Guid nextPlanetID;

        private Animator anim;
        private GameController gameController;
        private Transform spawnLocation;
        private Rigidbody2D playerRigidbody;

        // Movement flags for button input
        private bool moveLeft = false;
        private bool moveRight = false;
        private float horizontalInput = 0f;

    private PlayerState playerState;

        private bool debugMode = true;
        private float lastLogTime = 0f;
        private const float LOG_INTERVAL = 0.5f; // Log every 0.5 seconds

        private void Awake() {
            Debug.Log("Player Awake called - About to Initialize");
            Initialize();
            DontDestroyOnLoad(gameObject);
            Debug.Log("Player DontDestroyOnLoad set");
        }

        public void Initialize()
        {
            Debug.Log("PlayerController Initialize called");

            anim = GetComponent<Animator>();
            playerRigidbody = GetComponent<Rigidbody2D>();

            if (playerRigidbody == null)
            {
                Debug.LogError("PlayerController: Rigidbody2D component is missing.");
                return;
            }

            // Initialize gravity and physics properties
            playerRigidbody.gravityScale = EnableGravitation ? gravity : 0;
            playerRigidbody.mass = mass;
            playerRigidbody.freezeRotation = true;
            playerRigidbody.velocity = Vector2.zero; // Reset any initial velocity

            // Find GameController with more detailed logging
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            Debug.Log($"Searching through {allObjects.Length} objects for GameController");

            GameObject go = GameObject.FindGameObjectWithTag(Const.GAMECONTROLLER);
            if (go != null)
            {
                gameController = go.GetComponent<GameController>();
                Debug.Log($"GameController found: {go.name} and component assigned: {gameController != null}");
            }
            else
            {
                Debug.LogError($"GameController not found. Tag searched for: {Const.GAMECONTROLLER}");
            }

            whatIsGround = 1 << LayerMask.NameToLayer(Const.GROUND);
            
            // Ensure the player starts grounded
            grounded = true;
            
            Debug.Log($"PlayerController initialization complete. Gravity: {playerRigidbody.gravityScale}, Mass: {playerRigidbody.mass}");
        }        

        public void UpdateGravity() {
            if (playerRigidbody == null) return;

            if (grounded) {
                // When grounded, disable gravity and use local gravity for planet alignment
                playerRigidbody.gravityScale = 0;
                playerRigidbody.mass = 1;
            } else if (EnableGravitation) {
                // When in air and gravitation is enabled
                playerRigidbody.gravityScale = gravity;
                playerRigidbody.mass = mass;
            } else {
                // When in air but gravitation is disabled
                playerRigidbody.gravityScale = 0;
                playerRigidbody.mass = 1;
            }

            playerRigidbody.freezeRotation = true;
        }

        private void Update()
        {
            if (gameController == null || !gameController.StartGame)
            {
                LogDebug("Update skipped - game not started or controller null");
                return;
            }

            // Input state
            horizontalInput = 0f;
            if (moveLeft) horizontalInput = -1f;
            if (moveRight) horizontalInput = 1f;

            if (horizontalInput != 0)
            {
                LogDebug($"Moving with input: {horizontalInput}");
            }

            // Handle orientation BEFORE movement and gravity
            HandlePlayerOrientation();

            if (grounded)
            {
                HandleMovement();
            }

            UpdateGravity();

            if (!EnableGravitation && LookTransform != null)
            {
                AlignToPlanet(LookTransform.position);
            }
        }

        private bool IsTouchInputForJump() {
            if (Input.touchCount > 0) {
                foreach (Touch t in Input.touches) {
                    if (t.phase == TouchPhase.Began) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void HandleJump(bool doJumpButtonClicked) {
            if (!doJumpButtonClicked || playerRigidbody == null) return;

            if (grounded && !DoJump) {
                DoJump = true;
                playerRigidbody.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log($"Applying jump force: {jumpForce}");
            }

            if (EnableDoubleJump && !grounded && !DoJump && !DoDoubleJump) {
                DoDoubleJump = true;
                playerRigidbody.AddForce(transform.up * doubleJumpForce, ForceMode2D.Impulse);
                Debug.Log($"Applying double jump force: {doubleJumpForce}");
            }
        }

        void FixedUpdate() {
            if (gameController != null && gameController.StartGame && playerRigidbody != null) {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            if (LookTransform == null || playerRigidbody == null)
            {
                return;
            }

            Vector3 forward = Vector3.Cross(transform.up, -LookTransform.right).normalized;
            Vector3 right = Vector3.Cross(transform.up, LookTransform.forward).normalized;
            
            Vector2 targetVelocity = new Vector2(right.x, right.y) * horizontalInput * speed;
            Vector2 currentVelocity = playerRigidbody.velocity;
            Vector2 velocityChange = targetVelocity - currentVelocity;
            
            playerRigidbody.AddForce(velocityChange, ForceMode2D.Impulse);
        }

        private void ApplyMovementForces() {
            if (LookTransform == null) return;

            Vector3 forward = Vector3.Cross(transform.up, -LookTransform.right).normalized;
            Vector3 right = Vector3.Cross(transform.up, LookTransform.forward).normalized;

            Vector3 targetVelocity;
            if (EnableAutoMove) {
                targetVelocity = AutoMoveRight ? (forward + right) * speed : (forward - right) * speed;
            } else {
                targetVelocity = right * horizontalInput * speed;
            }

            Vector3 velocityChange = CalculateVelocityChange(targetVelocity);
            playerRigidbody.AddForce(velocityChange, ForceMode2D.Impulse);
        }

        private Vector3 CalculateVelocityChange(Vector3 targetVelocity) {
            Vector3 velocity = transform.InverseTransformDirection(playerRigidbody.velocity);
            velocity.y = 0;
            velocity = transform.TransformDirection(velocity);
            Vector3 velocityChange = transform.InverseTransformDirection(targetVelocity - velocity);

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            return transform.TransformDirection(velocityChange);
        }

        private void HandleFlipBasedOnInput()
        {
            if (horizontalInput != 0)
            {
                // Flip the sprite based on movement direction
                bool shouldFaceRight = horizontalInput > 0;
                transform.localScale = new Vector3(
                    shouldFaceRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        private void Flip() {
            facingRight = !facingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        private void AlignToPlanet(Vector3 targetPosition) {
            if (playerRigidbody == null) return;

            Vector3 toCenter = targetPosition - transform.position;
            toCenter.Normalize();
            playerRigidbody.AddForce(toCenter * localGravity, ForceMode2D.Force);

            Quaternion q = Quaternion.FromToRotation(transform.up, -toCenter) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, q, 1);
        }

        public void PlayerDie() {
            if (gameController != null) {
                gameController.DoSetGameOverLevel();
            }
        }

        public void SetPlanetIdAndAlign(System.Guid planetId, Vector3 pos) {
            nextPlanetID = planetId;
            AlignToPlanet(pos);
        }

        public void SetLookTransform(Transform newLookTransform)
        {
            LookTransform = newLookTransform;
        }

        public void SaveSpawnLocation() {
            spawnLocation = transform;
        }

        public void SetSpawnLocation() {
            if (spawnLocation != null) {
                transform.position = spawnLocation.position;
                transform.rotation = spawnLocation.rotation;
            }
        }

        public void StartChanceCard() {
            Debug.LogError("StartChanceCard called - Loading ChanceCard scene");
            SceneManager.LoadScene("ChanceCard");
        }

        // Button input handlers
        public void OnMoveLeftButtonDown()
        {
            if (!gameController?.StartGame ?? false)
            {
                Debug.LogWarning("Left button pressed but game not started!");
                return;
            }
            moveLeft = true;
            moveRight = false;
            LogDebug($"Left button pressed. moveLeft:{moveLeft}, moveRight:{moveRight}, StartGame:{gameController.StartGame}");
        }

        public void OnMoveLeftButtonUp()
        {
            if (!gameController?.StartGame ?? false)
            {
                Debug.LogWarning("Left button released but game not started!");
                return;
            }
            moveLeft = false;
            LogDebug($"Left button released. moveLeft:{moveLeft}, moveRight:{moveRight}");
        }

        public void OnMoveRightButtonDown()
        {
            if (!gameController?.StartGame ?? false)
            {
                Debug.LogWarning("Right button pressed but game not started!");
                return;
            }
            moveRight = true;
            moveLeft = false;
            LogDebug($"Right button pressed. moveLeft:{moveLeft}, moveRight:{moveRight}, StartGame:{gameController.StartGame}");
        }

        public void OnMoveRightButtonUp()
        {
            if (!gameController?.StartGame ?? false)
            {
                Debug.LogWarning("Right button released but game not started!");
                return;
            }
            moveRight = false;
            LogDebug($"Right button released. moveLeft:{moveLeft}, moveRight:{moveRight}");
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Verify and restore component references
            if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody2D>();
            if (gameController == null) gameController = FindObjectOfType<GameController>();
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "main")
            {
                // Clear any existing movement state immediately
                ResetMovementState();
                
                // Ensure we have references
                if (playerRigidbody == null) playerRigidbody = GetComponent<Rigidbody2D>();
                if (gameController == null) gameController = FindObjectOfType<GameController>();
                
                StartCoroutine(InitializeAfterSceneLoad());
                
                // Enable the component explicitly
                enabled = true;
                
                Debug.Log("PlayerController: Reinitialized on main scene load");
            }
        }

        private IEnumerator InitializeAfterSceneLoad()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Full initialization
            Initialize();
            
            // Reset physics state
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                playerRigidbody.simulated = true; // Ensure physics simulation is enabled
            }

            // Reconnect UI with debug logging
            Debug.Log("Attempting to reconnect UI buttons...");
            ReconnectUIButtons();
            
            // Find and set planet reference
            GameObject planet = GameObject.FindGameObjectWithTag("Planet");
            if (planet != null)
            {
                SetLookTransform(planet.transform);
                Debug.Log("Planet reference set successfully");
            }
            else
            {
                Debug.LogWarning("No planet found to set as look transform");
            }
        }

        public void ReconnectUIButtons()
        {
            Debug.Log("ReconnectUIButtons called");
            
            // Find buttons directly by name
            GameObject leftButton = GameObject.Find("MoveLeftButton");
            GameObject rightButton = GameObject.Find("MoveRightButton");

            if (leftButton == null || rightButton == null)
            {
                Debug.LogError($"Could not find movement buttons. Left: {leftButton != null}, Right: {rightButton != null}");
                return;
            }

            // Try to find or add Button components
            Button leftButtonComponent = leftButton.GetComponent<Button>();
            Button rightButtonComponent = rightButton.GetComponent<Button>();

            if (leftButtonComponent == null) leftButtonComponent = leftButton.AddComponent<Button>();
            if (rightButtonComponent == null) rightButtonComponent = rightButton.AddComponent<Button>();

            // Remove all existing listeners
            leftButtonComponent.onClick.RemoveAllListeners();
            rightButtonComponent.onClick.RemoveAllListeners();

            // Setup event triggers for both down and up events
            SetupButtonEvents(leftButton, OnMoveLeftButtonDown, OnMoveLeftButtonUp, "Left");
            SetupButtonEvents(rightButton, OnMoveRightButtonDown, OnMoveRightButtonUp, "Right");
            
            Debug.Log("Button events reconnected successfully");
        }

        private void SetupButtonEvents(GameObject button, UnityAction downAction, UnityAction upAction, string buttonName)
        {
            // Remove any existing EventTrigger
            EventTrigger existingTrigger = button.GetComponent<EventTrigger>();
            if (existingTrigger != null)
            {
                DestroyImmediate(existingTrigger);
            }

            // Add new EventTrigger
            EventTrigger trigger = button.AddComponent<EventTrigger>();

            // Setup PointerDown
            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { downAction(); });
            trigger.triggers.Add(entryDown);

            // Setup PointerUp
            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { upAction(); });
            trigger.triggers.Add(entryUp);

            // Setup PointerExit (in case the pointer leaves the button while pressed)
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { upAction(); });
            trigger.triggers.Add(entryExit);

            Debug.Log($"{buttonName} button events setup completed");
        }

        private void LogDebug(string message)
        {
            if (debugMode && Time.time - lastLogTime >= LOG_INTERVAL)
            {
                Debug.Log($"[PLAYER_DEBUG] {message}");
                lastLogTime = Time.time;
            }
        }

        private void HandlePlayerOrientation()
        {
            if (horizontalInput != 0)
            {
                // Always use original scale magnitude for consistency
                float originalXScale = Mathf.Abs(transform.localScale.x);
                Vector3 newScale = transform.localScale;
                
                // Flip based on input direction (negative for left, positive for right)
                newScale.x = horizontalInput < 0 ? -originalXScale : originalXScale;
                
                transform.localScale = newScale;
            }
        }

        public void ResetMovementState()
        {
            moveLeft = false;
            moveRight = false;
            horizontalInput = 0f;
            
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
            }
        }

        public void StartNewGame()
        {
            // Clear all monthly effects
            QuestionTemplate.ClearAllMonthlyEffects();
            
            // Reset question state
            if (QuestionContentManager.Instance != null)
            {
                QuestionContentManager.Instance.ResetGameState();
            }

            // Rest of your existing new game setup...
        }
    }
}