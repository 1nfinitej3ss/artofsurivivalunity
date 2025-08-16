using UnityEngine;

public class TimeSpeedController : MonoBehaviour
{
    public DayNightCycle2D dayNightCycle;
    public Rotation rotationScript;
    
    [Header("Time Speed Settings")]
    public float[] timeSpeedPresets = { 0.5f, 1.0f, 2.0f, 4.0f, 8.0f };
    private int currentPresetIndex = 1; // Start at normal speed (1.0f)

    void Start()
    {
        if (dayNightCycle == null)
        {
            dayNightCycle = FindObjectOfType<DayNightCycle2D>();
        }
        if (rotationScript == null)
        {
            rotationScript = FindObjectOfType<Rotation>();
        }
    }

    void Update()
    {
        // Press Space to cycle through time speeds
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPresetIndex = (currentPresetIndex + 1) % timeSpeedPresets.Length;
            float newSpeed = timeSpeedPresets[currentPresetIndex];
            
            if (dayNightCycle != null)
            {
                dayNightCycle.SetTimeSpeed(newSpeed);
                if (rotationScript != null)
                {
                    rotationScript.SetTimeSpeedManually();
                }
            }
            
            Debug.Log($"Time speed set to: {newSpeed}x");
        }
    }
} 