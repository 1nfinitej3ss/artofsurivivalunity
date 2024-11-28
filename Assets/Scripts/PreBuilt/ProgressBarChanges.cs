using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProgressBarChanges : MonoBehaviour 
{
    public GameObject MonthlySummary;
    public GameObject YearlySummary;
    public GameObject YearlySummaryOptions;
    public GameObject YearlySummaryResult;
    public TimeManager timeManager;
    public TextMeshProUGUI monthlySummaryValueText;

    private PlayerState playerState;
    private ProgressBar moneyProgressBar;
    

    void Start()
    {
        // Subscribe to the Month, Day and Year Passed event
        TimeManager.OnMonthPassed += HandleMonthPassed;
        TimeManager.OnDayPassed += HandleDayPassed;

        // Find the ProgressBar with a TitleType of Money
        ProgressBar[] progressBars = FindObjectsOfType<ProgressBar>();
        foreach (ProgressBar progressBar in progressBars) 
        {
            if (progressBar.titleType == ProgressBar.TitleType.Money) 
            {
                moneyProgressBar = progressBar;
                break;
            }
        }
        
        if(moneyProgressBar == null) 
        {
            //Debug.LogError("No Money ProgressBar found in the scene!");
        }

        playerState = PlayerState.Instance;
        SetYearlySummaryTime();
    }

    // Method to handle what happens when a day passes
    private void HandleDayPassed()
    {
        CheckYearlySummary();
    }

    // Method to handle what happens when a month passes
    private void HandleMonthPassed()
    {
        // Deduct Monthly Charges
        PlayerState.Instance.DeductMonthlyCharges();

        // Enable Monthly Summary
        EnableMonthlySummary();
    }

    // Unsubscribe from the OnDayPassed event when the object is destroyed
    private void OnDestroy()
    {
        TimeManager.OnDayPassed -= HandleDayPassed;
        TimeManager.OnMonthPassed -= HandleMonthPassed;
    }

    /// -----------------------------------------
    /// ---------    MONTHLY CHARGES    ---------
    /// -----------------------------------------

    private void EnableMonthlySummary()
    {
        // Update Values of Monthly Summary
        monthlySummaryValueText.text = $"Rent (-{PlayerState.Instance.GetMonthlyCharges("Rent")})\nUtilities (-{PlayerState.Instance.GetMonthlyCharges("Utilities")})\nGroceries (-{PlayerState.Instance.GetMonthlyCharges("Groceries")})";

        // Enable Summary
        MonthlySummary.SetActive(true);

        // Show Update In Console
        Debug.Log("A Month Has Been Passed --- Rent + Utilities + Groceries = Total -50 Money --- Have Been Deducted");

        // Stop Time
        Time.timeScale = 0f;
    }

    // This is set to OnBtnClick() of NextBtn present on the MonthlySummary Object in NW live scene
    public void DisableMonthlySummary()
    {
        // Start Time
        Time.timeScale = 1f;

        // Disable Summary
        MonthlySummary.SetActive(false);

        //Debug.Log("Monthly Summary Disabled!");
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
        // Enable Summary
        YearlySummary.SetActive(true);
        YearlySummaryOptions.SetActive(true);
        YearlySummaryResult.SetActive(false);

       // Debug.Log("Yearly Summary is Opened On Day " + timeManager.totalDaysPassed);

        // Stop Time
        Time.timeScale = 0f;
    }

    // This is set to OnBtnClick() of ContinueBtn present on the Options Panel on YearlySummary Object in NW live scene
    // Also on the Result's NextBtn on the same Yaerly Summary Obj
    public void DisableYearlySummary()
    {
        // Start Time
        Time.timeScale = 1f;

        // Disable Summary
        YearlySummary.SetActive(false);

       // Debug.Log("Yearly Summary Disabled!");
    }

    // This is set on OnBtnClick() of Upgrade Btn of Yearly Summary Obj
    public void OnYearlySummaryUpgrade()
    {
        // Update our monthly rent by +15 (+5 for rent, +5 for grocereis, +5 for utilities)
        PlayerState.Instance.SetMonthlyCharges(5 , 5 , 5);

        // Show in console
       // Debug.Log("Upgraded Living Standards via Yearly Summary!");
    }

    
}
