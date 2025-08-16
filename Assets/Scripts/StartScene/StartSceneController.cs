using PlanetRunner;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class StartSceneController : MonoBehaviour
{
    #region Private Fields
    [System.Serializable]
    private class IndependentEffect
    {
        public string type;
        public int value;
    }

    [SerializeField] private TextMeshProUGUI m_ScenarioText;
    [SerializeField] private TextMeshProUGUI m_EffectsText;
    [SerializeField] private GameObject m_DependentPanel;
    [SerializeField] private GameObject m_IndependentPanel;

    [SerializeField] private Toggle m_StudentLoanToggle;
    [SerializeField] private Toggle m_MortgageToggle;
    [SerializeField] private Toggle m_MedicalBillsToggle;
    [SerializeField] private Toggle m_RentToggle;
    [SerializeField] private Toggle m_HealthInsuranceToggle;
    [SerializeField] private Toggle m_FamilySupportToggle;
    [SerializeField] private Toggle m_NetflixToggle;
    [SerializeField] private Toggle m_NoneToggle;

    [System.Serializable]
    private class ScenarioEffect
    {
        public string type;
        public int value;
    }

    [System.Serializable]
    private class ScenarioData
    {
        public string scenarioText;
        public string effectsText;
        public List<ScenarioEffect> effects = new();
    }

    private readonly List<ScenarioData> m_Scenarios = new()
    {
        new ScenarioData 
        { 
            scenarioText = "Your partner/family is pleased to support you.",
            effectsText = "+10 money\n+10 career\n+10 energy",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Money", value = 10 },
                new ScenarioEffect { type = "Career", value = 10 },
                new ScenarioEffect { type = "Energy", value = 10 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "The pressure to succeed causes a creative block that affects your work.",
            effectsText = "-10 creativity\n-10 career",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Creativity", value = -10 },
                new ScenarioEffect { type = "Career", value = -10 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "Dependence on financial support causes tension in your relationships.",
            effectsText = "-10 time\n-15 energy",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Time", value = -10 },
                new ScenarioEffect { type = "Energy", value = -15 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "A wealthy patron takes an interest in your work and offers to sponsor your next project.",
            effectsText = "+20 money\n+10 career",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Money", value = 20 },
                new ScenarioEffect { type = "Career", value = 10 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "Your partner/family faces a financial crisis, and you need to support them.",
            effectsText = "-30 money\n-10 energy",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Money", value = -30 },
                new ScenarioEffect { type = "Energy", value = -10 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "You or a close family member become ill, leading to increased expenses and stress.",
            effectsText = "-20 money\n-20 energy",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Money", value = -20 },
                new ScenarioEffect { type = "Energy", value = -20 }
            }
        },
        new ScenarioData 
        { 
            scenarioText = "A distant relative leaves you an inheritance, easing your financial burden.",
            effectsText = "+40 money\n+5 energy",
            effects = new List<ScenarioEffect>
            {
                new ScenarioEffect { type = "Money", value = 40 },
                new ScenarioEffect { type = "Energy", value = 5 }
            }
        }
    };

    // Add dictionary to map effect names to PlayerPrefs keys
    private readonly Dictionary<string, string> m_EffectToPrefsMap = new()
    {
        { "Money", "PlayerMoney" },
        { "Career", "PlayerCareer" },
        { "Energy", "PlayerEnergy" },
        { "Creativity", "PlayerCreativity" },
        { "Time", "PlayerTime" }
    };

    private ScenarioData m_SelectedScenario;
    private const int c_DefaultStartValue = 100;

    private readonly Dictionary<string, List<IndependentEffect>> m_IndependentEffects = new()
    {
        { "StudentLoan", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -15 } } },
        { "Mortgage", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -15 } } },
        { "MedicalBills", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -15 } } },
        { "Rent", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -10 } } },
        { "HealthInsurance", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -10 } } },
        { "Netflix", new List<IndependentEffect> { new IndependentEffect { type = "Money", value = -5 } } },
        { "FamilySupport", new List<IndependentEffect> 
            { 
                new IndependentEffect { type = "Money", value = -15 },
                new IndependentEffect { type = "Energy", value = -15 }
            } 
        },
        { "None", new List<IndependentEffect>() }
    };

    private bool m_IsIndependentPath = false;

    // Track remaining scenarios to ensure all are shown before repeating
    private List<int> m_RemainingScenarios = new();
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        InitializeDefaultStats();
        SetupToggles();
        LoadRemainingScenarios();
        SelectRandomScenario();
    }

    private void SetupToggles()
    {
        // Remove ToggleGroup check since we're not using it anymore
        // Ensure all toggles are in the toggle group and properly configured
        Toggle[] allToggles = { m_StudentLoanToggle, m_MortgageToggle, m_MedicalBillsToggle, 
                               m_RentToggle, m_HealthInsuranceToggle, m_FamilySupportToggle, 
                               m_NetflixToggle, m_NoneToggle };
        
        foreach (var toggle in allToggles)
        {
            if (toggle != null)
            {
                // Remove toggle from group to allow multiple selection
                toggle.group = null;
                toggle.isOn = false; // Reset all toggles first
                
                // Remove any existing listeners to avoid duplicates
                toggle.onValueChanged.RemoveAllListeners();

                // Add listener with toggle name for debugging
                string toggleName = toggle.name; // Cache the name to ensure correct closure capture
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    Debug.Log($"[togglecheck] Toggle '{toggleName}' state changed - isOn: {isOn}");
                    
                    // Special handling for None toggle
                    if (toggle == m_NoneToggle)
                    {
                        if (isOn)
                        {
                            Debug.Log($"[togglecheck] None toggle selected, turning off all other toggles");
                            // Turn off all other toggles
                            foreach (var otherToggle in allToggles)
                            {
                                if (otherToggle != m_NoneToggle)
                                {
                                    otherToggle.isOn = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        // If any other toggle is turned on, turn off None
                        if (isOn)
                        {
                            Debug.Log($"[togglecheck] Regular toggle '{toggleName}' selected, turning off None toggle");
                            m_NoneToggle.isOn = false;
                        }
                    }
                });
            }
            else
            {
                Debug.LogError("[togglecheck] One of the toggles is not assigned!");
            }
        }
    }

    public void OnPlayBtnClicked()
    {
        Debug.Log("Game Start!");
        StartCoroutine(StartGameSequence());
    }

    public void ShowDependentPath()
    {
        m_IsIndependentPath = false;
        m_DependentPanel.SetActive(true);
        m_IndependentPanel.SetActive(false);
        Debug.Log($"Path set to Dependent. IsIndependentPath = {m_IsIndependentPath}");
        Debug.Log($"Panels - Dependent: {m_DependentPanel.activeSelf}, Independent: {m_IndependentPanel.activeSelf}");
    }

    public void ShowIndependentPath()
    {
        m_IsIndependentPath = true;
        m_DependentPanel.SetActive(false);
        m_IndependentPanel.SetActive(true);
        Debug.Log($"Path set to Independent. IsIndependentPath = {m_IsIndependentPath}");
        Debug.Log($"Panels - Dependent: {m_DependentPanel.activeSelf}, Independent: {m_IndependentPanel.activeSelf}");
    }
    #endregion

    #region Private Methods
    private void LoadRemainingScenarios()
    {
        m_RemainingScenarios.Clear();
        
        // Try to load remaining scenarios from PlayerPrefs
        string remainingScenariosStr = PlayerPrefs.GetString("RemainingScenarios", "");
        
        if (string.IsNullOrEmpty(remainingScenariosStr))
        {
            // If no saved state, initialize with all scenarios
            for (int i = 0; i < m_Scenarios.Count; i++)
            {
                m_RemainingScenarios.Add(i);
            }
            SaveRemainingScenarios();
        }
        else
        {
            // Load saved state
            string[] indices = remainingScenariosStr.Split(',');
            foreach (string index in indices)
            {
                if (int.TryParse(index, out int scenarioIndex))
                {
                    m_RemainingScenarios.Add(scenarioIndex);
                }
            }
        }
        
        Debug.Log($"Loaded remaining scenarios: {string.Join(", ", m_RemainingScenarios)}");
    }

    private void SaveRemainingScenarios()
    {
        string remainingScenariosStr = string.Join(",", m_RemainingScenarios);
        PlayerPrefs.SetString("RemainingScenarios", remainingScenariosStr);
        PlayerPrefs.Save();
        Debug.Log($"Saved remaining scenarios: {remainingScenariosStr}");
    }

    private void SelectRandomScenario()
    {
        if (!m_ScenarioText || !m_EffectsText)
        {
            Debug.LogError("Text components not assigned in StartSceneController!");
            return;
        }

        // If we've used all scenarios, reset the list
        if (m_RemainingScenarios.Count == 0)
        {
            Debug.Log("All scenarios have been shown, resetting the list");
            for (int i = 0; i < m_Scenarios.Count; i++)
            {
                m_RemainingScenarios.Add(i);
            }
            SaveRemainingScenarios();
        }

        // Log remaining scenarios for debugging
        Debug.Log($"Remaining scenarios: {string.Join(", ", m_RemainingScenarios)}");

        // Pick a random index from remaining scenarios
        int randomIndex = UnityEngine.Random.Range(0, m_RemainingScenarios.Count);
        int selectedScenarioIndex = m_RemainingScenarios[randomIndex];
        
        // Remove the selected scenario from remaining list
        m_RemainingScenarios.RemoveAt(randomIndex);
        
        // Set the selected scenario
        m_SelectedScenario = m_Scenarios[selectedScenarioIndex];

        m_ScenarioText.text = m_SelectedScenario.scenarioText;
        m_EffectsText.text = m_SelectedScenario.effectsText;
        
        // Save the updated remaining scenarios
        SaveRemainingScenarios();
        
        Debug.Log($"Selected scenario {selectedScenarioIndex}: '{m_SelectedScenario.scenarioText}' ({m_RemainingScenarios.Count} scenarios remaining)");
    }

    private void StoreScenarioEffects()
    {
        if (m_SelectedScenario == null)
        {
            Debug.LogError("No scenario selected!");
            return;
        }

        Debug.Log($"Storing effects for scenario: {m_SelectedScenario.scenarioText}");
        
        // Clear any existing effect keys
        foreach (var kvp in m_EffectToPrefsMap)
        {
            PlayerPrefs.DeleteKey($"StartingEffect_{kvp.Key}");
        }

        // Store the new effects
        foreach (var effect in m_SelectedScenario.effects)
        {
            string effectKey = $"StartingEffect_{effect.type}";
            PlayerPrefs.SetInt(effectKey, effect.value);
            Debug.Log($"Stored effect: {effectKey} = {effect.value}");
        }

        PlayerPrefs.Save();
    }

    private IEnumerator StartGameSequence()
    {
        Debug.Log("Starting game sequence...");
        Debug.Log($"Current path: {(m_IsIndependentPath ? "Independent" : "Dependent")}");
        
        // First set all stats to default value
        foreach (var kvp in m_EffectToPrefsMap)
        {
            PlayerPrefs.SetInt(kvp.Value, c_DefaultStartValue);
            Debug.Log($"Set {kvp.Key} to default value: {c_DefaultStartValue}");
        }

        if (m_IsIndependentPath)
        {
            Debug.Log("Applying independent effects...");
            ApplyIndependentEffects();
        }
        else
        {
            Debug.Log("Applying dependent scenario effects...");
            // Apply scenario effects directly
            foreach (var effect in m_SelectedScenario.effects)
            {
                string prefsKey = m_EffectToPrefsMap[effect.type];
                int currentValue = PlayerPrefs.GetInt(prefsKey);
                PlayerPrefs.SetInt(prefsKey, currentValue + effect.value);
                Debug.Log($"Applied scenario effect: {effect.type} = {currentValue + effect.value}");
            }
        }

        // Store the scenario text for reference
        PlayerPrefs.SetString("ScenarioText", m_SelectedScenario.scenarioText);
        PlayerPrefs.Save();

        // Clean up persistent objects that aren't essential
        var persistentObjects = GameObject.FindObjectsOfType<GameObject>()
            .Where(go => go.scene.name == "DontDestroyOnLoad");

        foreach (var obj in persistentObjects)
        {
            if (obj.GetComponent<QuestionContentManager>() != null || 
                obj.GetComponent<AudioManager>() != null)
            {
                continue;
            }
            Destroy(obj);
        }

        // Reset all location visited states
        PlayerPrefs.SetInt("HomeVisited", 0);
        PlayerPrefs.SetInt("StudioVisited", 0);
        PlayerPrefs.SetInt("GalleryVisited", 0);
        PlayerPrefs.SetInt("SocialVisited", 0);
        PlayerPrefs.SetInt("WorkVisited", 0);
        
        PlayerPrefs.Save();

        // Load main scene
        SceneManager.LoadScene("main");
        yield return null;
    }

    private void InitializeDefaultStats()
    {
        foreach (var kvp in m_EffectToPrefsMap)
        {
            if (!PlayerPrefs.HasKey(kvp.Value))
            {
                PlayerPrefs.SetInt(kvp.Value, c_DefaultStartValue);
            }
        }
        PlayerPrefs.Save();
    }

    private void ApplyIndependentEffects()
    {
        Debug.Log("[netflixcheck] Starting ApplyIndependentEffects");
        Debug.Log($"[netflixcheck] Netflix toggle state: {m_NetflixToggle.isOn}");
        
        // Log initial values for all stats
        foreach (var kvp in m_EffectToPrefsMap)
        {
            int initialValue = PlayerPrefs.GetInt(kvp.Value);
            Debug.Log($"[netflixcheck] Initial {kvp.Key} value: {initialValue}");
        }
        
        Dictionary<string, Toggle> toggleMap = new()
        {
            { "StudentLoan", m_StudentLoanToggle },
            { "Mortgage", m_MortgageToggle },
            { "MedicalBills", m_MedicalBillsToggle },
            { "Rent", m_RentToggle },
            { "HealthInsurance", m_HealthInsuranceToggle },
            { "FamilySupport", m_FamilySupportToggle },
            { "Netflix", m_NetflixToggle },
            { "None", m_NoneToggle }
        };

        foreach (var kvp in toggleMap)
        {
            Debug.Log($"[netflixcheck] Checking toggle {kvp.Key} - isOn: {kvp.Value.isOn}");
            if (kvp.Value.isOn)
            {
                Debug.Log($"[netflixcheck] Processing {kvp.Key} toggle - isOn: {kvp.Value.isOn}");
                Debug.Log($"[netflixcheck] Effects defined for {kvp.Key}: {string.Join(", ", m_IndependentEffects[kvp.Key].Select(e => $"{e.type}: {e.value}"))}");
                
                foreach (var effect in m_IndependentEffects[kvp.Key])
                {
                    string prefsKey = m_EffectToPrefsMap[effect.type];
                    int currentValue = PlayerPrefs.GetInt(prefsKey);
                    int newValue = currentValue + effect.value;
                    PlayerPrefs.SetInt(prefsKey, newValue);
                    Debug.Log($"[netflixcheck] Applied {kvp.Key} effect: {effect.type} changed from {currentValue} to {newValue}");
                }
            }
        }

        // Log final values for all stats
        foreach (var kvp in m_EffectToPrefsMap)
        {
            int finalValue = PlayerPrefs.GetInt(kvp.Value);
            Debug.Log($"[netflixcheck] Final {kvp.Key} value: {finalValue}");
        }
        
        PlayerPrefs.Save();
    }
    #endregion
}