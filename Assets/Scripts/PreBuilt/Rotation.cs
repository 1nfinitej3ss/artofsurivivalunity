using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float defaultRotationRate = 10f; // Default rotation rate
    public float boostedRotationRate = 30f; // Faster rotation rate when the cursor is near poles
    public float transitionSpeed = 5f; // Speed of transitioning between rotation rates
    public float poleThreshold = 0.1f; // Threshold for how close to the poles (east/west) the cursor should be

    public float minTimeSpeed = 0.5f; // Minimum time speed when rotation is slow
    public float maxTimeSpeed = 2.0f; // Maximum time speed when rotation is fast

    private float currentRotationRate;
    public DayNightCycle2D dayNightCycle; // Reference to the DayNightCycle2D script

    void Start()
    {
        currentRotationRate = defaultRotationRate; // Initialize with the default rotation rate
    }

    void Update()
    {
        // Check if the cursor is near the east or west pole
        if (IsCursorAtPole())
        {
            // If the cursor is at the poles, increase the rotation rate
            currentRotationRate = Mathf.Lerp(currentRotationRate, boostedRotationRate, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // If the cursor is not at the poles, return to the default rotation rate
            currentRotationRate = Mathf.Lerp(currentRotationRate, defaultRotationRate, Time.deltaTime * transitionSpeed);
        }

        // Rotate the globe based on the current rotation rate
        transform.Rotate(Vector3.forward, currentRotationRate * Time.deltaTime);

        // Adjust the time speed in the DayNightCycle2D script based on the current rotation rate
        if (dayNightCycle != null)
        {
            // Calculate a normalized speed factor based on the current rotation rate
            float normalizedSpeed = (currentRotationRate - defaultRotationRate) / (boostedRotationRate - defaultRotationRate);
            // Use the normalized speed to set the time speed within the min and max range
            dayNightCycle.timeSpeed = Mathf.Lerp(minTimeSpeed, maxTimeSpeed, Mathf.Clamp01(normalizedSpeed));
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
}
