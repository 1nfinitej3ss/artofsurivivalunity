using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class QuestionContentManager : MonoBehaviour
{
    public static QuestionContentManager Instance { get; private set; }

    [SerializeField] private TextAsset questionDataFile;
    private QuestionData[] questions;
    private string currentJsonContent;
    private Dictionary<string, int> questionAppearances = new Dictionary<string, int>();
    private bool m_HasLoadedCustomJsonInStartScene = false;
    private const string CUSTOM_JSON_ADDED_KEY = "CustomJsonAdded";

    // Wrapper class for JSON deserialization
    [Serializable]
    private class QuestionDataWrapper
    {
        public QuestionData[] questions;
    }

    [Serializable]
    private class QuestionAppearanceData
    {
        public string questionId;
        public int appearances;
    }

    [Serializable]
    private class QuestionAppearanceWrapper
    {
        public QuestionAppearanceData[] appearances;
    }

    private bool hasShownFirstQuestion = false;
    private const string FIRST_QUESTION_ID = "home_1";

    private bool m_HasInitialized = false;
    private bool m_HasShownFirst = false;
    private List<QuestionData> m_Questions = new List<QuestionData>();

    private Dictionary<string, Dictionary<string, int>> sceneQuestionAppearances = new Dictionary<string, Dictionary<string, int>>();
    private Dictionary<string, bool> sceneCycleComplete = new Dictionary<string, bool>();

    private int GetCurrentYear()
    {
        // Get current year (1 is start/year 5, 5 is end/year 1)
        int currentYear = 1;  // Default to start of game
        if (PlayerState.Instance != null)
        {
            int daysPassed = PlayerState.Instance.totalDaysPassed;
            int yearsFromStart = daysPassed / 365;
            currentYear = 1 + yearsFromStart;  // Start at 1 and count up
            currentYear = Mathf.Clamp(currentYear, 1, 5);
        }
        return currentYear;
    }

    private void LogState(string context)
    {
        Debug.Log($"[QCM State] {context}:");
        Debug.Log($"- Instance null? {Instance == null}");
        Debug.Log($"- This is instance? {Instance == this}");
        Debug.Log($"- Questions null? {questions == null}");
        Debug.Log($"- Questions count: {questions?.Length ?? 0}");
        Debug.Log($"- Has shown first? {hasShownFirstQuestion}");
        Debug.Log($"- Current JSON null? {string.IsNullOrEmpty(currentJsonContent)}");
        Debug.Log($"- PlayerPrefs JSON exists? {PlayerPrefs.HasKey("CustomJsonContent")}");
    }

    private void Awake()
    {
        LogState("Awake - Before");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize question appearances dictionary
            questionAppearances = new Dictionary<string, int>();
            
            // Load hasShownFirstQuestion state
            hasShownFirstQuestion = PlayerPrefs.GetInt("HasShownFirstQuestion", 0) == 1;
            
            // Always load from question data file first
            if (questionDataFile != null)
            {
                Debug.Log($"Loading questions from default data file: {questionDataFile.name}");
                LoadCustomJson(questionDataFile.text);
            }
            
            // Only load from PlayerPrefs if we're in the start scene and custom JSON was explicitly added
            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            if ((currentScene == "start" || currentScene == "startscene") && 
                PlayerPrefs.GetInt(CUSTOM_JSON_ADDED_KEY, 0) == 1)
            {
                string savedJson = PlayerPrefs.GetString("CustomJsonContent", "");
                if (!string.IsNullOrEmpty(savedJson))
                {
                    Debug.Log("Loading custom JSON from PlayerPrefs in start scene");
                    Debug.Log($"Current scene: {currentScene}");
                    Debug.Log($"Question data file: {(questionDataFile != null ? questionDataFile.name : "null")}");
                    Debug.Log($"Custom JSON length: {savedJson.Length} characters");
                    LoadCustomJson(savedJson);
                    m_HasLoadedCustomJsonInStartScene = true;
                }
                else
                {
                    Debug.Log("No custom JSON found in PlayerPrefs, using default question data file");
                }
            }
            
            Debug.Log("QuestionContentManager initialized and set to persist");
        }
        else if (Instance != this)
        {
            Debug.Log("Transferring state to new QuestionContentManager instance");
            // Store old instance reference before changing it
            var oldInstance = Instance;
            // Transfer state
            questions = Instance.questions;
            currentJsonContent = Instance.currentJsonContent;
            hasShownFirstQuestion = Instance.hasShownFirstQuestion;
            questionAppearances = new Dictionary<string, int>(Instance.questionAppearances);
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"Destroying old instance: {oldInstance.gameObject.name}");
            Destroy(oldInstance.gameObject);
        }
        LogState("Awake - After");
    }

    public bool HasLoadedQuestions()
    {
        return questions != null && questions.Length > 0;
    }

    public void LoadCustomJson(string jsonContent)
    {
        try
        {
            Debug.Log("Loading custom JSON content");
            
            // Store the JSON content
            currentJsonContent = jsonContent;
            
            // Deserialize the JSON
            var wrapper = JsonUtility.FromJson<QuestionDataWrapper>(jsonContent);
            
            if (wrapper != null && wrapper.questions != null)
            {
                questions = wrapper.questions;
                Debug.Log($"Successfully loaded {questions.Length} questions");
                
                // Log which file we're using
                if (jsonContent == questionDataFile?.text)
                {
                    Debug.Log($"Using default question data file: {questionDataFile.name}");
                }
                else
                {
                    Debug.Log("Using custom JSON from PlayerPrefs");
                }
                
                // Log detailed breakdown
                var scenes = questions.Select(q => q.scene?.ToLower()).Distinct().OrderBy(s => s);
                foreach (var scene in scenes)
                {
                    var sceneQuestions = questions.Where(q => q.scene?.ToLower() == scene).ToList();
                    Debug.Log($"Scene '{scene}': {sceneQuestions.Count} questions");
                    
                    // Breakdown by occurrence type
                    var nonRepeatable = sceneQuestions.Count(q => q.maxOccurrences == 1);
                    var repeatable = sceneQuestions.Count(q => q.maxOccurrences == null || q.maxOccurrences > 1);
                    var yearLimited = sceneQuestions.Count(q => q.hasYearLimit);
                    
                    Debug.Log($"  - Non-repeatable: {nonRepeatable}");
                    Debug.Log($"  - Repeatable: {repeatable}");
                    Debug.Log($"  - Year limited: {yearLimited}");
                }
            }
            else
            {
                Debug.LogError("Failed to deserialize questions from JSON");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading custom JSON: {e.Message}");
            questions = null;
        }
    }

    private int? ConvertTypeToMaxOccurrences(string type)
    {
        switch (type?.ToLower())
        {
            case "black": return 1;
            case "orange": return null;  // unlimited
            case "green": return 2;
            case "blue": return 2;
            default: return null;
        }
    }

    // Legacy class for old JSON format
    [Serializable]
    private class LegacyQuestionData
    {
        public string questionId;
        public string type;
        public string text;
        public string scene;
        public OptionData[] options;
        public int minYear;
        public int maxYear;
        public bool isMonthlyEffect;
        public string questionBackgroundKey;
        public string[] requirements;
    }

    [Serializable]
    private class LegacyQuestionWrapper
    {
        public LegacyQuestionData[] questions;
    }

    private void LoadQuestions()
    {
        if (questionDataFile != null)
        {
            Debug.Log($"Attempting to load questions from {questionDataFile.name}");
            try
            {
                QuestionDataWrapper wrapper = JsonUtility.FromJson<QuestionDataWrapper>(questionDataFile.text);
                questions = wrapper.questions;
                Debug.Log($"Loaded {questions.Length} questions successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading questions: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("No question data file assigned!");
        }
    }

    public QuestionData GetQuestion(string questionId)
    {
        return Array.Find(questions, q => q.questionId == questionId);
    }

    public QuestionData GetRandomQuestion()
    {
        LogState("GetRandomQuestion - Start");

        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("No questions available!");
            return null;
        }

        // If this is the first question and home_1 exists, return it
        if (!hasShownFirstQuestion)
        {
            var firstQuestion = questions.FirstOrDefault(q => q.questionId == FIRST_QUESTION_ID);
            if (firstQuestion != null)
            {
                Debug.Log($"Showing first question: {FIRST_QUESTION_ID}");
                hasShownFirstQuestion = true;
                
                // Save the state
                PlayerPrefs.SetInt("HasShownFirstQuestion", 1);
                PlayerPrefs.Save();
                
                // Initialize appearance tracking
                if (!questionAppearances.ContainsKey(FIRST_QUESTION_ID))
                {
                    questionAppearances[FIRST_QUESTION_ID] = 0;
                }
                questionAppearances[FIRST_QUESTION_ID]++;
                
                return firstQuestion;
            }
            else
            {
                Debug.LogWarning($"First question {FIRST_QUESTION_ID} not found in loaded questions!");
                hasShownFirstQuestion = true;
                PlayerPrefs.SetInt("HasShownFirstQuestion", 1);
                PlayerPrefs.Save();
            }
        }

        // For subsequent questions, exclude home_1 and get random from remaining
        var eligibleQuestions = questions
            .Where(q => q.questionId != FIRST_QUESTION_ID) // Exclude home_1
            .Where(q => !questionAppearances.ContainsKey(q.questionId) || 
                        questionAppearances[q.questionId] == questionAppearances
                            .Where(kvp => kvp.Key != FIRST_QUESTION_ID) // Exclude home_1 from min calculation
                            .Min(kvp => kvp.Value))
            .ToList();

        if (eligibleQuestions.Count == 0)
        {
            Debug.LogError("No eligible questions found!");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, eligibleQuestions.Count);
        var selectedQuestion = eligibleQuestions[randomIndex];

        // Update appearance count
        if (!questionAppearances.ContainsKey(selectedQuestion.questionId))
        {
            questionAppearances[selectedQuestion.questionId] = 0;
        }
        questionAppearances[selectedQuestion.questionId]++;

        return selectedQuestion;
    }

    public bool CheckQuestionRequirements(QuestionData question)
    {
        if (question == null) return false;

        // Get current year (1 is start/year 5, 5 is end/year 1)
        int currentYear = 1;  // Default to start of game
        if (PlayerState.Instance != null)
        {
            int daysPassed = PlayerState.Instance.totalDaysPassed;
            int yearsFromStart = daysPassed / 365;
            currentYear = 1 + yearsFromStart;  // Start at 1 and count up
            currentYear = Mathf.Clamp(currentYear, 1, 5);
        }

        // Check year requirements
        if (currentYear < question.minYear || currentYear > question.maxYear)
            return false;

        // Check year limit (former Blue type questions)
        if (question.hasYearLimit && currentYear > 3)
            return false;

        // Check max occurrences
        if (question.maxOccurrences.HasValue)
        {
            int appearances = questionAppearances.ContainsKey(question.questionId) 
                ? questionAppearances[question.questionId] 
                : 0;
            
            if (appearances >= question.maxOccurrences.Value)
                return false;
        }

        // Check other requirements
        if (question.requirements != null && question.requirements.Length > 0)
        {
            // Implement requirement checking logic
            // This would check things like "hasPartner", etc.
        }

        return true;
    }

    public void ResetGameState()
    {
        Debug.Log("Performing full game state reset...");
        
        // Reset first question flag
        hasShownFirstQuestion = false;
        
        // Clear question appearances
        questionAppearances.Clear();
        
        // Clear saved state in PlayerPrefs
        PlayerPrefs.DeleteKey("QuestionAppearances");
        PlayerPrefs.DeleteKey("HasShownFirstQuestion");
        
        // Save the reset state
        PlayerPrefs.Save();
        
        Debug.Log("Game state reset complete");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset question state when returning to start scene
        if (scene.name.ToLower() == "start" || scene.name.ToLower() == "startscene")
        {
            ResetGameState();
        }
    }

    private void SaveQuestionAppearances()
    {
        var wrapper = new QuestionAppearanceWrapper
        {
            appearances = questionAppearances.Select(kvp => new QuestionAppearanceData 
            { 
                questionId = kvp.Key, 
                appearances = kvp.Value 
            }).ToArray()
        };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("QuestionAppearances", json);
    }

    private void LoadQuestionAppearances()
    {
        string json = PlayerPrefs.GetString("QuestionAppearances", "");
        if (!string.IsNullOrEmpty(json))
        {
            var wrapper = JsonUtility.FromJson<QuestionAppearanceWrapper>(json);
            questionAppearances = wrapper.appearances.ToDictionary(
                data => data.questionId,
                data => data.appearances
            );
        }
        else
        {
            questionAppearances = new Dictionary<string, int>();
        }
    }

    private void OnEnable()
    {
        LogState("OnEnable - Before");
        if (Instance == this && !HasLoadedQuestions())
        {
            // Always try to load from question data file first
            if (questionDataFile != null)
            {
                Debug.Log($"Loading questions from default data file on enable: {questionDataFile.name}");
                LoadCustomJson(questionDataFile.text);
            }
            
            // Only load from PlayerPrefs if we're in the start scene and custom JSON was explicitly added
            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            if ((currentScene == "start" || currentScene == "startscene") && 
                PlayerPrefs.GetInt(CUSTOM_JSON_ADDED_KEY, 0) == 1)
            {
                string savedJson = PlayerPrefs.GetString("CustomJsonContent", "");
                if (!string.IsNullOrEmpty(savedJson))
                {
                    Debug.Log("Loading custom JSON from PlayerPrefs in start scene on enable");
                    Debug.Log($"Current scene: {currentScene}");
                    Debug.Log($"Question data file: {(questionDataFile != null ? questionDataFile.name : "null")}");
                    Debug.Log($"Custom JSON length: {savedJson.Length} characters");
                    LoadCustomJson(savedJson);
                    m_HasLoadedCustomJsonInStartScene = true;
                }
                else
                {
                    Debug.Log("No custom JSON found in PlayerPrefs, using default question data file");
                }
            }
            
            LoadQuestionAppearances();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        LogState("OnEnable - After");

        if (!m_HasInitialized)
        {
            InitializeQuestions();
        }
        
        if (m_Questions != null && m_Questions.Count > 0 && !m_HasShownFirst)
        {
            ShowNextQuestion();
        }
    }

    private void OnDisable()
    {
        LogState("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        LogState("OnDestroy - Before");
        if (Instance == this)
        {
            Debug.LogWarning("QuestionContentManager instance being destroyed - saving state");
            if (!string.IsNullOrEmpty(currentJsonContent))
            {
                PlayerPrefs.SetString("CustomJsonContent", currentJsonContent);
                SaveQuestionAppearances();
                PlayerPrefs.Save();
            }
            // Don't null out Instance if we're just changing scenes
            if (!gameObject.scene.isLoaded)
            {
                Instance = null;
            }
        }
        LogState("OnDestroy - After");
    }

    // Add this method to maintain compatibility with existing code
    public void ResetQuestionState()
    {
        Debug.LogWarning("ResetQuestionState is deprecated. Please use ResetGameState instead.");
        ResetGameState();
    }

    // Add this method to get all questions
    public QuestionData[] GetAllQuestions()
    {
        // If you already have a questions array/list as a field, return that
        return questions;  // Assuming 'questions' is your existing array/list of questions
        
        // OR if you need to load them from JSON:
        // return LoadQuestionsFromJSON();
    }

    private void InitializeSceneTracking()
    {
        string[] scenes = { "work", "home", "studio", "social", "gallery" };
        foreach (string scene in scenes)
        {
            if (!sceneQuestionAppearances.ContainsKey(scene))
            {
                sceneQuestionAppearances[scene] = new Dictionary<string, int>();
            }
            sceneCycleComplete[scene] = false;
        }
    }

    private bool HaveAllQuestionsInSceneBeenShown(string scene, List<QuestionData> sceneQuestions)
    {
        if (!sceneQuestionAppearances.ContainsKey(scene)) return false;
        
        var appearances = sceneQuestionAppearances[scene];
        int currentYear = GetCurrentYear();
        foreach (var question in sceneQuestions)
        {
            // Skip questions that aren't eligible in the current year
            if (question.minYear > currentYear || question.maxYear < currentYear) continue;
            if (question.hasYearLimit && currentYear > 3) continue;
            
            // If an eligible question hasn't been shown, return false
            if (!appearances.ContainsKey(question.questionId))
            {
                return false;
            }
        }
        return true;
    }

    private void ResetSceneCycle(string scene)
    {
        if (sceneQuestionAppearances.ContainsKey(scene))
        {
            var appearances = sceneQuestionAppearances[scene];
            // Only reset questions that haven't reached their max occurrences
            foreach (var questionId in appearances.Keys.ToList())
            {
                var question = GetQuestion(questionId);
                if (question != null && (question.maxOccurrences == null || appearances[questionId] < question.maxOccurrences))
                {
                    appearances[questionId] = 0;
                }
            }
        }
        sceneCycleComplete[scene] = false;
    }

    public QuestionData GetRandomQuestionForScene(string scene)
    {
        Debug.Log($"[QCM] Getting random question for scene: {scene}");
        
        if (!m_HasInitialized)
        {
            InitializeQuestions();
        }

        if (!sceneQuestionAppearances.ContainsKey(scene))
        {
            InitializeSceneTracking();
        }

        // Get all questions for this scene
        var sceneQuestions = questions.Where(q => q.scene == scene).ToList();
        var eligibleQuestions = new List<QuestionData>();
        int currentYear = GetCurrentYear();

        // Check if all questions in the scene have been shown
        if (!sceneCycleComplete[scene] && HaveAllQuestionsInSceneBeenShown(scene, sceneQuestions))
        {
            Debug.Log($"[QCM] All questions in {scene} have been shown, resetting cycle");
            ResetSceneCycle(scene);
            sceneCycleComplete[scene] = true;
        }

        // Get the minimum appearance count for this scene
        int minAppearances = sceneQuestionAppearances[scene].Count > 0 
            ? sceneQuestionAppearances[scene].Values.Min() 
            : 0;

        foreach (var question in sceneQuestions)
        {
            // Skip if not within year range
            if (question.minYear > currentYear || question.maxYear < currentYear) continue;
            if (question.hasYearLimit && currentYear > 3) continue;

            bool hasAppeared = sceneQuestionAppearances[scene].ContainsKey(question.questionId);
            int appearances = hasAppeared ? sceneQuestionAppearances[scene][question.questionId] : 0;

            // Add question if:
            // 1. It hasn't appeared yet, or
            // 2. It's at minimum appearances AND all other questions have been shown once
            if (!hasAppeared || 
                (sceneCycleComplete[scene] && 
                 appearances == minAppearances && 
                 (question.maxOccurrences == null || appearances < question.maxOccurrences)))
            {
                eligibleQuestions.Add(question);
            }
        }

        if (eligibleQuestions.Count == 0)
        {
            Debug.LogWarning($"[QCM] No eligible questions found for scene {scene}");
            return null;
        }

        // Pick a random question from eligible ones
        int randomIndex = UnityEngine.Random.Range(0, eligibleQuestions.Count);
        var selectedQuestion = eligibleQuestions[randomIndex];
        Debug.Log($"[QCM] Selected question: {selectedQuestion.questionId}");

        // Track appearance
        if (!sceneQuestionAppearances[scene].ContainsKey(selectedQuestion.questionId))
        {
            sceneQuestionAppearances[scene][selectedQuestion.questionId] = 1;
        }
        else
        {
            sceneQuestionAppearances[scene][selectedQuestion.questionId]++;
        }

        // Also update the main questionAppearances for compatibility
        if (!questionAppearances.ContainsKey(selectedQuestion.questionId))
        {
            questionAppearances[selectedQuestion.questionId] = 1;
        }
        else
        {
            questionAppearances[selectedQuestion.questionId]++;
        }

        SaveQuestionAppearances();
        return selectedQuestion;
    }

    public void ClearSavedContent()
    {
        Debug.Log("Clearing saved JSON content from PlayerPrefs");
        PlayerPrefs.DeleteKey("CustomJsonContent");
        PlayerPrefs.DeleteKey("QuestionAppearances");
        PlayerPrefs.DeleteKey("HasShownFirstQuestion");
        PlayerPrefs.Save();
        
        // Reset local state
        questions = null;
        currentJsonContent = null;
        questionAppearances.Clear();
        sceneQuestionAppearances.Clear();
        sceneCycleComplete.Clear();
        hasShownFirstQuestion = false;
    }

    private void InitializeQuestions()
    {
        LoadQuestionsFromJSON();
        m_HasInitialized = true;
    }

    private void LoadQuestionsFromJSON()
    {
        // Implementation of LoadQuestionsFromJSON method
    }

    private void ShowNextQuestion()
    {
        // Implementation of ShowNextQuestion method
    }

    // Add this method to allow resetting the custom JSON flag
    public void ResetCustomJsonFlag()
    {
        m_HasLoadedCustomJsonInStartScene = false;
    }

    // Add this method to be called when custom JSON is explicitly added
    public void SetCustomJsonAdded()
    {
        PlayerPrefs.SetInt(CUSTOM_JSON_ADDED_KEY, 1);
        PlayerPrefs.Save();
    }

    // Add this method to clear the custom JSON added flag
    public void ClearCustomJsonAdded()
    {
        PlayerPrefs.DeleteKey(CUSTOM_JSON_ADDED_KEY);
        PlayerPrefs.Save();
    }
} 