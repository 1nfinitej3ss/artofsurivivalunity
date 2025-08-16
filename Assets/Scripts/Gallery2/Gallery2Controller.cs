using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this for UI components
using System.Collections;

public class Gallery2Controller : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private GameObject questionTemplatePrefab;
    [SerializeField] private GameObject scoreTrackerManagerPrefab; // Reference to the ScoreTrackerManager prefab
    [SerializeField] private GameObject scoreTrackerPanelPrefab; // Reference to the panel prefab
    private QuestionTemplate currentQuestion;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Debug.Log("Gallery2Controller Start");
        
        // First ensure ScoreTrackerManager exists
        if (ScoreTrackerManager.Instance == null)
        {
            if (scoreTrackerManagerPrefab != null)
            {
                GameObject managerObj = Instantiate(scoreTrackerManagerPrefab);
                DontDestroyOnLoad(managerObj);
                Debug.Log("ScoreTrackerManager instantiated");
            }
            else
            {
                Debug.LogError("ScoreTrackerManager prefab not assigned!");
                return;
            }
        }

        // Now handle the panel
        if (GameObject.FindObjectOfType<ScorePanelController>() == null)
        {
            if (scoreTrackerPanelPrefab != null)
            {
                // Create a canvas if it doesn't exist
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    GameObject canvasObj = new GameObject("MainCanvas");
                    mainCanvas = canvasObj.AddComponent<Canvas>();
                    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }

                // Instantiate the panel as a child of the canvas
                GameObject panel = Instantiate(scoreTrackerPanelPrefab, mainCanvas.transform);
                Debug.Log("Score tracker panel instantiated under canvas");
            }
            else
            {
                Debug.LogError("Score Tracker Panel prefab not assigned!");
            }
        }
        
        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return null;

        Debug.Log("[Gallery2] Getting random question from QuestionContentManager");
        QuestionData question = QuestionContentManager.Instance.GetRandomQuestionForScene("gallery");
        if (question != null)
        {
            Debug.Log($"Got gallery question: {question.questionId}");
            ShowQuestion(question);
        }
        else
        {
            Debug.LogError("No gallery question returned from QuestionContentManager");
            // Change this to a scene that exists in your build settings
            SceneManager.LoadScene("main"); // or whatever your main scene is called
        }
    }

    private void ShowQuestion(QuestionData questionData)
    {
        if (currentQuestion != null)
        {
            Destroy(currentQuestion.gameObject);
        }

        GameObject questionObj = Instantiate(questionTemplatePrefab, transform);
        questionObj.transform.SetAsLastSibling();
        
        currentQuestion = questionObj.GetComponent<QuestionTemplate>();
        currentQuestion.Initialize(questionData);
    }
    #endregion
} 