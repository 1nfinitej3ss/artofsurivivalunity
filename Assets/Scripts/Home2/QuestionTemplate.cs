using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using PlanetRunner;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuestionTemplate : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private Image fadeOverlay;

    [Header("Question UI")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private VideoPlayer questionVideo;
    [SerializeField] private Button nextButton;

    [Header("Options UI")]
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private VideoPlayer optionsVideo;

    [Header("Results UI")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI effectsText;
    [SerializeField] private VideoPlayer resultsVideo;
    [SerializeField] private Button continueButton;

    public UnityEvent onQuestionComplete = new UnityEvent();
    private QuestionData currentQuestion;
    private CanvasGroup scoreCanvasGroup;
    private CanvasGroup optionsCanvasGroup;
    private CanvasGroup resultsCanvasGroup;

    private void Start()
    {
        // Set up button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowOptionsPanel);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        // Find the score panel's CanvasGroup
        var scorePanel = GameObject.FindObjectOfType<ScorePanelController>();
        if (scorePanel != null)
        {
            scoreCanvasGroup = scorePanel.GetComponent<CanvasGroup>();
        }

        // Ensure panels have CanvasGroups
        optionsCanvasGroup = EnsureCanvasGroup(optionsPanel);
        resultsCanvasGroup = EnsureCanvasGroup(resultsPanel);

        // Initialize fade overlay
        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = false;  // Don't block raycasts when transparent
            fadeOverlay.color = new Color(0, 0, 0, 0);  // Start fully transparent
        }
    }

    private CanvasGroup EnsureCanvasGroup(GameObject panel)
    {
        var canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    public void Initialize(QuestionData questionData)
    {
        Debug.Log($"Initializing question: {questionData?.questionId}");
        currentQuestion = questionData;
        
        // Set up question
        if (questionText != null)
        {
            questionText.text = questionData.text;
            Debug.Log($"Set question text to: {questionData.text}");
        }
        else
        {
            Debug.LogError("Question Text component is not assigned!");
        }
        
        SetupVideo(questionVideo, questionData.questionBackgroundKey);
        
        // Clear existing options
        if (optionsContainer != null)
        {
            Debug.Log($"Clearing {optionsContainer.childCount} existing options");
            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create option buttons
            Debug.Log($"Creating {questionData.options.Length} option buttons");
            foreach (var option in questionData.options)
            {
                CreateOptionButton(option);
            }
        }
        else
        {
            Debug.LogError("Options Container is not assigned!");
        }

        ShowQuestionPanel();
    }

    private void CreateOptionButton(OptionData optionData)
    {
        Debug.Log($"Creating button for option: {optionData.text}");
        
        if (optionButtonPrefab == null)
        {
            Debug.LogError("Option Button Prefab is not assigned!");
            return;
        }

        if (optionsContainer == null)
        {
            Debug.LogError("Options Container is not assigned!");
            return;
        }

        GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
        var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = optionData.text;
            Debug.Log($"Set button text to: {optionData.text}");
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in button prefab!");
        }

        var button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component not found!");
        }
        else
        {
            // Add the onClick listener
            button.onClick.AddListener(() => OnOptionSelected(optionData));
            Debug.Log($"Added onClick listener for option: {optionData.text}");
        }
    }

    private IEnumerator TransitionToResults()
    {
        // First hide score panel immediately
        if (scoreCanvasGroup != null)
        {
            scoreCanvasGroup.alpha = 0f;
            scoreCanvasGroup.interactable = false;
            scoreCanvasGroup.blocksRaycasts = false;
        }

        // Activate results panel but keep it hidden
        resultsPanel.SetActive(true);
        resultsCanvasGroup.alpha = 0f;

        // Enable raycast blocking before fade starts
        fadeOverlay.raycastTarget = true;

        // Fade to black
        float elapsedTime = 0f;
        while (elapsedTime < transitionDelay / 2)
        {
            float normalizedTime = elapsedTime / (transitionDelay / 2);
            optionsCanvasGroup.alpha = 1 - normalizedTime;
            fadeOverlay.color = new Color(0, 0, 0, normalizedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we're fully black
        optionsCanvasGroup.alpha = 0f;
        fadeOverlay.color = new Color(0, 0, 0, 1);
        optionsPanel.SetActive(false);

        // Small pause at black
        yield return new WaitForSeconds(0.1f);

        // Fade from black to results
        elapsedTime = 0f;
        while (elapsedTime < transitionDelay / 2)
        {
            float normalizedTime = elapsedTime / (transitionDelay / 2);
            resultsCanvasGroup.alpha = normalizedTime;
            fadeOverlay.color = new Color(0, 0, 0, 1 - normalizedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final states
        resultsCanvasGroup.alpha = 1f;
        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.raycastTarget = false;  // Disable raycast blocking when fully transparent
    }

    public void OnOptionSelected(OptionData option)
    {
        // Start the transition sequence
        StartCoroutine(TransitionToResults());
        
        // Apply immediate effects
        if (option.effects != null)
        {
            ApplyEffects(option.effects);
        }

        // Handle monthly effects if present
        if (option.isMonthlyEffect && option.monthlyEffects != null)
        {
            SetupMonthlyEffects(option.monthlyEffects);
        }

        // Handle random results if present
        if (option.randomResults != null && option.randomResults.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, option.randomResults.Length);
            var result = option.randomResults[randomIndex];
            ApplyEffects(result.effects);
            resultText.text = result.resultText;
            effectsText.text = FormatEffectsText(result.effects);
        }
        else
        {
            // Use resultText if available, otherwise use option text
            resultText.text = !string.IsNullOrEmpty(option.resultText) ? option.resultText : option.text;
            effectsText.text = FormatEffectsText(option.effects);
        }

        // Handle game ending
        if (option.isGameEnding)
        {
            Debug.Log("Game ending triggered");
            return;
        }

        SetupVideo(resultsVideo, option.optionsBackgroundKey);
    }

    private string FormatEffectsText(EffectData effects)
    {
        if (effects == null) return "";

        List<string> effectTexts = new List<string>();
        
        if (effects.money != 0)
            effectTexts.Add($"{(effects.money > 0 ? "+" : "")}{effects.money} money");
        if (effects.career != 0)
            effectTexts.Add($"{(effects.career > 0 ? "+" : "")}{effects.career} career");
        if (effects.energy != 0)
            effectTexts.Add($"{(effects.energy > 0 ? "+" : "")}{effects.energy} energy");
        if (effects.creativity != 0)
            effectTexts.Add($"{(effects.creativity > 0 ? "+" : "")}{effects.creativity} creativity");
        if (effects.time != 0)
            effectTexts.Add($"{(effects.time > 0 ? "+" : "")}{effects.time} time");

        return effectTexts.Count > 0 ? string.Join("\n", effectTexts) : "";
    }

    private void SetupMonthlyEffects(EffectData monthlyEffects)
    {
        string effectsKey = $"MonthlyEffects_{currentQuestion.questionId}";
        
        Debug.Log($"Setting up monthly effects for {currentQuestion.questionId}");
        Debug.Log($"Monthly effects - Money: {monthlyEffects.money}, Career: {monthlyEffects.career}, " +
                  $"Energy: {monthlyEffects.energy}, Creativity: {monthlyEffects.creativity}");
        
        // If this effect includes a monthly money charge, update the monthly charges
        if (monthlyEffects.money < 0)
        {
            // Split the negative money effect evenly between groceries and utilities
            int chargePerCategory = (int)Mathf.Abs(monthlyEffects.money) / 2;
            
            int currentGroceries = PlayerState.Instance.GetMonthlyCharges("Groceries");
            int currentUtilities = PlayerState.Instance.GetMonthlyCharges("Utilities");
            
            PlayerState.Instance.SetMonthlyCharges(
                PlayerState.Instance.GetMonthlyCharges("Rent"),
                currentUtilities + chargePerCategory,  // Add half to utilities
                currentGroceries + chargePerCategory   // Add half to groceries
            );
            
            Debug.Log($"Updated monthly charges - Groceries: {currentGroceries + chargePerCategory}, " +
                      $"Utilities: {currentUtilities + chargePerCategory}");
        }
        
        // Store the non-money effects as usual (excluding time)
        PlayerPrefs.SetFloat($"{effectsKey}_career", monthlyEffects.career);
        PlayerPrefs.SetFloat($"{effectsKey}_energy", monthlyEffects.energy);
        PlayerPrefs.SetFloat($"{effectsKey}_creativity", monthlyEffects.creativity);
        
        PlayerPrefs.SetInt($"{effectsKey}_active", 1);
        
        // Add to active effects tracking
        string activeEffects = PlayerPrefs.GetString("ActiveMonthlyEffects", "");
        if (!activeEffects.Contains(effectsKey))
        {
            activeEffects = string.IsNullOrEmpty(activeEffects) ? effectsKey : $"{activeEffects},{effectsKey}";
            PlayerPrefs.SetString("ActiveMonthlyEffects", activeEffects);
        }
        
        PlayerPrefs.Save();
        Debug.Log($"Monthly effects saved for {currentQuestion.questionId}");
    }

    public static void ApplyMonthlyEffects()
    {
        Debug.Log("Applying monthly effects...");
        
        var questionManager = GameObject.FindObjectOfType<QuestionContentManager>();
        if (questionManager == null)
        {
            Debug.LogError("QuestionContentManager not found - cannot apply monthly effects!");
            return;
        }

        // Store all changes first (excluding time)
        Dictionary<string, float> totalChanges = new Dictionary<string, float>()
        {
            {"Money", 0},
            {"Career", 0},
            {"Energy", 0},
            {"Creativity", 0}
        };

        foreach (var question in questionManager.GetAllQuestions())
        {
            string effectsKey = $"MonthlyEffects_{question.questionId}";
            if (PlayerPrefs.GetInt($"{effectsKey}_active", 0) == 1)
            {
                // Get stored monthly effects (excluding time)
                totalChanges["Money"] += PlayerPrefs.GetFloat($"{effectsKey}_money", 0);
                totalChanges["Career"] += PlayerPrefs.GetFloat($"{effectsKey}_career", 0);
                totalChanges["Energy"] += PlayerPrefs.GetFloat($"{effectsKey}_energy", 0);
                totalChanges["Creativity"] += PlayerPrefs.GetFloat($"{effectsKey}_creativity", 0);

                Debug.Log($"Added monthly effects from {question.questionId}:");
                Debug.Log($"Money: {totalChanges["Money"]}, Career: {totalChanges["Career"]}, " +
                         $"Energy: {totalChanges["Energy"]}, Creativity: {totalChanges["Creativity"]}");
            }
        }

        // Apply all changes at once with checkChanceCard = false
        if (PlayerState.Instance != null)
        {
            foreach (var change in totalChanges)
            {
                if (change.Value != 0)
                {
                    try
                    {
                        float currentValue = PlayerState.Instance.GetPlayerValue(change.Key);
                        float newValue = currentValue + change.Value;
                        PlayerState.Instance.SetPlayerValue(change.Key, newValue, false);
                        Debug.Log($"Applied {change.Key} change: {currentValue} + {change.Value} = {newValue}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error applying {change.Key} change: {e.Message}");
                    }
                }
            }
        }

        Debug.Log("Monthly effects application complete");
    }

    // Helper method to get all monthly effect keys
    private static List<string> GetAllMonthlyEffectKeys()
    {
        List<string> keys = new List<string>();
        
        var questionManager = GameObject.FindObjectOfType<QuestionContentManager>();
        if (questionManager != null)
        {
            foreach (var question in questionManager.GetAllQuestions())
            {
                string key = $"MonthlyEffects_{question.questionId}";
                if (PlayerPrefs.HasKey($"{key}_active"))
                {
                    keys.Add(key);
                }
            }
        }
        else
        {
            Debug.LogError("QuestionContentManager not found - cannot get monthly effect keys!");
        }
        
        return keys;
    }

    private void ApplyEffects(EffectData effects)
    {
        // Apply all effects
        if (effects != null)
        {
            Debug.Log("Applying effects to player state...");
            
            if (effects.money != 0)
            {
                float currentMoney = PlayerState.Instance.GetPlayerValue("Money");
                float newMoney = currentMoney + effects.money;
                Debug.Log($"Money: {currentMoney} + {effects.money} = {newMoney}");
                PlayerState.Instance?.SetPlayerValue("Money", newMoney, false);
            }
            
            if (effects.career != 0)
            {
                float currentCareer = PlayerState.Instance.GetPlayerValue("Career");
                float newCareer = currentCareer + effects.career;
                PlayerState.Instance?.SetPlayerValue("Career", newCareer, false);
            }
            
            if (effects.energy != 0)
            {
                float currentEnergy = PlayerState.Instance.GetPlayerValue("Energy");
                float newEnergy = currentEnergy + effects.energy;
                PlayerState.Instance?.SetPlayerValue("Energy", newEnergy, false);
            }
            
            if (effects.creativity != 0)
            {
                float currentCreativity = PlayerState.Instance.GetPlayerValue("Creativity");
                float newCreativity = currentCreativity + effects.creativity;
                PlayerState.Instance?.SetPlayerValue("Creativity", newCreativity, false);
            }
            
            if (effects.time != 0)
            {
                float currentTime = PlayerState.Instance.GetPlayerValue("Time");
                float newTime = currentTime + effects.time;
                PlayerState.Instance?.SetPlayerValue("Time", newTime, false);
            }
        }
    }

    public void OnContinueClicked()
    {
        // Track question appearance for all questions
        if (currentQuestion != null)
        {
            int appearances = PlayerPrefs.GetInt(currentQuestion.questionId + "_appearances", 0);
            PlayerPrefs.SetInt(currentQuestion.questionId + "_appearances", appearances + 1);
            PlayerPrefs.Save();
        }

        // Invoke completion event
        onQuestionComplete.Invoke();

        // First check if any stats are at 0 - this should take priority
        if (PlayerState.Instance != null)
        {
            string[] statsToCheck = { "Money", "Career", "Energy", "Creativity", "Time" };
            bool hasZeroStat = false;
            foreach (var stat in statsToCheck)
            {
                float value = PlayerState.Instance.GetPlayerValue(stat);
                if (value <= 0)
                {
                    Debug.LogWarning($"{stat} has hit 0 - Setting game over pending");
                    hasZeroStat = true;
                    PlayerState.Instance.SetGameOverPending(true);
                    break;
                }
            }

            // If we have a zero stat, we'll handle it in the main scene
            if (hasZeroStat)
            {
                // Return to main scene first
                SceneManager.LoadScene("main");
                return;
            }

            // Only check for critical levels if we haven't hit game over
            bool needsChanceCard = false;
            string criticalStat = "";
            foreach (var stat in statsToCheck)
            {
                float value = PlayerState.Instance.GetPlayerValue(stat);
                if (value > 0 && value <= 20)
                {
                    Debug.Log($"{stat} is at critical level: {value}");
                    needsChanceCard = true;
                    criticalStat = stat;
                    break;
                }
            }

            // Handle scene transitions
            if (needsChanceCard)
            {
                Debug.Log($"Loading ChanceCard scene due to critical {criticalStat} level");
                // Set the chanceCardKey before loading the scene
                if (PlayerState.Instance != null)
                {
                    PlayerState.Instance.SetChanceCardKey(criticalStat);
                }
                
                var playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                {
                    playerController.StartChanceCard();
                }
                else
                {
                    Debug.LogError("PlayerController not found, falling back to main scene");
                    SceneManager.LoadScene("main");
                }
            }
            else
            {
                // Return to main scene
                SceneManager.LoadScene("main");
            }
        }
        else
        {
            // If no PlayerState, just return to main scene
            SceneManager.LoadScene("main");
        }
    }

    private void SetupVideo(VideoPlayer player, string videoKey)
    {
        if (player != null && !string.IsNullOrEmpty(videoKey))
        {
            // Load and play video
            // Implementation depends on your video management system
        }
    }

    public void ShowQuestionPanel()
    {
        questionPanel.SetActive(true);
        optionsPanel.SetActive(false);
        resultsPanel.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        questionPanel.SetActive(false);
        optionsPanel.SetActive(true);
        resultsPanel.SetActive(false);
        
        // Reset panel alphas
        optionsCanvasGroup.alpha = 1f;
        resultsCanvasGroup.alpha = 0f;
        
        // Reset fade overlay
        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.raycastTarget = false;
        
        // Ensure score panel is visible when showing options
        if (scoreCanvasGroup != null)
        {
            scoreCanvasGroup.alpha = 1f;
            scoreCanvasGroup.interactable = true;
            scoreCanvasGroup.blocksRaycasts = true;
        }
    }

    public void ShowResultsPanel()
    {
        questionPanel.SetActive(false);
        optionsPanel.SetActive(false);
        resultsPanel.SetActive(true);
    }

    public static void ClearAllMonthlyEffects()
    {
        Debug.Log("Clearing all monthly effects...");
        
        foreach (string key in GetAllMonthlyEffectKeys())
        {
            PlayerPrefs.DeleteKey($"{key}_active");
            PlayerPrefs.DeleteKey($"{key}_money");
            PlayerPrefs.DeleteKey($"{key}_career");
            PlayerPrefs.DeleteKey($"{key}_energy");
            PlayerPrefs.DeleteKey($"{key}_creativity");
        }
        
        PlayerPrefs.DeleteKey("ActiveMonthlyEffects");
        PlayerPrefs.Save();
        Debug.Log("Monthly effects cleared");
    }
} 