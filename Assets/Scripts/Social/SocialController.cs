using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SocialController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject questionTemplatePrefab;
    [SerializeField] private GameObject scoreTrackerManagerPrefab;
    [SerializeField] private GameObject scoreTrackerPanelPrefab;
    #endregion

    #region Private Fields
    private QuestionTemplate currentQuestion;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Debug.Log("SocialController Start");
        
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
        
        // Continue with existing initialization
        StartCoroutine(InitializeWithDelay());
    }
    #endregion

    #region Private Methods
    private IEnumerator InitializeWithDelay()
    {
        yield return null;

        Debug.Log("[Social] Checking QuestionContentManager state");
        if (QuestionContentManager.Instance == null)
        {
            Debug.LogError("[Social] QuestionContentManager.Instance is null! Loading default questions...");
            GameObject qcmObject = new GameObject("QuestionContentManager");
            var qcm = qcmObject.AddComponent<QuestionContentManager>();
            DontDestroyOnLoad(qcmObject);
            
            yield return null;
        }
        
        Debug.Log("[Social] Checking if questions are loaded");
        if (!QuestionContentManager.Instance.HasLoadedQuestions())
        {
            Debug.LogError("[Social] No questions loaded! Please load questions in the start scene first.");
            SceneManager.LoadScene("StartScene");
            yield break;
        }
        
        Debug.Log("[Social] Getting random question from QuestionContentManager");
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
    #endregion
} 