using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugShortcuts : MonoBehaviour
{
    #if UNITY_EDITOR
    private void Awake()
    {
        Debug.Log("DebugShortcuts initialized - Press 'o' for Game Over, 'v' for Victory, 'c' for Chance Card");
        DontDestroyOnLoad(gameObject); // Keep this object alive between scenes
    }

    private void Update()
    {
        // Log any key press for debugging
        if (Input.anyKeyDown)
        {
            Debug.Log($"Key pressed: {Input.inputString}");
        }

        // 'o' for instant game over (lowercase)
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("'o' key detected - Triggering Game Over");
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.GameOver(); // Use PlayerState's GameOver instead of direct scene load
            }
        }

        // 'v' for instant victory (lowercase)
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("'v' key detected - Triggering Victory");
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.GameVictory(); // Use PlayerState's GameVictory instead of direct scene load
            }
        }

        // 'c' for chance card (lowercase)
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("'c' key detected - Triggering Chance Card");
            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.SetChanceCardKey("Money"); // Set a default key for testing
            }
            SceneManager.LoadScene("ChanceCard");
        }

        // Add new debug shortcuts for deducting stats
        if (PlayerState.Instance != null)
        {
            // 1 = Money
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Money");
                PlayerState.Instance.SetPlayerValue("Money", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Money. New value: {currentValue - 15}");
            }
            // 2 = Career
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Career");
                PlayerState.Instance.SetPlayerValue("Career", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Career. New value: {currentValue - 15}");
            }
            // 3 = Energy
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Energy");
                PlayerState.Instance.SetPlayerValue("Energy", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Energy. New value: {currentValue - 15}");
            }
            // 4 = Health
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Health");
                PlayerState.Instance.SetPlayerValue("Health", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Health. New value: {currentValue - 15}");
            }
            // 5 = Creativity
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Creativity");
                PlayerState.Instance.SetPlayerValue("Creativity", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Creativity. New value: {currentValue - 15}");
            }
            // 6 = Time
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                float currentValue = PlayerState.Instance.GetPlayerValue("Time");
                PlayerState.Instance.SetPlayerValue("Time", currentValue - 15, true);
                Debug.Log($"Deducted 15 from Time. New value: {currentValue - 15}");
            }
        }
    }

    private void OnEnable()
    {
        Debug.Log("DebugShortcuts enabled");
    }
    #endif
} 