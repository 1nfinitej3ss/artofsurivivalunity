using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public DayNightCycle2D dayNightCycle;

    public static event Action OnDayPassed; // Event that fires when a day passes
    public static event Action OnMonthPassed; // Event that fires when a month passes

    public int totalDaysPassed;
    private float lastDayTime;

    private PlayerState playerState;

    void Start() 
    {
        playerState = PlayerState.Instance;

        totalDaysPassed = playerState.totalDaysPassed;
        lastDayTime = dayNightCycle.TotalTimePassed();
    }

    // Add a public method to check subscriber count
    public static int GetMonthlySubscriberCount()
    {
        return OnMonthPassed?.GetInvocationList()?.Length ?? 0;
    }

    void Update() 
    {
        float currentDayTime = dayNightCycle.TotalTimePassed();

        if (Mathf.Floor(currentDayTime) > Mathf.Floor(lastDayTime))
        {
            totalDaysPassed += 1;
            playerState.SetTime(playerState.currentTimeOfDay, playerState.totalTimePassed, totalDaysPassed);
            
            Debug.Log($"Day passed: {totalDaysPassed}"); // Track every day

            OnDayPassed?.Invoke();

            if (totalDaysPassed % 30 == 0)
            {
                // Add detailed logging for month trigger
                Debug.Log($"Month trigger - Day {totalDaysPassed} is divisible by 30");
                Debug.Log($"Current subscribers to OnMonthPassed: {GetMonthlySubscriberCount()}");
                
                // Check if time is paused
                if (Time.timeScale == 0)
                {
                    Debug.LogWarning("Time is paused during month trigger!");
                }

                OnMonthPassed?.Invoke();
            }

            if (totalDaysPassed == 365 * 5)
            {
                Debug.Log("Game Victory --- 5 Years Have Been Passed");
                if (PlayerState.Instance != null)
                {
                    PlayerState.Instance.GameVictory();
                }
            }

            lastDayTime = currentDayTime;
        }
    }

    #if UNITY_EDITOR
    // Method to set days for testing
    private void SetTotalDaysForTesting(int days)
    {
        totalDaysPassed = days;
        playerState.SetTime(playerState.currentTimeOfDay, playerState.totalTimePassed, totalDaysPassed);
        
        // Check victory condition immediately
        if (totalDaysPassed >= 365 * 5)
        {
            Debug.Log("Game Victory --- 5 Years Have Been Passed (Test Mode)");
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.GameVictory();
            }
        }
    }
    
    // Quick test method to jump to near victory
    [ContextMenu("Test Near Victory")]
    private void TestNearVictory()
    {
        SetTotalDaysForTesting(365 * 5 - 1); // Set to 1 day before victory
    }

    // Quick test method to trigger victory
    [ContextMenu("Test Victory")]
    private void TestVictory()
    {
        SetTotalDaysForTesting(365 * 5);
    }
    #endif
}
