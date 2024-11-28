using UnityEngine;
using System.Collections.Generic;
using PlanetRunner;
using UnityEngine.SceneManagement;

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
    public string chanceCardKey {  get; private set; }

    // Initialization
    private void Awake()
    {
        yearlySummaryTimeList = new List<int>(0);

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

    private void Start()
    {
        // Set those charges once in the start of game, they won't be reset to this again as Start() wont be called again until we restart game.
        SetMonthlyCharges(30 , 10 , 10);

        // Set the player's starting position when the scene loads
        SetPlayerStartPosition();
    }

    // Function to Set Monthly Charges
    /// <summary>
    /// Increment or Decrement in the existing monthly charges. Type -(value) for decrement
    /// </summary>
    /// <param name="rent"></param>
    /// <param name="utilities"></param>
    /// <param name="groceries"></param>
    public void SetMonthlyCharges(int rent , int utilities , int groceries)
    {
        // Set Rent
        monthlyCharges["Rent"] = monthlyCharges["Rent"] + rent;
        //Debug.Log($"Monthly Charges (Rent) have been updated by {rent}");

        // Set Utilities
        monthlyCharges["Utilities"] = monthlyCharges["Utilities"] + utilities;
        //Debug.Log($"Monthly Charges (Utilities) have been updated by {utilities}");

        // Set Groceries
        monthlyCharges["Groceries"] = monthlyCharges["Groceries"] + utilities;
        //Debug.Log($"Monthly Charges (Groceries) have been updated by {groceries}");
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
        // Save a temp var of the amount of money we have
        float currentMoney = GetPlayerValue("Money");

        // Deduct the amount of each item in our monthlyCharges from our currentMoney
        foreach (var item in monthlyCharges)
        {
            currentMoney -= item.Value;
        }

        // Update our actual "Money" Attribute by replacing it with our temp currentMoney
        SetPlayerValue("Money", currentMoney, true);
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
    public void SetPlayerValue(string key, float value , bool checkChanceCard)
    {
        playerStats[key] = value;
        OnStateChange?.Invoke(key, (int)value);

        // If our value is in hold state and we jsut set its value above 20, remove it from hold state state
        HoldStateCheck(key , value);

        // Game Over + Chance Card Checking
        CheckForAlerts(checkChanceCard);
    }

    private void HoldStateCheck(string key, float value)
    {
        if (playerStats[key] > 20 && playerHoldState[key] == true)
        {
            // Remove that Hold State; we can start Chance Card now for this key
            SetHoldState(key , false);

            //Debug.Log($"Hold State Removed From - {key}");
        }
    }

    private void CheckForAlerts(bool checkChanceCard)
    {
        // Addind the gameover condition prevents chance card scene to open after the game is over
        // Adding checkChanceCard condition becuase we don't want to check Chance Card always (For exmaple just coming out of Chance Card)
        if (!GameOverTrigger() && checkChanceCard)
        {
            ChanceCardTrigger();
        }
    }

    private bool GameOverTrigger()
    {
        // Make temp bool to return either game is over or not
        bool isGameOver = false;

        foreach (var stat in playerStats)
        {
            if (stat.Value <= 0)
            {
                // Console Awareness
                Debug.Log($"{stat.Key} - IS 0... GAME OVER!");

                // Add Game Over System Here
                isGameOver = true;
                GameOver();

                // Break our foreach loop so it doesn't start our game over logic again if there is more than one value = 0
                break;
            }
        }

        return isGameOver;
    }

    private void ChanceCardTrigger()
    {
        foreach (var stat in playerStats)
        {
            if (stat.Value <= 20 && playerHoldState[stat.Key] != true)
            {
                // Console Awareness
                Debug.Log($"{stat.Key} has reached 20 or less... CHANCE CARD OPENING!");

                // Save which value has reached 20 or less; we need this for ChanceCard Title Text
                chanceCardKey = stat.Key;

                // Activate Hold State
                SetHoldState(stat.Key , true);
                Debug.Log($"Hold State Added To - {stat.Key}");

                // Update game mechanics + game controller that we are going to chance card
                // Adding an if condition becuase we only want this in NW live scene; check the GameController's OnSceneChanged() for more info.
                if(SceneManager.GetActiveScene().name == "NW live")
                {
                    gameController.OnSceneChanged();
                }

                // Add Chance Card Opening System Here
                playerController.StartChanceCard();

                // Break our foreach loop so it doesn't start our chance card again if there is more than one value below 20
                break;
            }
        }
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
    
    private void GameOver()
    {
        // Start Game Over!
        SceneManager.LoadScene(gameOverSceneName);
    }

    /// -----------------------------------------
    /// -----------    GAME VICTORY    ----------
    /// -----------------------------------------

    // This is called in TimeManager's Update()
    public void GameVictory()
    {
        // Start Game Victory!
        SceneManager.LoadScene(gameVictorySceneName);
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
        if (scene.name == "MainScene")
        {
            SetPlayerStartPosition(); // Reset the player's position
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.gameObject.SetActive(true); // Ensure the player is active
                //Debug.Log("PlayerController activated and positioned in MainScene.");
            }
            else
            {
                //Debug.LogError("PlayerController is not assigned.");
            }
            
        }
        //Debug.Log($"PlayerController position: {playerController.transform.position}, active: {playerController.gameObject.activeInHierarchy}");

    }
}

