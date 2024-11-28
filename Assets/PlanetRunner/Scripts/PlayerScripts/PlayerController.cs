using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
            Debug.Log("PlayerController initialization complete");
        }        

        public void UpdateGravity() {
            if (playerRigidbody == null) return;

            if (EnableGravitation) {
                playerRigidbody.gravityScale = gravity;
                playerRigidbody.mass = mass;
            } else {
                playerRigidbody.gravityScale = 0;
                playerRigidbody.mass = 1;
            }

            if (grounded) {
                playerRigidbody.gravityScale = 0;
                playerRigidbody.mass = 1;
            }

            playerRigidbody.freezeRotation = true;
        }

        void Update() {
            if (GroundCheck != null) {
                grounded = Physics2D.OverlapCircle(GroundCheck.position, 0.16f, whatIsGround);
                Debug.Log($"Ground check state: {grounded}");
            }

            if (anim != null) {
                anim.SetBool(Const.GROUND, grounded);
            }

            if (gameController != null && gameController.StartGame) {
                Debug.Log($"Update running - Horizontal Input: {horizontalInput}, Grounded: {grounded}");
                
                // Set horizontal input based on button state
                horizontalInput = 0f;
                if (moveLeft) horizontalInput = -1f;
                if (moveRight) horizontalInput = 1f;

                if (grounded) {
                    DoJump = false;
                    DoDoubleJump = false;
                }

                UpdateGravity();

                bool doJumpButtonClicked = IsTouchInputForJump();
                HandleJump(doJumpButtonClicked);

                if (!EnableGravitation && LookTransform != null) {
                    AlignToPlanet(LookTransform.position);
                }
            } else {
                Debug.Log($"GameController null or StartGame false. Controller: {(gameController != null ? "exists" : "null")}, StartGame: {(gameController != null ? gameController.StartGame.ToString() : "N/A")}");
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
                Debug.Log($"Handling movement - Velocity: {playerRigidbody.velocity}, Input: {horizontalInput}");
            }
        }

        private void HandleMovement() {
            if (grounded) {
                Debug.Log($"Grounded movement - Current velocity: {playerRigidbody.velocity}");
                
                if (Mathf.Abs(playerRigidbody.velocity.magnitude) <= 0.01f) {
                    if (anim != null) anim.SetBool("isRun", false);
                } else {
                    if (anim != null) {
                        anim.SetFloat(Const.SPEED, Mathf.Abs(playerRigidbody.velocity.magnitude));
                        anim.SetBool("isRun", true);
                    }
                }

                if (LookTransform != null) {
                    ApplyMovementForces();
                } else {
                    Debug.LogWarning("LookTransform is null during movement");
                }

                HandleFlipBasedOnInput();
            } else {
                Debug.Log("Not grounded during movement update");
            }
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
            
            Debug.Log($"Applying forces - Input: {horizontalInput}, Target: {targetVelocity}, Change: {velocityChange}");
            
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

        private void HandleFlipBasedOnInput() {
            if ((moveRight && !facingRight) || (moveLeft && facingRight)) {
                Flip();
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

        public void SetLookTransform(Transform lookTransform) {
            LookTransform = lookTransform;
            Debug.Log($"LookTransform set to: {(lookTransform != null ? lookTransform.name : "null")}");
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
            SceneManager.LoadScene("ChanceCard");
        }

        // Button input handlers
        public void OnMoveLeftButtonDown() {
            Debug.Log("Left Button DOWN pressed - before state change");
            moveLeft = true;
            moveRight = false;
            Debug.Log($"Left Button DOWN pressed - after state change. moveLeft: {moveLeft}");
        }

        public void OnMoveLeftButtonUp() {
            if (moveLeft) {
                moveLeft = false;
                Debug.Log("Left button released");
            }
        }

        public void OnMoveRightButtonDown() {
            Debug.Log("Right Button DOWN pressed - before state change");
            moveRight = true;
            moveLeft = false;
            Debug.Log($"Right Button DOWN pressed - after state change. moveRight: {moveRight}");
        }

        public void OnMoveRightButtonUp() {
            if (moveRight) {
                moveRight = false;
                Debug.Log("Right button released");
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
                // Ensure the PlayerController reference is properly set
                if (playerState.playerController == null)
                {
                    // Find the PlayerController component in the scene
                    playerState.playerController = FindObjectOfType<PlayerController>();
                    if (playerState.playerController != null)
                    {
                        playerState.playerController.enabled = true;
                        playerState.playerController.gameObject.SetActive(true);
                        Debug.Log("PlayerController found and activated in MainScene.");
                    }
                    else
                    {
                        Debug.LogError("PlayerController not found in MainScene.");
                    }
                }
                else
                {
                    // Ensure the PlayerController is enabled and active
                    playerState.playerController.enabled = true;
                    playerState.playerController.gameObject.SetActive(true);
                    Debug.Log("PlayerController activated in MainScene.");
                }

                // Reset the player's starting position
                playerState.SetPlayerStartPosition();
            }
        }

        private void ReconnectUIButtons()
        {
            // Find the movement buttons
            GameObject rightButton = GameObject.Find("MoveRightButton");
            GameObject leftButton = GameObject.Find("MoveLeftButton");

            if (rightButton != null && leftButton != null)
            {
                // Get EventTrigger components
                EventTrigger rightTrigger = rightButton.GetComponent<EventTrigger>();
                EventTrigger leftTrigger = leftButton.GetComponent<EventTrigger>();

                // Reconnect the event entries
                foreach (EventTrigger.Entry entry in rightTrigger.triggers)
                {
                    if (entry.eventID == EventTriggerType.PointerDown)
                    {
                        entry.callback.AddListener((data) => { OnMoveRightButtonDown(); });
                    }
                    else if (entry.eventID == EventTriggerType.PointerUp)
                    {
                        entry.callback.AddListener((data) => { OnMoveRightButtonUp(); });
                    }
                }

                foreach (EventTrigger.Entry entry in leftTrigger.triggers)
                {
                    if (entry.eventID == EventTriggerType.PointerDown)
                    {
                        entry.callback.AddListener((data) => { OnMoveLeftButtonDown(); });
                    }
                    else if (entry.eventID == EventTriggerType.PointerUp)
                    {
                        entry.callback.AddListener((data) => { OnMoveLeftButtonUp(); });
                    }
                }

                Debug.Log("UI buttons reconnected successfully");
            }
            else
            {
                Debug.LogWarning("Could not find movement buttons");
            }
        }
    }
}