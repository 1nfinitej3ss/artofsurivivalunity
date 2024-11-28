using UnityEngine;

public class RotateSprite : MonoBehaviour
{
    public float defaultRotationRate = 100f; // Default rotation rate
    public float boostedRotationRate = 300f; // Faster rotation rate when the cursor is in the lower-right corner
    public float transitionSpeed = 5f; // Speed of transitioning between rotation rates
    public float lowerRightThreshold = 0.2f; // Threshold for how close to the lower-right corner the cursor should be

    private float currentRotationRate;

    void Start()
    {
        currentRotationRate = defaultRotationRate; // Initialize with the default rotation rate
    }

    void Update()
    {
        // Check if the cursor is in the lower-right corner
        if (IsCursorInLowerRight())
        {
            // Increase the rotation rate if the cursor is in the lower-right
            currentRotationRate = Mathf.Lerp(currentRotationRate, boostedRotationRate, Time.deltaTime * transitionSpeed);
        }
        else
        {
            // Return to the default rotation rate if not in the lower-right
            currentRotationRate = Mathf.Lerp(currentRotationRate, defaultRotationRate, Time.deltaTime * transitionSpeed);
        }

        // Rotate the sprite along the z-axis
        transform.Rotate(0, 0, currentRotationRate * Time.deltaTime);
    }

    // Method to check if the cursor is in the lower-right corner
    private bool IsCursorInLowerRight()
    {
        // Get the cursor position in screen coordinates
        Vector3 cursorPosition = Input.mousePosition;

        // Convert the cursor position to viewport coordinates (0 to 1)
        Vector3 viewportPosition = Camera.main.ScreenToViewportPoint(cursorPosition);

        // Check if the cursor is within the lower-right threshold
        return viewportPosition.x > (1 - lowerRightThreshold) && viewportPosition.y < lowerRightThreshold;
    }
}
