using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreTrackerManager : MonoBehaviour
{
    public static ScoreTrackerManager Instance { get; private set; }
    
    [SerializeField] private GameObject controlledPanel;
    private bool isSearchingForPanel = false;
    
    private void Awake()
    {
        Debug.Log($"ScoreTrackerManager: Awake called on GameObject '{gameObject.name}'");
        
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        if (Instance != null && Instance != this)
        {
            Debug.Log("ScoreTrackerManager: Destroying duplicate instance");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("ScoreTrackerManager: Instance set and DontDestroyOnLoad called");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // If we're searching for a panel and don't have one yet
        if (isSearchingForPanel && controlledPanel == null)
        {
            FindPanelInCurrentScene();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"ScoreTrackerManager: New scene loaded - {scene.name}");
        controlledPanel = null;
        isSearchingForPanel = true;
        
        // Try to find immediately
        FindPanelInCurrentScene();
        
        // Will keep trying in Update if not found
    }

    private void FindPanelInCurrentScene()
    {
        var controller = FindObjectOfType<ScorePanelController>();
        if (controller != null)
        {
            controlledPanel = controller.gameObject;
            isSearchingForPanel = false;
            Debug.Log($"ScoreTrackerManager: Found new panel '{controlledPanel.name}' in current scene");
            ValidateSetup();
        }
    }

    private void ValidateSetup()
    {
        if (controlledPanel == null)
        {
            Debug.LogWarning("ScoreTrackerManager: No panel found in current scene");
        }
        else
        {
            var canvas = controlledPanel.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("ScoreTrackerManager: Panel must be child of a Canvas!");
            }
            
            var rectTransform = controlledPanel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("ScoreTrackerManager: Panel must have a RectTransform component!");
            }
        }
    }

    public void TogglePanel(bool show)
    {
        Debug.Log($"ScoreTrackerManager: TogglePanel called with show={show}");
        
        if (controlledPanel == null)
        {
            FindPanelInCurrentScene();
        }

        if (controlledPanel != null)
        {
            controlledPanel.SetActive(show);
            Debug.Log($"ScoreTrackerManager: Panel '{controlledPanel.name}' visibility set to {show}");
        }
        else
        {
            Debug.LogWarning($"ScoreTrackerManager: No panel found to control in current scene");
        }
    }
} 