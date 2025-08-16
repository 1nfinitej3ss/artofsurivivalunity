using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlanetRunner;  // Add this for PlayerController

public class ProgressBarChanges : MonoBehaviour 
{
    [Header("UI References")]
    [SerializeField, Tooltip("The Monthly Summary panel GameObject")]
    private GameObject MonthlySummary;
    [SerializeField, Tooltip("The TextMeshProUGUI component for displaying monthly values")]
    private TextMeshProUGUI monthlySummaryValueText;
    public GameObject YearlySummary;
    public GameObject YearlySummaryOptions;
    public GameObject YearlySummaryResult;
    public TimeManager timeManager;

    private PlayerState playerState;
    private ProgressBar moneyProgressBar;
    

    private void OnEnable()
    {
        Debug.Log("ProgressBarChanges: OnEnable - Subscribing to events");
        TimeManager.OnMonthPassed += HandleMonthPassed;
        TimeManager.OnDayPassed += HandleDayPassed;
    }

    void Start()
    {
        Debug.Log("ProgressBarChanges: Start method called");
        
        // Ensure we have a reference to TimeManager
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
            if (timeManager == null)
            {
                Debug.LogError("TimeManager reference not found!");
                return;
            }
        }

        // Verify UI components
        if (MonthlySummary == null)
        {
            Debug.LogError("Monthly Summary GameObject is null!");
        }
        if (monthlySummaryValueText == null)
        {
            Debug.LogError("Monthly Summary Value Text component is null!");
        }

        // Verify PlayerState
        playerState = PlayerState.Instance;
        if (playerState == null)
        {
            Debug.LogError("PlayerState not found!");
            return;
        }

        // Double-check event subscription
        Debug.Log($"Monthly event has {TimeManager.GetMonthlySubscriberCount()} subscribers after Start");

        MonthlySummary?.SetActive(false);
        SetYearlySummaryTime();
    }

    private void OnDisable()
    {
        Debug.Log("ProgressBarChanges: OnDisable - Unsubscribing from events");
        TimeManager.OnDayPassed -= HandleDayPassed;
        TimeManager.OnMonthPassed -= HandleMonthPassed;
    }

    // Method to handle what happens when a day passes
    private void HandleDayPassed()
    {
        CheckYearlySummary();
    }

    // Method to handle what happens when a month passes
    private void HandleMonthPassed()
    {
        Debug.Log($"Month passed - handling monthly deductions (Day: {timeManager.totalDaysPassed})");
        
        // Verify we're in the correct scene
        Debug.Log($"Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Verify components
        if (timeManager == null)
        {
            Debug.LogError("TimeManager is null in HandleMonthPassed!");
            return;
        }

        if (PlayerState.Instance == null)
        {
            Debug.LogError("PlayerState is null during monthly deduction!");
            return;
        }

        // First apply any monthly effects from questions
        QuestionTemplate.ApplyMonthlyEffects();

        // Add check for UI components
        if (MonthlySummary == null || monthlySummaryValueText == null)
        {
            Debug.LogError("Monthly Summary UI components missing during monthly update!");
            return;
        }

        Debug.Log($"Current monthly charges - Rent: {PlayerState.Instance.GetMonthlyCharges("Rent")}, " +
                  $"Utilities: {PlayerState.Instance.GetMonthlyCharges("Utilities")}, " +
                  $"Groceries: {PlayerState.Instance.GetMonthlyCharges("Groceries")}");

        int totalCharges = PlayerState.Instance.GetMonthlyCharges("Rent") + 
                          PlayerState.Instance.GetMonthlyCharges("Utilities") + 
                          PlayerState.Instance.GetMonthlyCharges("Groceries");

        PlayerState.Instance.DeductMonthlyCharges();
        
        // Add check for time scale
        if (Time.timeScale == 0)
        {
            Debug.LogWarning("Time is paused during monthly update!");
        }
        
        EnableMonthlySummary();
        
        Debug.Log($"Monthly charges deducted: {totalCharges}");
    }

    /// -----------------------------------------
    /// ---------    MONTHLY CHARGES    ---------
    /// -----------------------------------------

    private void EnableMonthlySummary()
    {
        Debug.Log("Attempting to enable monthly summary");
        
        if (MonthlySummary == null || monthlySummaryValueText == null)
        {
            Debug.LogError("Monthly Summary UI components missing!");
            return;
        }

        // Set summary active flag
        PlayerState.SetSummaryActive(true);

        // Format the text with current values
        string summaryText = $"Monthly Expenses:\n\n" +
                           $"Rent: -${PlayerState.Instance.GetMonthlyCharges("Rent")}\n" +
                           $"Utilities: -${PlayerState.Instance.GetMonthlyCharges("Utilities")}\n" +
                           $"Groceries: -${PlayerState.Instance.GetMonthlyCharges("Groceries")}";

        monthlySummaryValueText.text = summaryText;
        MonthlySummary.SetActive(true);
        Debug.Log("Monthly Summary panel displayed");
    }

    public void DisableMonthlySummary()
    {
        Time.timeScale = 1f;  // Resumes the game
        
        // Clear summary active flag
        PlayerState.SetSummaryActive(false);

        // Disable Summary
        MonthlySummary.SetActive(false);

        // Now check for chance card conditions
        if (PlayerState.Instance != null)
        {
            string[] statsToCheck = { "Money", "Career", "Energy", "Creativity", "Time" };
            foreach (var stat in statsToCheck)
            {
                float value = PlayerState.Instance.GetPlayerValue(stat);
                // Check if value is in critical range (0-20)
                if (value > 0 && value <= 20)
                {
                    Debug.Log($"{stat} is at critical level: {value}");
                    var playerController = FindObjectOfType<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.StartChanceCard();
                    }
                    break;
                }
            }
        }
    }

    /// -----------------------------------------
    /// ---------    YEARLY CHARGES    ----------
    /// -----------------------------------------

    private void SetYearlySummaryTime()
    {
        // If our player state has already saved an yearly list
        if(playerState.yearlySummaryTimeList.Count == 5)
        {
            //Debug.Log("Player has Yearly Summary Saved, no new list will be made!");
        }

        else
        {
            // Initialize list and set it's capacity to 0
            List<int> yearlySummaryTimeList = new List<int>(0);

            // This For Loop simply adds 5 times (each time is more than the previous one) on which Yearly Summary will open
            for (int yearNumber = 0; yearNumber < timeManager.dayNightCycle.targetTimeYears; yearNumber++)
            {
                // Make an int to store a temp value of when we will start yearlySummary next year
                int yearlySummaryTime = Random.Range(180, 365);

                // Make final variable which will be the time we will be adding in our list
                int timeToAdd;

                // Set its value
                if (yearNumber == 0) // This will run for the first year
                {
                    timeToAdd = yearlySummaryTime;
                }

                else // This will run for all years after the first one
                {
                    // Simply our passed days + our random yearly time (which are also days)
                    timeToAdd = ((yearNumber) * 365) + yearlySummaryTime;
                }

                // Add the final time to our list
                yearlySummaryTimeList.Add(timeToAdd);
                
                // Show in console
               // Debug.Log($"Year No. {yearNumber + 1} - will show Yearly Summary on Day {timeToAdd}");
            }

            // Save our list in the playerState
            playerState.yearlySummaryTimeList = yearlySummaryTimeList;
        }
    }

    public void CheckYearlySummary()
    {
        foreach (int time in playerState.yearlySummaryTimeList)
        {
            if (timeManager.totalDaysPassed == time)
            {
                // Show Yearly summary
                EnableYearlySummary();
            }
        }
    }

    private void EnableYearlySummary()
    {
        // Set summary active flag
        PlayerState.SetSummaryActive(true);

        // Enable Summary
        YearlySummary.SetActive(true);
        YearlySummaryOptions.SetActive(true);
        YearlySummaryResult.SetActive(false);

        Debug.Log("Yearly Summary is Opened On Day " + timeManager.totalDaysPassed);
    }

    public void DisableYearlySummary()
    {
        Time.timeScale = 1f;  // Resumes the game

        // Clear summary active flag
        PlayerState.SetSummaryActive(false);

        // Disable Summary
        YearlySummary.SetActive(false);

        // Now check for chance card conditions
        if (PlayerState.Instance != null)
        {
            string[] statsToCheck = { "Money", "Career", "Energy", "Creativity", "Time" };
            foreach (var stat in statsToCheck)
            {
                float value = PlayerState.Instance.GetPlayerValue(stat);
                // Check if value is in critical range (0-20)
                if (value > 0 && value <= 20)
                {
                    Debug.Log($"{stat} is at critical level: {value}");
                    var playerController = FindObjectOfType<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.StartChanceCard();
                    }
                    break;
                }
            }
        }
    }

    // This is set on OnBtnClick() of Upgrade Btn of Yearly Summary Obj
    public void OnYearlySummaryUpgrade()
    {
        // Update our monthly rent by +15 (+5 for rent, +5 for grocereis, +5 for utilities)
        PlayerState.Instance.SetMonthlyCharges(5 , 5 , 5);

        // Show in console
       // Debug.Log("Upgraded Living Standards via Yearly Summary!");
    }

    public void OnUpgradeEnvironment()
    {
        // First modify the charges...
        int currentRent = PlayerState.Instance.GetMonthlyCharges("Rent");
        int currentUtilities = PlayerState.Instance.GetMonthlyCharges("Utilities");
        int currentGroceries = PlayerState.Instance.GetMonthlyCharges("Groceries");
        
        PlayerState.Instance.SetMonthlyCharges(
            currentRent + 5,
            currentUtilities + 5,
            currentGroceries
        );
        
        // Check if this is a yearly or monthly summary and disable accordingly
        if (YearlySummary != null && YearlySummary.activeSelf)
        {
            DisableYearlySummary();
        }
        else
        {
            DisableMonthlySummary();
        }
        
        Debug.Log("Living environment upgraded - Monthly charges increased");
    }

    public void OnDowngradeEnvironment()
    {
        // First modify the charges...
        int currentRent = PlayerState.Instance.GetMonthlyCharges("Rent");
        int currentUtilities = PlayerState.Instance.GetMonthlyCharges("Utilities");
        int currentGroceries = PlayerState.Instance.GetMonthlyCharges("Groceries");
        
        PlayerState.Instance.SetMonthlyCharges(
            currentRent - 5,
            currentUtilities - 5,
            currentGroceries
        );
        
        // Check if this is a yearly or monthly summary and disable accordingly
        if (YearlySummary != null && YearlySummary.activeSelf)
        {
            DisableYearlySummary();
        }
        else
        {
            DisableMonthlySummary();
        }
        
        Debug.Log("Living environment downgraded - Monthly charges decreased");
    }
}
