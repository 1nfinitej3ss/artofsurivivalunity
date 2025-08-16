using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ScenarioEffect
{
    public enum AttributeType
    {
        Money,
        Career,
        Energy,
        Health,
        Creativity,
        Time
    }

    public AttributeType affectedAttribute;
    public int effectValue;
    public string effectDescription;
}

public class ChanceCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private GameObject[] scenarios;
    [SerializeField] private string nextScene;
    [SerializeField] private GameObject cardImage;
    [SerializeField] private GameObject arrowImage;
    [SerializeField] private GameObject chanceCardText;
    [SerializeField] private GameObject additionalText;

    [Header("Animation Settings")]
    [SerializeField] private float revealDelay = 0.5f;
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideDistance = 500f; // Distance to slide from in pixels

    [Header("Scenario Effects")]
    [SerializeField] private ScenarioEffect[] scenarioEffects; // Array matching scenarios array

    [Header("Background References")]
    [SerializeField] private GameObject background1;
    [SerializeField] private GameObject background2;

    [Header("UI Elements")]
    [SerializeField] private Button m_ContinueButton; // Reference to the continue button

    private CardWiggle cardWiggle;
    private Button cardButton;
    private bool hasBeenClicked = false;
    private List<int> remainingScenarios;
    private ScenarioEffect.AttributeType m_TriggeringAttribute;

    private void Awake()
    {
        if (PlayerState.Instance == null)
        {
            Debug.LogWarning("PlayerState not found. Using default values for testing.");
            GameObject playerState = new GameObject("PlayerState");
            playerState.AddComponent<PlayerState>();
        }
        
        // Get the triggering attribute from PlayerState
        if (PlayerState.Instance.chanceCardKey != null)
        {
            // Parse the string key to our enum
            if (System.Enum.TryParse<ScenarioEffect.AttributeType>(PlayerState.Instance.chanceCardKey, out ScenarioEffect.AttributeType attribute))
            {
                m_TriggeringAttribute = attribute;
            }
            else
            {
                Debug.LogError($"Failed to parse attribute type from key: {PlayerState.Instance.chanceCardKey}");
            }
        }
        else
        {
            Debug.LogError("No chance card key set in PlayerState!");
        }
    }

    private void Start()
    {
        InitializeScene();
        // Get reference to the Button component
        cardButton = cardImage.GetComponent<Button>();

        // Ensure initial background state
        if (background1 != null && background2 != null)
        {
            background1.SetActive(true);
            background2.SetActive(false);
        }

        // Debug logging for all attribute values
        if (PlayerState.Instance != null)
        {
            Debug.Log("=== Current Attribute Values ===");
            foreach (ScenarioEffect.AttributeType attribute in System.Enum.GetValues(typeof(ScenarioEffect.AttributeType)))
            {
                float value = PlayerState.Instance.GetPlayerValue(attribute.ToString());
                Debug.Log($"{attribute}: {value}");
            }
            Debug.Log($"Triggering Attribute: {PlayerState.Instance.chanceCardKey}");
            Debug.Log("============================");
        }
        else
        {
            Debug.LogWarning("PlayerState.Instance is null - cannot debug attribute values");
        }
    }

    private void InitializeScene()
    {
        if (arrowImage != null)
        {
            arrowImage.SetActive(false);
        }

        remainingScenarios = new List<int>();
        for (int i = 0; i < scenarios.Length; i++)
        {
            remainingScenarios.Add(i);
        }

        foreach (GameObject scenario in scenarios)
        {
            if (scenario != null)
            {
                scenario.SetActive(false);
            }
        }

        if (cardImage != null)
        {
            cardImage.SetActive(true);
            cardWiggle = cardImage.GetComponent<CardWiggle>();
            if (cardWiggle == null)
            {
                Debug.LogError("CardWiggle component missing from cardImage!");
            }
        }
        else
        {
            Debug.LogError("Card Image not assigned in ChanceCard script!");
        }

        // Hide the continue button initially
        if (m_ContinueButton != null)
        {
            m_ContinueButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Continue button reference not set in ChanceCard script!");
        }

        UpdateTitleText();
    }

    public void OnCardClick()
    {
        if (!hasBeenClicked && cardWiggle != null)
        {
            hasBeenClicked = true;
            
            // Hide both text objects
            if (chanceCardText != null)
            {
                chanceCardText.SetActive(false);
            }
            if (additionalText != null)
            {
                additionalText.SetActive(false);
            }
            
            // Completely deactivate the Button component
            if (cardButton != null)
            {
                cardButton.enabled = false;
            }
            
            Debug.Log("Card clicked - starting animation sequence");
            cardWiggle.OnClick();
            StartCoroutine(RevealAfterAnimation());
        }
    }

    private IEnumerator RevealAfterAnimation()
    {
        yield return new WaitForSeconds(revealDelay);
        
        StartScenario();
        SwitchBackgrounds();
        
        if (arrowImage != null)
        {
            arrowImage.SetActive(true);
        }

        StartCoroutine(SlideInScenarioText());
    }

    private IEnumerator SlideInScenarioText()
    {
        TextMeshProUGUI scenarioText = null;
        RectTransform textRect = null;

        foreach (GameObject scenario in scenarios)
        {
            if (scenario.activeSelf)
            {
                scenarioText = scenario.GetComponentInChildren<TextMeshProUGUI>();
                textRect = scenarioText.GetComponent<RectTransform>();
                break;
            }
        }

        if (scenarioText != null && textRect != null)
        {
            // Store original position
            Vector2 endPosition = textRect.anchoredPosition;
            // Set start position (left of the screen)
            Vector2 startPosition = endPosition + new Vector2(-slideDistance, 0);
            textRect.anchoredPosition = startPosition;

            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                float progress = elapsed / slideInDuration;
                // Use smoothstep for easing
                float smoothProgress = progress * progress * (3f - 2f * progress);
                textRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, smoothProgress);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure we end up exactly at the target position
            textRect.anchoredPosition = endPosition;

            // Show the continue button after the scenario text has finished sliding in
            if (m_ContinueButton != null)
            {
                m_ContinueButton.gameObject.SetActive(true);
            }
        }
    }

    private void StartScenario()
    {
        int scenarioIndex = ChoseRandomScenario();
        Debug.Log($"Starting scenario {scenarioIndex}");
        
        if (scenarioIndex < scenarios.Length && scenarios[scenarioIndex] != null)
        {
            GameObject scenario = scenarios[scenarioIndex];
            scenario.SetActive(true);
            
            // Apply the effect and update the text
            if (scenarioIndex < scenarioEffects.Length)
            {
                ScenarioEffect effect = scenarioEffects[scenarioIndex];
                ApplyScenarioEffect(effect);
                
                // Get the TextMeshProUGUI component and update its text
                TextMeshProUGUI scenarioText = scenario.GetComponentInChildren<TextMeshProUGUI>();
                if (scenarioText != null)
                {
                    // Enable text wrapping and auto-sizing
                    scenarioText.enableWordWrapping = true;
                    scenarioText.overflowMode = TextOverflowModes.Overflow;
                    scenarioText.enableAutoSizing = false;
                    
                    // Set the text
                    scenarioText.text = FormatScenarioText(effect);
                    
                    // Force text to update
                    scenarioText.ForceMeshUpdate();
                }
            }
        }
        else
        {
            Debug.LogError($"Invalid scenario index or null scenario: {scenarioIndex}");
        }
    }

    private int ChoseRandomScenario()
    {
        if (remainingScenarios.Count == 0)
        {
            // All scenarios have been used, reset the list
            for (int i = 0; i < scenarios.Length; i++)
            {
                remainingScenarios.Add(i);
            }
        }

        // Pick a random index from remaining scenarios
        int randomIndex = Random.Range(0, remainingScenarios.Count);
        int selectedScenario = remainingScenarios[randomIndex];
        
        // Remove the selected scenario from the remaining list
        remainingScenarios.RemoveAt(randomIndex);
        
        return selectedScenario;
    }

    private void ApplyScenarioEffect(ScenarioEffect effect)
    {
        if (PlayerState.Instance == null) return;

        string statKey = effect.affectedAttribute.ToString();
        float currentValue = PlayerState.Instance.GetPlayerValue(statKey);
        float newValue = currentValue + effect.effectValue;
        
        // Apply the effect
        PlayerState.Instance.SetPlayerValue(statKey, newValue, true);
        
        // Check if any attribute has dropped to 0 or below
        string[] statsToCheck = { "Money", "Career", "Energy", "Creativity" };
        foreach (var stat in statsToCheck)
        {
            float value = PlayerState.Instance.GetPlayerValue(stat);
            if (value <= 0)
            {
                Debug.LogWarning($"{stat} has hit 0 in ChanceCard - Setting game over pending");
                PlayerState.Instance.SetGameOverPending(true);
                break;
            }
        }
    }

    private string FormatScenarioText(ScenarioEffect effect)
    {
        if (effect == null) return "";
        
        // Split the description if it contains parentheses
        string description = effect.effectDescription;
        if (description.Contains("("))
        {
            description = description.Substring(0, description.IndexOf("(")).Trim();
        }
        
        string sign = effect.effectValue >= 0 ? "+" : "";
        return $"{description}\n\n<align=center><color=#FF9999>{sign}{effect.effectValue} {effect.affectedAttribute}</color></align>";
    }

    public void SetTriggeringAttribute(ScenarioEffect.AttributeType _attribute)
    {
        m_TriggeringAttribute = _attribute;
    }

    private void UpdateTitleText()
    {
        if (titleTxt == null)
        {
            Debug.LogError("Title Text component not assigned in ChanceCard script!");
            return;
        }

        // Get the attribute name from PlayerState's chanceCardKey
        string attributeName = PlayerState.Instance?.chanceCardKey;
        
        if (string.IsNullOrEmpty(attributeName))
        {
            Debug.LogError("No chanceCardKey found in PlayerState!");
            titleTxt.text = "ERROR: Could not determine triggering attribute!";
            return;
        }

        attributeName = attributeName.ToUpper();
        titleTxt.text = $"YOUR {attributeName} SCORE IS TOO LOW!";
        Debug.Log($"Set title text to: {titleTxt.text} based on chanceCardKey: {PlayerState.Instance.chanceCardKey}");
    }

    public void CloseChanceCard()
    {
        // Check if game over is pending
        if (PlayerState.Instance != null && PlayerState.Instance.IsGameOverPending())
        {
            // If game over is pending, go to main scene first
            SceneManager.LoadScene("main");
        }
        else
        {
            // Otherwise, load the next scene that was set
            SceneManager.LoadScene(nextScene);
        }
    }

    public void SimulateCardClick()
    {
        OnCardClick();
    }

    public void SwitchBackgrounds()
    {
        if (background1 == null || background2 == null)
        {
            Debug.LogWarning("Background references not set in ChanceCard script! Background1: " + 
                           (background1 != null ? "Set" : "Null") + ", Background2: " + 
                           (background2 != null ? "Set" : "Null"));
            return;
        }

        // Log the current states before switching
        Debug.Log($"Before switch - Background1 active: {background1.activeSelf}, Background2 active: {background2.activeSelf}");

        // Make sure at least one background is active initially
        if (!background1.activeSelf && !background2.activeSelf)
        {
            background1.SetActive(true);
        }

        // Switch the backgrounds
        bool wasBackground1Active = background1.activeSelf;
        background1.SetActive(!wasBackground1Active);
        background2.SetActive(wasBackground1Active);

        // Log the new states
        Debug.Log($"After switch - Background1 active: {background1.activeSelf}, Background2 active: {background2.activeSelf}");
    }
}