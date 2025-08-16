using UnityEngine;
using System.Collections.Generic;
using PlanetRunner;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerState : MonoBehaviour
{
    // Singleton instance
    public static PlayerState Instance { get; private set; }

    // Delegate for event
    public delegate void OnStateChangeDelegate(string key, int value);
    public event OnStateChangeDelegate OnStateChange;

    // Player Controller
    public PlayerController playerController;
    public Rigidbody2D playerRb;

    // Scene Names
    public string gameOverSceneName;
    public string gameVictorySceneName;

    // Player stats
    private Dictionary<string, float> playerStats = new Dictionary<string, float>()
    {
        {"Money", 100},
        {"Career", 100},
        {"Energy", 100},
        {"Health", 100},
        {"Creativity", 100},
        {"Time", 100}
    };

    // Player Hold State
    private Dictionary<string, bool> playerHoldState = new Dictionary<string, bool>()
    {
        {"Money", false},
        {"Career", false},
        {"Energy", false},
        {"Health", false},
        {"Creativity", false},
        {"Time", false}
    };

    // YearlySummaryStartTimes
    [HideInInspector] public List<int> yearlySummaryTimeList;

    // Time Stats
    public float currentTimeOfDay {  get; private set; }
    public float totalTimePassed { get; private set; }
    public int totalDaysPassed { get; private set; }

    // Other Variables
    [HideInInspector] public GameController gameController; // We are setting this in Start() of GameController
    private Dictionary<string, int> monthlyCharges = new Dictionary<string, int>()
    {
        {"Rent" , 0},
        {"Utilities" , 0},
        {"Groceries" , 0}
    };
    public string chanceCardKey { get; private set; }

    // Add these properties to the PlayerState class
    public bool HasSavedTimeState { get; private set; } = false;
    public float CurrentTimeOfDay { get; private set; }
    public float TotalTimePassed { get; private set; }

    // Add a new field to track if game over is pending
    private bool isGameOverPending = false;

    // Add near the top with other fields
    private static bool s_SummaryActive = false;
    public static bool IsSummaryActive => s_SummaryActive;

    // Initialization
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Only initialize default values if they don't exist in PlayerPrefs
            foreach (var stat in playerStats.Keys.ToList())
            {
                string prefsKey = $"Player{stat}";
                if (PlayerPrefs.HasKey(prefsKey))
                {
                    float value = PlayerPrefs.GetInt(prefsKey);
                    playerStats[stat] = value;
                    Debug.Log($"Loaded {stat} from PlayerPrefs: {value}");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set those charges once in the start of game, they won't be reset to this again as Start() wont be called again until we restart game.
        SetMonthlyCharges(5 , 0 , 0);

        // Set the player's starting position when the scene loads
        SetPlayerStartPosition();
    }

    // Add this after the Start() method and before SetMonthlyCharges()
    private void Update()
    {
        // Attribute deduction controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            float currentValue = GetPlayerValue("Money");
            SetPlayerValue("Money", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Money. New value: {currentValue - 15}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            float currentValue = GetPlayerValue("Career");
            SetPlayerValue("Career", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Career. New value: {currentValue - 15}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            float currentValue = GetPlayerValue("Energy");
            SetPlayerValue("Energy", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Energy. New value: {currentValue - 15}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            float currentValue = GetPlayerValue("Health");
            SetPlayerValue("Health", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Health. New value: {currentValue - 15}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            float currentValue = GetPlayerValue("Creativity");
            SetPlayerValue("Creativity", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Creativity. New value: {currentValue - 15}");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            float currentValue = GetPlayerValue("Time");
            SetPlayerValue("Time", currentValue - 15, true);
            Debug.Log($"Deducted 15 from Time. New value: {currentValue - 15}");
        }
    }

    // Function to Set Monthly Charges
    /// <summary>
    /// Increment or Decrement in the existing monthly charges. Type -(value) for decrement
    /// </summary>
    /// <param name="rent"></param>
    /// <param name="utilities"></param>
    /// <param name="groceries"></param>
    public void SetMonthlyCharges(int rent, int utilities, int groceries)
    {
        // Set absolute values instead of adding to existing
        monthlyCharges["Rent"] = rent;
        monthlyCharges["Utilities"] = utilities;
        monthlyCharges["Groceries"] = groceries;

        Debug.Log($"Monthly charges set - Rent: ${rent}, Utilities: ${utilities}, Groceries: ${groceries}");
    }

    public int GetMonthlyCharges(string key)
    {
        if (monthlyCharges.ContainsKey(key))
        {
            return monthlyCharges[key];
        }

        else
        {
            //Debug.LogError("The key does not exist");
            return 0;
        }
    }

    // This method is called in ProgressBarChanges in the HandleMonthPassed Method
    public void DeductMonthlyCharges()
    {
        float currentMoney = GetPlayerValue("Money");
        float totalDeduction = 0;

        foreach (var charge in monthlyCharges)
        {
            totalDeduction += charge.Value;
        }

        float newBalance = currentMoney - totalDeduction;
        SetPlayerValue("Money", newBalance, true);

        Debug.Log($"Monthly charges deducted - Total: ${totalDeduction}, New Balance: ${newBalance}");
    }

    // Function to get player values
    public float GetPlayerValue(string key)
    {
        if (playerStats.ContainsKey(key))
            return playerStats[key];
        else
            return 0;
    }

    // Function to set player values and trigger the event
    public void SetPlayerValue(string key, float value, bool triggerEvent = true)
    {
        if (!playerStats.ContainsKey(key))
        {
            Debug.LogError($"[TimeCheck] Trying to set non-existent stat: {key}");
            return;
        }

        // Add detailed logging for time attribute changes
        if (key == "Time")
        {
            Debug.Log($"[TimeCheck] Time value changing from {playerStats[key]} to {value}. Stack Trace:\n{System.Environment.StackTrace}");
        }

        playerStats[key] = value;
        
        if (triggerEvent)
        {
            OnStateChange?.Invoke(key, (int)value);
        }

        // Check for game over condition
        if (value <= 0)
        {
            Debug.LogError($"[TimeCheck] {key} has reached {value}. Stack Trace:\n{System.Environment.StackTrace}");
            GameOverTrigger();
        }
    }

    private void HoldStateCheck(string key, float value)
    {
        if (playerStats[key] > 20 && playerHoldState[key] == true)
        {
            // Remove that Hold State; we can start Chance Card now for this key
            SetHoldState(key , false);
            Debug.Log($"[TimeCheck] Hold State Removed From - {key}");
        }
    }

    private void CheckForAlerts(bool checkChanceCard)
    {
        // Only check game over once
        bool isGameOver = GameOverTrigger();
        
        if (isGameOver)
        {
            Debug.Log("[TimeCheck] Game over condition detected in CheckForAlerts");
            return;
        }

        // Only check chance card if no game over and checkChanceCard is true
        if (checkChanceCard)
        {
            ChanceCardTrigger();
        }
    }

    private bool GameOverTrigger()
    {
        foreach (var stat in playerStats)
        {
            Debug.LogError($"[TimeCheck] Checking {stat.Key}: {stat.Value}");
            if (stat.Value <= 0)
            {
                Debug.LogError($"[TimeCheck] GAME OVER triggered by {stat.Key} at {stat.Value}");
                SaveFinalScores();
                isGameOverPending = true;
                return true;
            }
        }
        return false;
    }

    private void ChanceCardTrigger()
    {
        foreach (var stat in playerStats)
        {
            if (stat.Value <= 0)
            {
                Debug.Log($"[TimeCheck] {stat.Key} - IS 0... GAME OVER!");
                SaveFinalScores();  // Make sure to save scores
                isGameOverPending = true;
                return;
            }
        }
        // ... rest of chance card logic
    }

    // Method to set the player to the starting position
    public void SetPlayerStartPosition()
    {
        // Assuming playerController is attached to the player object
        if (playerController != null)
        {
            playerController.transform.position = new Vector3(-0.15f, 7.67f, 0f);  // Set position from your screenshot
             Debug.LogError("PlayerController positioned.");
        }
        else
        {
            Debug.LogError("PlayerController is not assigned.");
        }
    }    

    private void SetHoldState(string key , bool value)
    {
        playerHoldState[key] = value;
    }

    public void SetTime(float CurrentDayTime , float TotalTimePassed , int TotalDaysPassed)
    {
        currentTimeOfDay = CurrentDayTime;
        totalTimePassed = TotalTimePassed;        
        totalDaysPassed = TotalDaysPassed;
    }

    /// -----------------------------------------
    /// ------------    GAME OVER    ------------
    /// -----------------------------------------
    
    public void SaveFinalScores()
    {
        Debug.Log("[TimeCheck] Saving final scores to PlayerPrefs:");
        foreach (var stat in playerStats)
        {
            int value = (int)stat.Value;
            PlayerPrefs.SetInt($"Final{stat.Key}", value);
            Debug.Log($"[TimeCheck] Saved {stat.Key}: {value}");
        }
        PlayerPrefs.Save();
    }

    public void GameOver()
    {
        Debug.LogError("[TimeCheck] GameOver() called - About to load GameOver scene");
        SaveFinalScores();
        SceneManager.LoadScene("GameOver");
    }

    /// -----------------------------------------
    /// -----------    GAME VICTORY    ----------
    /// -----------------------------------------

    // This is called in TimeManager's Update()
    public void GameVictory()
    {
        // Save final scores before transitioning
        SaveFinalScores();
        
        // Disable the player controller if it exists
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.gameObject.SetActive(false);
        }
        
        // Load the victory scene
        SceneManager.LoadScene("GameVictory");
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("SceneManager.sceneLoaded event attached.");
       
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check for both possible scene names
        if (scene.name == "MainScene" || scene.name == "main")
        {
            Debug.Log($"Main scene loaded. Scene name: {scene.name}");
            
            // Ensure we have a player controller
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
                Debug.Log($"Looking for PlayerController: {(playerController != null ? "Found" : "Not Found")}");
            }

            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.gameObject.SetActive(true);
                SetPlayerStartPosition();
                Debug.Log("PlayerController activated and positioned");
            }
            else
            {
                Debug.LogError("PlayerController not found in main scene!");
            }
        }
    }

    // Add these methods to set the values
    public void SaveTimeState(float currentTimeOfDay, float totalTimePassed)
    {
        CurrentTimeOfDay = currentTimeOfDay;
        TotalTimePassed = totalTimePassed;
        HasSavedTimeState = true;
    }

    // Let the scene manager handle the actual transition
    public bool IsGameOverPending()
    {
        return isGameOverPending;
    }

    public void TriggerGameOver()
    {
        Debug.LogWarning($"[TimeCheck] TriggerGameOver called. isGameOverPending: {isGameOverPending}");
        if (isGameOverPending)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "main")
            {
                Debug.LogWarning("[TimeCheck] In main scene, loading GameOver scene...");
                isGameOverPending = false;
                GameOver();
            }
            else
            {
                Debug.LogWarning($"[TimeCheck] Not in main scene ({currentScene}), game over will trigger when returning to main");
            }
        }
    }

    public void ResetAllValues()
    {
        // Reset all player values to their starting defaults
        SetPlayerValue("Money", 0, false);
        SetPlayerValue("Career", 0, false);
        SetPlayerValue("Energy", 100, false);
        SetPlayerValue("Health", 100, false);
        SetPlayerValue("Creativity", 0, false);
        SetPlayerValue("Time", 0, false);
        
        // Reset any other state variables
        HasSavedTimeState = false;
        CurrentTimeOfDay = 0;
        TotalTimePassed = 0;
        
        // Reset hold states
        foreach (var key in playerHoldState.Keys.ToList())
        {
            playerHoldState[key] = false;
        }
        
        // Trigger UI update for each stat
        foreach (var stat in playerStats)
        {
            OnStateChange?.Invoke(stat.Key, (int)stat.Value);
        }
    }

    // Add this method
    public void SetChanceCardKey(string key)
    {
        chanceCardKey = key;
        Debug.Log($"Set chanceCardKey to: {key}");
    }

    // Add this helper method to check if any days have passed
    public bool HasAnyDaysPassed()
    {
        return totalDaysPassed > 0;
    }

    // Add these public methods to manage the summary state
    public static void SetSummaryActive(bool active)
    {
        s_SummaryActive = active;
        Debug.Log($"Summary active state set to: {active}");
    }

    public void SetGameOverPending(bool _isPending)
    {
        isGameOverPending = _isPending;
        Debug.Log($"Game over pending set to: {_isPending}");
    }
}

