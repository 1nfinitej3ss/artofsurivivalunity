using UnityEngine;
using System.Collections.Generic;

public class MainSceneController : MonoBehaviour
{
    #region Private Fields
    private const int c_DefaultStartValue = 100;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Ensure we initialize stats before PlayerState starts using them
        InitializePlayerStats();
    }
    #endregion

    #region Private Methods
    private void InitializePlayerStats()
    {
        if (PlayerState.Instance == null)
        {
            Debug.LogError("PlayerState.Instance is null!");
            return;
        }

        // Load the values directly from PlayerPrefs into PlayerState
        PlayerState.Instance.SetPlayerValue("Money", PlayerPrefs.GetInt("PlayerMoney"), false);
        PlayerState.Instance.SetPlayerValue("Career", PlayerPrefs.GetInt("PlayerCareer"), false);
        PlayerState.Instance.SetPlayerValue("Energy", PlayerPrefs.GetInt("PlayerEnergy"), false);
        PlayerState.Instance.SetPlayerValue("Creativity", PlayerPrefs.GetInt("PlayerCreativity"), false);
        PlayerState.Instance.SetPlayerValue("Time", PlayerPrefs.GetInt("PlayerTime"), false);

        Debug.Log($"Loaded stats - Money: {PlayerPrefs.GetInt("PlayerMoney")}, " +
                  $"Career: {PlayerPrefs.GetInt("PlayerCareer")}, " +
                  $"Energy: {PlayerPrefs.GetInt("PlayerEnergy")}, " +
                  $"Creativity: {PlayerPrefs.GetInt("PlayerCreativity")}, " +
                  $"Time: {PlayerPrefs.GetInt("PlayerTime")}");
    }

    private void InitializePlayerState()
    {
        if (PlayerState.Instance == null)
        {
            Debug.LogError("PlayerState.Instance is null!");
            return;
        }

        // Initialize PlayerState with the final values after effects have been applied
        PlayerState.Instance.SetPlayerValue("Money", PlayerPrefs.GetInt("PlayerMoney"), false);
        PlayerState.Instance.SetPlayerValue("Career", PlayerPrefs.GetInt("PlayerCareer"), false);
        PlayerState.Instance.SetPlayerValue("Energy", PlayerPrefs.GetInt("PlayerEnergy"), false);
        PlayerState.Instance.SetPlayerValue("Creativity", PlayerPrefs.GetInt("PlayerCreativity"), false);
        PlayerState.Instance.SetPlayerValue("Time", PlayerPrefs.GetInt("PlayerTime"), false);

        Debug.Log("PlayerState initialized with final values including starting effects");
    }

    private void ApplyStartingEffects()
    {
        Debug.Log("=== Applying Starting Effects ===");
        string[] statTypes = { "money", "career", "energy", "creativity", "time" };
        bool anyEffectsApplied = false;
        
        foreach (string statType in statTypes)
        {
            string effectKey = $"StartingEffect_{statType}";
            if (PlayerPrefs.HasKey(effectKey))
            {
                anyEffectsApplied = true;
                int effect = PlayerPrefs.GetInt(effectKey);
                string prefsKey = $"Player{char.ToUpper(statType[0]) + statType.Substring(1)}";
                
                int currentValue = PlayerPrefs.GetInt(prefsKey, c_DefaultStartValue);
                int newValue = currentValue + effect;
                
                PlayerPrefs.SetInt(prefsKey, newValue);
                Debug.Log($"Applied effect to {statType}: {currentValue} + {effect} = {newValue}");
                
                PlayerPrefs.DeleteKey(effectKey);
            }
        }

        if (!anyEffectsApplied)
        {
            Debug.Log("No starting effects found to apply");
        }
        
        PlayerPrefs.Save();
    }

    private void LogInitialStats()
    {
        // Log both PlayerPrefs and PlayerState values to verify they match
        Debug.Log("PlayerPrefs Values:");
        Debug.Log($"Initial Stats - Money: {PlayerPrefs.GetInt("PlayerMoney")}, " +
                 $"Career: {PlayerPrefs.GetInt("PlayerCareer")}, " +
                 $"Energy: {PlayerPrefs.GetInt("PlayerEnergy")}, " +
                 $"Creativity: {PlayerPrefs.GetInt("PlayerCreativity")}, " +
                 $"Time: {PlayerPrefs.GetInt("PlayerTime")}");

        if (PlayerState.Instance != null)
        {
            Debug.Log("PlayerState Values:");
            Debug.Log($"PlayerState Stats - Money: {PlayerState.Instance.GetPlayerValue("Money")}, " +
                     $"Career: {PlayerState.Instance.GetPlayerValue("Career")}, " +
                     $"Energy: {PlayerState.Instance.GetPlayerValue("Energy")}, " +
                     $"Creativity: {PlayerState.Instance.GetPlayerValue("Creativity")}, " +
                     $"Time: {PlayerState.Instance.GetPlayerValue("Time")}");
        }

        string scenarioText = PlayerPrefs.GetString("ScenarioText", "");
        if (!string.IsNullOrEmpty(scenarioText))
        {
            Debug.Log($"Starting scenario: {scenarioText}");
        }
    }
    #endregion
} 