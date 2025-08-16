using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class ScorePanelController : MonoBehaviour
{
    private float updateInterval = 0.5f;
    private float nextUpdateTime = 0f;
    private DayNightCycle2D dayNightSystem;

    [Header("Text Elements")]
    [SerializeField] private Text moneyText;
    [SerializeField] private Text careerText;
    [SerializeField] private Text creativityText;
    [SerializeField] private Text energyText;
    [SerializeField] private Text timeText;

    [Header("Progress Bars")]
    [SerializeField] private Image moneyProgressBar;
    [SerializeField] private Image careerProgressBar;
    [SerializeField] private Image creativityProgressBar;
    [SerializeField] private Image energyProgressBar;
    [SerializeField] private Image timeProgressBar;

    [Header("Time Display")]
    [SerializeField] private TimeData m_TimeData;
    [SerializeField] private Text m_TimeDisplayText;

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"ScorePanelController: No Canvas found in parents of {gameObject.name}!");
        }

        // Find the DayNightCycle2D system
        dayNightSystem = FindObjectOfType<DayNightCycle2D>();
        if (dayNightSystem == null)
        {
            Debug.LogWarning("ScorePanelController: Could not find DayNightCycle2D system!");
        }

        // Find TimeData if not assigned
        if (m_TimeData == null)
        {
            m_TimeData = Resources.Load<TimeData>("TimeData");
            if (m_TimeData == null)
            {
                Debug.LogError("TimeData asset not found! Please create it in Resources folder");
            }
        }

        // Subscribe to Results panel state changes
        foreach (var resultsPanel in GameObject.FindGameObjectsWithTag("Results"))
        {
            if (resultsPanel.TryGetComponent<Canvas>(out var resultsPanelCanvas))
            {
                resultsPanelCanvas.enabled = false; // Ensure Results panels start hidden
            }
        }
    }

    private void OnEnable()
    {
        //Debug.Log($"ScorePanelController: OnEnable called on {gameObject.name}");
        UpdateScoreDisplay();
        UpdatePanelVisibility();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //Debug.Log("ScorePanelController: OnDisable called");
        if (ScoreTrackerManager.Instance != null)
        {
            ScoreTrackerManager.Instance.TogglePanel(false);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdatePanelVisibility();
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateScoreDisplay();
            UpdateTimeDisplay();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateScoreDisplay()
    {
        if (PlayerState.Instance != null)
        {
            // Update stats displays
            float moneyValue = PlayerState.Instance.GetPlayerValue("Money");
            float careerValue = PlayerState.Instance.GetPlayerValue("Career");
            float creativityValue = PlayerState.Instance.GetPlayerValue("Creativity");
            float energyValue = PlayerState.Instance.GetPlayerValue("Energy");
            float timeValue = PlayerState.Instance.GetPlayerValue("Time");

            // Update text displays
            if (moneyText) moneyText.text = $"{moneyValue:F0}";
            if (careerText) careerText.text = $"{careerValue:F0}";
            if (creativityText) creativityText.text = $"{creativityValue:F0}";
            if (energyText) energyText.text = $"{energyValue:F0}";
            if (timeText) timeText.text = $"{timeValue:F0}";

            // Update progress bars
            UpdateProgressBar(moneyProgressBar, moneyValue);
            UpdateProgressBar(careerProgressBar, careerValue);
            UpdateProgressBar(creativityProgressBar, creativityValue);
            UpdateProgressBar(energyProgressBar, energyValue);
            UpdateProgressBar(timeProgressBar, timeValue);
        }
    }

    private void UpdateProgressBar(Image progressBar, float value)
    {
        if (progressBar != null)
        {
            // Convert value to 0-1 range
            float normalizedValue = value / 100f;
            
            // Simply set the fill amount
            progressBar.fillAmount = normalizedValue;
        }
    }

    private void UpdatePanelVisibility()
    {
        if (!TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            Debug.LogError($"ScorePanelController: No CanvasGroup component found on {gameObject.name}!");
            return;
        }

        // Check if any results panels are active
        bool anyResultsActive = GameObject.FindGameObjectsWithTag("Results")
            .Any(results => results != null && results.activeSelf);

        // Update both the ScoreTracker visibility and this panel's visibility
        canvasGroup.alpha = anyResultsActive ? 0f : 1f;
        canvasGroup.interactable = !anyResultsActive;
        canvasGroup.blocksRaycasts = !anyResultsActive;
        
        ScoreTrackerManager.Instance.TogglePanel(!anyResultsActive);
    }

    private void UpdateTimeDisplay()
    {
        if (m_TimeDisplayText != null && m_TimeData != null)
        {
            // Check if any days have passed using PlayerState
            if (PlayerState.Instance != null && !PlayerState.Instance.HasAnyDaysPassed())
            {
                m_TimeDisplayText.text = "5 years left";
            }
            else
            {
                m_TimeDisplayText.text = m_TimeData.CountdownText;
            }
        }
    }
} 