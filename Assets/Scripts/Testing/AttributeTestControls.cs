using UnityEngine;

public class AttributeTestControls : MonoBehaviour
{
    private const float DEDUCTION_AMOUNT = 15f;
    private PlayerState playerState;

    private void Start()
    {
        playerState = PlayerState.Instance;
        if (playerState == null)
        {
            Debug.LogError("PlayerState not found! Attribute testing controls won't work.");
        }
    }

    private void Update()
    {
        if (playerState == null) return;

        // Check for number key presses and deduct from corresponding attributes
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DeductFromAttribute("Money");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DeductFromAttribute("Career");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            DeductFromAttribute("Energy");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            DeductFromAttribute("Health");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            DeductFromAttribute("Creativity");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            DeductFromAttribute("Time");
        }
    }

    private void DeductFromAttribute(string attributeName)
    {
        float currentValue = playerState.GetPlayerValue(attributeName);
        playerState.SetPlayerValue(attributeName, currentValue - DEDUCTION_AMOUNT, true);
    }
} 