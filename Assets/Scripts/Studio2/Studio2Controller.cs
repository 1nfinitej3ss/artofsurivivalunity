using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Studio2Controller : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject questionTemplatePrefab;
    [SerializeField] private GameObject scoreTrackerManagerPrefab;
    [SerializeField] private GameObject scoreTrackerPanelPrefab;
    #endregion

    #region Private Fields
    private QuestionTemplate m_CurrentQuestion;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Debug.Log("Studio2Controller Start");
        InitializeScoreTracking();
        StartCoroutine(InitializeWithDelay());
    }
    #endregion

    #region Private Methods
    private void InitializeScoreTracking()
    {
        // Initialize ScoreTrackerManager if it doesn't exist
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

        // Initialize score panel if it doesn't exist
        if (GameObject.FindObjectOfType<ScorePanelController>() == null)
        {
            if (scoreTrackerPanelPrefab != null)
            {
                // Create or find main canvas
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    GameObject canvasObj = new GameObject("MainCanvas");
                    mainCanvas = canvasObj.AddComponent<Canvas>();
                    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }

                // Instantiate the panel under the canvas
                GameObject panel = Instantiate(scoreTrackerPanelPrefab, mainCanvas.transform);
                Debug.Log("Score tracker panel instantiated under canvas");
            }
            else
            {
                Debug.LogError("Score Tracker Panel prefab not assigned!");
            }
        }
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return null;

        Debug.Log("[Studio2] Checking QuestionContentManager state");
        if (QuestionContentManager.Instance == null)
        {
            Debug.LogError("[Studio2] QuestionContentManager.Instance is null! Loading default questions...");
            GameObject qcmObject = new GameObject("QuestionContentManager");
            var qcm = qcmObject.AddComponent<QuestionContentManager>();
            DontDestroyOnLoad(qcmObject);
            
            yield return null;
        }
        
        Debug.Log("[Studio2] Checking if questions are loaded");
        if (!QuestionContentManager.Instance.HasLoadedQuestions())
        {
            Debug.LogError("[Studio2] No questions loaded! Please load questions in the start scene first.");
            SceneManager.LoadScene("StartScene");
            yield break;
        }
        
        Debug.Log("[Studio2] Getting random question from QuestionContentManager");
        QuestionData question = QuestionContentManager.Instance.GetRandomQuestionForScene("studio");
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

    private void ShowQuestion(QuestionData _questionData)
    {
        if (m_CurrentQuestion != null)
        {
            Destroy(m_CurrentQuestion.gameObject);
        }

        GameObject questionObj = Instantiate(questionTemplatePrefab, transform);
        questionObj.transform.SetAsLastSibling();
        
        m_CurrentQuestion = questionObj.GetComponent<QuestionTemplate>();
        m_CurrentQuestion.Initialize(_questionData);
    }
    #endregion
} 