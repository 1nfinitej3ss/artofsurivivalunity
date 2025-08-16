using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckForZero : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private PlayerState m_PlayerState;
    private bool m_IsInMainScene = false;
    private readonly string[] m_StatsToCheck = { "Money", "Career", "Energy", "Creativity", "Time" };
    private bool m_HasLoggedStats = false;
    private bool m_GameOverTriggered = false;
    private bool m_IsInitialized = false;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Debug.Log("CheckForZero: Awake called");
        InitializeComponent();
    }

    private void Start()
    {
        Debug.Log("CheckForZero: Start called");
        if (!m_IsInitialized)
        {
            InitializeComponent();
        }
    }

    private void InitializeComponent()
    {
        if (m_PlayerState == null)
        {
            m_PlayerState = GetComponent<PlayerState>();
            if (m_PlayerState == null)
            {
                Debug.LogError("CheckForZero: Failed to find PlayerState component!");
                enabled = false;
                return;
            }
        }
        m_IsInitialized = true;
        Debug.Log("CheckForZero: Successfully initialized");
    }

    private void OnEnable()
    {
        Debug.Log("CheckForZero: OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
        m_GameOverTriggered = false;
    }

    private void OnDisable()
    {
        Debug.Log("CheckForZero: OnDisable called");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_IsInMainScene = (scene.name == "MainScene" || scene.name == "main");
        m_HasLoggedStats = false;
        m_GameOverTriggered = false;
        Debug.Log($"CheckForZero: Scene loaded - {scene.name}, IsMainScene: {m_IsInMainScene}");
        
        if (m_IsInMainScene)
        {
            LogAllStats(); // Log stats immediately when entering main scene
        }
    }

    private void Update()
    {
        if (!m_IsInitialized || !m_IsInMainScene || m_GameOverTriggered)
        {
            return;
        }

        if (!m_HasLoggedStats)
        {
            LogAllStats();
            m_HasLoggedStats = true;
        }
        
        CheckGameOverConditions();
    }
    #endregion

    #region Private Methods
    private void LogAllStats()
    {
        if (m_PlayerState == null)
        {
            Debug.LogError("CheckForZero: PlayerState is null during stat logging!");
            return;
        }

        Debug.Log("CheckForZero: Current Stats:");
        foreach (string stat in m_StatsToCheck)
        {
            float value = m_PlayerState.GetPlayerValue(stat);
            Debug.Log($"CheckForZero: {stat} = {value}");
        }
    }

    private void CheckGameOverConditions()
    {
        if (m_PlayerState == null || m_GameOverTriggered)
        {
            return;
        }

        foreach (string stat in m_StatsToCheck)
        {
            float value = m_PlayerState.GetPlayerValue(stat);
            if (value <= 0)
            {
                Debug.LogWarning($"CheckForZero: Game Over triggered by {stat} falling to {value}");
                m_GameOverTriggered = true;
                m_PlayerState.SaveFinalScores();
                m_PlayerState.SetGameOverPending(true);
                SceneManager.LoadScene("main");
                enabled = false;
                return;
            }
        }
    }
    #endregion

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_PlayerState == null)
        {
            m_PlayerState = GetComponent<PlayerState>();
        }
    }
    #endif
}
