using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle2D : MonoBehaviour {
    public UnityEngine.UI.Text countdownText;
    public float startTime = 0.6f;
    public Transform sunMoonPivot;
    public Transform moon;
    float currentTime = 0.0f; // This seems to be the time of day which ranges from 0-1
    public float timeSpeed = 1.0f;
    public SpriteRenderer background;
    public SpriteRenderer scatterGradient;
    public GameObject starField;
    public Color dayColor;
    public Color nightColor;
    public Color scatterColor = Color.red;
    bool day = true;
    public float transitionSpeed = 8.0f;
    float totalTimePassed = 0.0f; // This seems to be actual time that is passed.
    [HideInInspector]
    public bool timePaused = false;
    public float targetTimeYears;
    private PlayerState playerState;
    // Declare years, months, and days as class-level fields
    private int years = 0;
    private int months = 0;
    private int days = 0;

    void Start ()
    {
        // Save player state instance in a var
        playerState = PlayerState.Instance;

        currentTime = playerState.currentTimeOfDay;
        totalTimePassed = playerState.totalTimePassed;
    }

    void Update () {
        float newTimeDelta = Time.deltaTime * timeSpeed;
        if (!timePaused) {
            currentTime += newTimeDelta;    
        }

        totalTimePassed += newTimeDelta;

        if (currentTime >= 1.0f) {
            currentTime = 0.0f;
        }
        
        if (sunMoonPivot) {
            sunMoonPivot.eulerAngles = new Vector3(0.0f, 0.0f, -360.0f * currentTime);

            if (moon)
                moon.localEulerAngles = new Vector3(0.0f, 0.0f, 360.0f * currentTime);
        }
        
        if (background) {
            float colorValue = 0.0f;

            if (currentTime >= 0.0f && currentTime <= 0.5f) {
                colorValue = currentTime * transitionSpeed;

                if (colorValue > 1.0f)
                    colorValue = 1.0f;

                day = true;
            } else if (currentTime > 0.5f && currentTime < 1.0f) {
                colorValue = (currentTime * transitionSpeed) - transitionSpeed/2.0f;
                
                if (colorValue > 1.0f)
                    colorValue = 1.0f;
                
                day = false;
            }

            if (day) {
                background.color = Color.Lerp(nightColor, dayColor, colorValue);
            } else {
                background.color = Color.Lerp(dayColor, nightColor, colorValue);
            }

            if (scatterGradient) {
                if (colorValue < .5) {
                    scatterGradient.color = Color.Lerp(Color.clear, scatterColor, colorValue * 2.0f);
                } else {
                    scatterGradient.color = Color.Lerp(scatterColor, Color.clear, (colorValue - .5f) * 2.0f);
                }
            }

            if (starField) {
                foreach (SpriteRenderer starFieldRenderer in starField.GetComponentsInChildren<SpriteRenderer>()) {
                    if (!day) {
                        starFieldRenderer.color = Color.Lerp(Color.clear, Color.white, colorValue * 2.0f);
                    } else {
                        starFieldRenderer.color = Color.Lerp(Color.white, Color.clear, colorValue * 2.0f);
                    }
                }
            }
        }

        // Calculate the total time passed in years
        float yearsPassed = totalTimePassed / 365.0f;
        float remainingYears = targetTimeYears - yearsPassed;

        // Convert the remaining years into total days
        int totalRemainingDays = Mathf.FloorToInt(remainingYears * 365.0f);

        // Calculate years, months, and days from totalRemainingDays
        years = totalRemainingDays / 365;
        int remainingDays = totalRemainingDays % 365;
        months = remainingDays / 30;
        days = remainingDays % 30;

        // Update the countdown text to show years, months, and days
        countdownText.text = string.Format("{0}y {1:D2}m {2:D2}d", years, months, days);


        // Some Testing Methods

        /*//speed up or slow down time speed with key presses
        if (Input.GetKeyDown(KeyCode.F))
        {
            IncreaseTimeSpeed();
        }

        // Check for the 's' key press for slower speed
        if (Input.GetKeyDown(KeyCode.S))
        {
            DecreaseTimeSpeed();
        }

        // Check for the 'r' key press for reset speed
        if (Input.GetKeyDown(KeyCode.R))
        {
            timeSpeed = 0.05f;
        }*/
    }

    public float TotalTimePassed() { 
        return totalTimePassed; 
    }

    public void PauseAndUnPause() {
        timePaused = !timePaused;
    }

    //Set new time
    public void SetNewTime(float newTime) {
        currentTime = newTime;
    }

    //decrease or increase speed functions
    // Test Method
    public void IncreaseTimeSpeed() 
    {
        timeSpeed *= 2;
    }

    // Test Method
    public void DecreaseTimeSpeed() 
    {
        timeSpeed /= 2;
    }    

    public int GetCurrentYears() {
        // Assuming you've already calculated "years" in your update method
        return years;
    }

    public int GetCurrentMonths() {
        return months;
    }

    public int GetCurrentDays() {
        return days;
    }
}
