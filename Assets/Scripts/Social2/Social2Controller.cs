using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Social2Controller : MonoBehaviour
{
    [SerializeField] private GameObject questionTemplatePrefab;
    [SerializeField] private GameObject scoreTrackerManagerPrefab; // Reference to the ScoreTrackerManager prefab
    [SerializeField] private GameObject scoreTrackerPanelPrefab; // Reference to the panel prefab

    private QuestionTemplate currentQuestion;

    private void Start()
    {
        Debug.Log("Social2Controller Start");
        
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

        Debug.Log("[Social2] Checking QuestionContentManager state");
        if (QuestionContentManager.Instance == null)
        {
            Debug.LogError("[Social2] QuestionContentManager.Instance is null! Loading default questions...");
            GameObject qcmObject = new GameObject("QuestionContentManager");
            var qcm = qcmObject.AddComponent<QuestionContentManager>();
            DontDestroyOnLoad(qcmObject);
            
            yield return null;
        }
        
        Debug.Log("[Social2] Checking if questions are loaded");
        if (!QuestionContentManager.Instance.HasLoadedQuestions())
        {
            Debug.LogError("[Social2] No questions loaded! Please load questions in the start scene first.");
            SceneManager.LoadScene("StartScene");
            yield break;
        }
        
        Debug.Log("[Social2] Getting random question from QuestionContentManager");
        QuestionData question = QuestionContentManager.Instance.GetRandomQuestionForScene("social");
        if (question != null)
        {
            Debug.Log($"Got question: {question.questionId}");
            ShowQuestion(question);
        }
        else
        {
            Debug.LogError("No question returned from QuestionContentManager");
        }
    }

    private void ShowQuestion(QuestionData questionData)
    {
        if (currentQuestion != null)
        {
            Destroy(currentQuestion.gameObject);
        }

        GameObject questionObj = Instantiate(questionTemplatePrefab, transform);
        // Set as last sibling to ensure it appears on top
        questionObj.transform.SetAsLastSibling();
        
        currentQuestion = questionObj.GetComponent<QuestionTemplate>();
        currentQuestion.Initialize(questionData);
    }
} 