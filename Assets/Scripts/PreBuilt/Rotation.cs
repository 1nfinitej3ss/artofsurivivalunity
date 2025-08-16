using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float defaultRotationRate = 10f; // Default rotation rate
    public float boostedRotationRate = 30f; // Faster rotation rate when the cursor is near poles
    public float transitionSpeed = 5f; // Speed of transitioning between rotation rates
    public float poleThreshold = 0.1f; // Threshold for how close to the poles (east/west) the cursor should be

    public float minTimeSpeed = 1.0f; // Minimum time speed when rotation is slow
    public float maxTimeSpeed = 8.0f; // Maximum time speed when rotation is fast

    private float currentRotationRate;
    public DayNightCycle2D dayNightCycle; // Reference to the DayNightCycle2D script
    private bool timeSpeedManuallySet = false;

    // Reference to the globe object (the part that should rotate with cursor)
    public Transform globeTransform;

    void Start()
    {
        currentRotationRate = defaultRotationRate; // Initialize with the default rotation rate
        
        // Add debug logging
        if (dayNightCycle == null)
        {
            Debug.LogError("Rotation: DayNightCycle2D reference is missing! Please assign it in the Inspector.");
            enabled = false;
            return;
        }
        
        // Only set the day/night cycle speed if it hasn't been manually set in the Inspector
        if (dayNightCycle.dayNightCycleSpeed == 1.0f)
        {
            dayNightCycle.dayNightCycleSpeed = 1.0f;
        }
        Debug.Log($"Rotation: Connected to DayNightCycle2D. Day/Night cycle speed: {dayNightCycle.dayNightCycleSpeed}, Game time speed: {dayNightCycle.timeSpeed}");

        // If globeTransform is not set, use this transform
        if (globeTransform == null)
        {
            globeTransform = transform;
        }
    }

    void Update()
    {
        // Check if the cursor is near the east or west pole
        if (IsCursorAtPole())
        {
            // If the cursor is at the poles, increase the rotation rate
            currentRotationRate = Mathf.Lerp(currentRotationRate, boostedRotationRate, Time.deltaTime * transitionSpeed);
            Debug.Log($"Rotation: At pole - Setting game time speed to {maxTimeSpeed}x");
        }
        else
        {
            // If the cursor is not at the poles, return to the default rotation rate
            currentRotationRate = Mathf.Lerp(currentRotationRate, defaultRotationRate, Time.deltaTime * transitionSpeed);
            Debug.Log($"Rotation: At center - Setting game time speed to {minTimeSpeed}x");
        }

        // Rotate only the globe (not the sun/moon) based on the current rotation rate
        globeTransform.Rotate(Vector3.forward, currentRotationRate * Time.deltaTime);

        // Only adjust game time speed if it hasn't been manually set
        if (dayNightCycle != null && !timeSpeedManuallySet)
        {
            // Calculate a normalized speed factor based on the current rotation rate
            float normalizedSpeed = (currentRotationRate - defaultRotationRate) / (boostedRotationRate - defaultRotationRate);
            // Use the normalized speed to set the time speed within the min and max range
            float newSpeed = Mathf.Lerp(minTimeSpeed, maxTimeSpeed, Mathf.Clamp01(normalizedSpeed));
            dayNightCycle.timeSpeed = newSpeed;
            Debug.Log($"Rotation: Setting game time speed to {newSpeed}x (Day/Night cycle remains at {dayNightCycle.dayNightCycleSpeed}x)");
        }
    }

    // Method to check if the cursor is at the east or west pole
    private bool IsCursorAtPole()
    {
        // Get the cursor position in screen coordinates
        Vector3 cursorPosition = Input.mousePosition;

        // Convert the cursor position to viewport coordinates (0 to 1)
        Vector3 viewportPosition = Camera.main.ScreenToViewportPoint(cursorPosition);

        // Check if the cursor's X-coordinate is near the left or right edge (east or west poles)
        return Mathf.Abs(viewportPosition.x - 0.5f) > (0.5f - poleThreshold) && Mathf.Abs(viewportPosition.y) < poleThreshold;
    }

    // Add this method to indicate that time speed has been manually set
    public void SetTimeSpeedManually()
    {
        timeSpeedManuallySet = true;
    }
}
