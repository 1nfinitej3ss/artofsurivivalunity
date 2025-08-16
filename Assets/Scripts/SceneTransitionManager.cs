using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager s_Instance;
    
    [SerializeField] private Color m_BackgroundColor = Color.black;
    private CanvasGroup m_FadeCanvasGroup;
    private GameObject m_TransitionCanvas; // Add reference to canvas
    
    public static SceneTransitionManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                var go = new GameObject("SceneTransitionManager");
                s_Instance = go.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(go);
                s_Instance.Initialize();
            }
            return s_Instance;
        }
    }
    
    private void Initialize()
    {
        // Set camera background color
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = m_BackgroundColor;
        }
        
        // Create overlay canvas
        m_TransitionCanvas = new GameObject("TransitionCanvas");
        m_TransitionCanvas.transform.SetParent(transform);
        
        var canvas = m_TransitionCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it renders on top
        
        m_FadeCanvasGroup = m_TransitionCanvas.AddComponent<CanvasGroup>();
        
        var panel = new GameObject("BlackPanel");
        panel.transform.SetParent(m_TransitionCanvas.transform);
        
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        var image = panel.AddComponent<UnityEngine.UI.Image>();
        image.color = m_BackgroundColor;
        
        // Ensure initial state is hidden
        m_FadeCanvasGroup.alpha = 0;
        m_FadeCanvasGroup.interactable = false;
        m_FadeCanvasGroup.blocksRaycasts = false;
        
        DontDestroyOnLoad(m_TransitionCanvas);
    }
    
    public void LoadScene(string _sceneName)
    {
        StartCoroutine(LoadSceneRoutine(_sceneName));
    }
    
    private IEnumerator LoadSceneRoutine(string _sceneName)
    {
        // Ensure canvas is active and ready
        if (m_TransitionCanvas != null)
        {
            m_TransitionCanvas.SetActive(true);
        }
        
        // Fade out
        float elapsedTime = 0;
        float fadeDuration = 0.2f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            m_FadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Load the scene
        var asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
        asyncOperation.allowSceneActivation = true;
        
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        // Ensure new scene's camera has black background
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = m_BackgroundColor;
        }
        
        // Fade in
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            m_FadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Ensure canvas is completely hidden and disabled
        m_FadeCanvasGroup.alpha = 0;
        m_FadeCanvasGroup.interactable = false;
        m_FadeCanvasGroup.blocksRaycasts = false;
        
        // Hide the entire canvas to prevent any visual artifacts
        if (m_TransitionCanvas != null)
        {
            m_TransitionCanvas.SetActive(false);
        }
    }
} 