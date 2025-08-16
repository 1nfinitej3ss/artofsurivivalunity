using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle2D : MonoBehaviour {
    public UnityEngine.UI.Text countdownText;
    public float startTime = 0.6f;
    public Transform sunMoonPivot;
    public Transform moon;
    float currentTime = 0.0f; // This seems to be the time of day which ranges from 0-1
    [SerializeField] private float m_TimeSpeed = 1.0f;
    public float timeSpeed 
    { 
        get => m_TimeSpeed;
        set => m_TimeSpeed = value;
    }
    [SerializeField, Range(0.1f, 2.0f)] private float m_DayNightCycleSpeed = 1.0f; // Separate speed for visual day/night cycle
    public float dayNightCycleSpeed
    {
        get => m_DayNightCycleSpeed;
        set => m_DayNightCycleSpeed = value;
    }
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
    public float targetTimeYears = 5f; // Set default to 5 years
    private PlayerState playerState;
    // Declare years, months, and days as class-level fields
    private int years = 0;
    private int months = 0;
    private int days = 0;
    public GameObject[] clouds; // Array to hold cloud GameObjects
    private bool hasRepositionedClouds = false;
    public float minCloudX = -10f; // Adjust these values based on your scene
    public float maxCloudX = 10f;  // Adjust these values based on your scene
    [SerializeField] private TimeData m_TimeData;

    void Start ()
    {
        // Save player state instance in a var
        playerState = PlayerState.Instance;

        // Load the saved time values from PlayerState
        if (playerState.HasSavedTimeState)
        {
            currentTime = playerState.CurrentTimeOfDay;
            totalTimePassed = playerState.TotalTimePassed;
            
            // Calculate remaining time instead of elapsed time
            CalculateRemainingTime();
        }
        else
        {
            currentTime = startTime;
            totalTimePassed = 0f;
            // Start with 5 years
            years = 5;
            months = 0;
            days = 0;
        }

        // Update the countdown text immediately
        UpdateCountdownText();
    }

    // Add this helper method to update the countdown text
    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            string timeText;
            
            // Check if any days have passed using PlayerState
            if (PlayerState.Instance != null && !PlayerState.Instance.HasAnyDaysPassed())
            {
                timeText = "5 years left";
            }
            else
            {
                timeText = $"{years}y {months:D2}m {days:D2}d";
            }
            
            countdownText.text = timeText;
            
            // Update the ScriptableObject
            if (m_TimeData != null)
            {
                m_TimeData.CountdownText = timeText;
                m_TimeData.Years = years;
                m_TimeData.Months = months;
                m_TimeData.Days = days;
            }
        }
    }

    void Update () {
        // Use unscaled time for visual elements to make them completely independent of game time
        float visualTimeDelta = Time.unscaledDeltaTime * m_DayNightCycleSpeed * 0.5f; // The 0.5f slows it down to a reasonable speed
        if (!timePaused) {
            currentTime += visualTimeDelta;    
        }

        // Use timeSpeed for game time progression - affects days passing, events, etc.
        if (!timePaused) {
            totalTimePassed += Time.deltaTime * m_TimeSpeed;
        }

        if (currentTime >= 1.0f) {
            currentTime = 0.0f;
        }
        
        // Visual elements (sun/moon rotation) use currentTime which is controlled by dayNightCycleSpeed
        if (sunMoonPivot) {
            sunMoonPivot.eulerAngles = new Vector3(0.0f, 0.0f, -360.0f * currentTime);

            if (moon)
                moon.localEulerAngles = new Vector3(0.0f, 0.0f, 360.0f * currentTime);
        }
        
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
        
        if (background) {
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

        if (clouds != null && clouds.Length > 0) {
            foreach (GameObject cloud in clouds) {
                MeshRenderer cloudRenderer = cloud.GetComponent<MeshRenderer>();
                if (cloudRenderer != null) {
                    Material cloudMaterial = cloudRenderer.material;
                    if (!day) {
                        // During night, fade clouds out
                        float targetAlpha = Mathf.Lerp(1f, 0f, colorValue);
                        Color currentColor = cloudMaterial.color;
                        cloudMaterial.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                        
                        // When clouds are fully transparent, reposition them
                        if (targetAlpha <= 0.01f && !hasRepositionedClouds) {
                            Vector3 position = cloud.transform.position;
                            position.x = Random.Range(minCloudX, maxCloudX);
                            cloud.transform.position = position;
                            hasRepositionedClouds = true;
                        }
                    } else {
                        // Reset the flag when transitioning to day
                        if (colorValue <= 0.01f) {
                            hasRepositionedClouds = false;
                        }
                        
                        // During day, fade clouds in
                        float targetAlpha = Mathf.Lerp(0f, 1f, colorValue);
                        Color currentColor = cloudMaterial.color;
                        cloudMaterial.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
                    }
                }
            }
        }

        // Calculate remaining time every frame
        CalculateRemainingTime();
        
        // Update the countdown text
        UpdateCountdownText();
        
        // Save the current time state to PlayerState
        playerState.SaveTimeState(currentTime, totalTimePassed);

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

    // Add this method to properly calculate remaining time
    private void CalculateRemainingTime()
    {
        // Calculate total days passed
        float totalDays = Mathf.Floor(totalTimePassed);
        
        // Calculate remaining time from 5 years (1825 days)
        float remainingDays = (365 * 5) - totalDays;
        
        // Start with full years and months
        years = 4;  // Start with 4 since we're showing the remaining full years
        months = 12;
        days = 30;
        
        // Subtract the passed days
        if (totalDays > 0)
        {
            // First reduce days
            days = 30 - (Mathf.FloorToInt(totalDays) % 30);
            
            // If we've passed at least 30 days, reduce months
            int totalMonthsPassed = Mathf.FloorToInt(totalDays / 30);
            if (totalMonthsPassed > 0)
            {
                months = 12 - (totalMonthsPassed % 12);
                
                // If we've passed at least 12 months, reduce years
                int yearsPassed = totalMonthsPassed / 12;
                if (yearsPassed > 0)
                {
                    years = 4 - yearsPassed;  // Start from 4 years since we're showing remaining full years
                }
            }
            
            // Adjust for month/year rollovers
            if (days == 30)
            {
                days = 0;
                months++;
                if (months > 12)
                {
                    months = 1;
                    years++;
                }
            }
        }
        else
        {
            // If no days have passed, show "5 years left"
            years = 5;
            months = 12;
            days = 30;
        }
    }

    // Add this new method near the other public methods
    public void SetTimeSpeed(float newSpeed)
    {
        m_TimeSpeed = newSpeed;
        Debug.Log($"Time speed set to: {newSpeed}");
    }
}
