using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Question : MonoBehaviour
{
    [HideInInspector] public string optionSelected { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject question;
    [SerializeField] private GameObject options;
    [SerializeField] private GameObject results;

    [Header("VALUES")]
    [SerializeField] private SceneName sceneName;
    [SerializeField] private QuestionType questionType;

    private string playerPrefsKey;
    private SceneManagerHelper sceneManager;

    /// <summary>
    /// Black = Only Once
    /// Orange = Repeated
    /// Green = Repeated (Max 2 Times - Then Stop This Question)
    /// Blue = Repeated (Max 2 Times - Before 3rd Year - Then Stop This Question)
    /// </summary>
    private enum QuestionType { Black, Orange, Green, Blue }
    private enum SceneName { home, gallery, social, studio, work }

    private void Awake()
    {
        // Find SceneManagerHelper
        sceneManager = SceneManagerHelper.Instance;
        if (sceneManager == null)
        {
            Debug.LogError("SceneManagerHelper not found! Make sure it exists in your start scene.");
        }

        StartQuestion();
    }

    private void OnEnable()
    {
        SetQuestionType();
    }

    private void StartQuestion()
    {
        options.SetActive(false);
        results.SetActive(false);
        question.SetActive(true);
    }

    private void StartOptions()
    {
        options.SetActive(true);
        question.SetActive(false);
    }

    private void StartResults()
    {
        results.SetActive(true);
        options.SetActive(false);
    }

    // Button Functions
    public void OnQuestionNextBtnClicked()
    {
        StartOptions();
    }

    public void OnOptionSelected(string value)
    {
        Debug.Log($"Option {value} is Selected");
        optionSelected = value;
        StartResults();
    }

    public void OnResultNextBtnClicked()
    {
        Debug.Log("Going Back To Main Scene");
        
        // Check if game over is pending
        if (PlayerState.Instance != null && PlayerState.Instance.IsGameOverPending())
        {
            Debug.Log("Game Over is pending, returning to main scene first");
            SceneManager.LoadScene("main");
        }
        else if (sceneManager != null)
        {
            sceneManager.ReturnToMain();
        }
        else
        {
            Debug.LogWarning("SceneManagerHelper not found, falling back to direct scene loading");
            SceneManager.LoadScene("main");
        }
    }

    private void SetQuestionType()
    {
        switch (questionType)
        {
            case QuestionType.Black:
                // Save PlayerPrefs; +1 to know that this question has been enabled once
                PlayerPrefs.SetInt(playerPrefsKey, GetQuestionValue() + 1);
                Debug.Log($"{playerPrefsKey} --- Has Been Set To {GetQuestionValue()}");
                break;

            case QuestionType.Orange:
                // We are not saving anything as Orange can be enabled without a max limit
                Debug.Log("Orange Has Been Enabled - No Value Saved!");
                break;

            case QuestionType.Green:
            case QuestionType.Blue:
                // Save PlayerPrefs; +1 to know that this question has been enabled once
                PlayerPrefs.SetInt(playerPrefsKey, GetQuestionValue() + 1);
                Debug.Log($"{playerPrefsKey} --- Has Been Set To {GetQuestionValue()}");
                break;
        }
    }

    public int GetQuestionValue()
    {
        return PlayerPrefs.HasKey(playerPrefsKey) ? PlayerPrefs.GetInt(playerPrefsKey) : 0;
    }

    public string GetQuestionType()
    {
        return questionType.ToString();
    }

    public void InitializeKeyForPlayerPrefs()
    {
        playerPrefsKey = $"{sceneName}-{gameObject.name}-{questionType}";
    }

    // Helper method to manually navigate to a specific scene if needed
    public void LoadSpecificScene(string sceneName)
    {
        if (sceneManager != null)
        {
            sceneManager.LoadSubScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneManagerHelper not found, falling back to direct scene loading");
            SceneManager.LoadScene(sceneName);
        }
    }
}