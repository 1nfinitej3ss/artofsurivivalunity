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

    void Update() 
    {
        float currentDayTime = dayNightCycle.TotalTimePassed();

        // Check if a day has passed
        if (Mathf.Floor(currentDayTime) > Mathf.Floor(lastDayTime))
        {
            // Add +1 Day to our totalDaysPassed
            totalDaysPassed += 1;

            // Save our totalPassedDays in PlayerStats to sync across scenes
            playerState.SetTime(playerState.currentTimeOfDay , playerState.totalTimePassed , totalDaysPassed);

            //Debug.Log("Total Days Passed " + totalDaysPassed);

            // If a day has passed, invoke the OnDayPassed event
            OnDayPassed?.Invoke();

            // Check if a month has passed
            if (totalDaysPassed % 30 == 0)
            {
                // If a month has passed, invoke the OnMonthPassed event
                OnMonthPassed?.Invoke();                
            }

            // Check if 5 Years have passed
            if (totalDaysPassed == 365 * 5)
            {
                // Show in Console
                Debug.Log("Game Win --- 5 Years Have Been Passed");

                // Game Win
                playerState.GameVictory();
            }
        }

        lastDayTime = currentDayTime;
    }
}
