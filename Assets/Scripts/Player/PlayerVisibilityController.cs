using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVisibilityController : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private string m_MainSceneName = "Main";
    private SpriteRenderer[] m_SpriteRenderers;
    private Color[] m_OriginalColors;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    #endregion

    #region Private Methods
    private void InitializeComponents()
    {
        // Get all sprite renderers on this object and its children
        m_SpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        m_OriginalColors = new Color[m_SpriteRenderers.Length];
        
        // Store original colors
        for (int i = 0; i < m_SpriteRenderers.Length; i++)
        {
            m_OriginalColors[i] = m_SpriteRenderers[i].color;
        }
    }

    private void OnActiveSceneChanged(Scene _prevScene, Scene _newScene)
    {
        bool isMainScene = _newScene.name == m_MainSceneName;
        SetPlayerVisibility(isMainScene);
    }

    private void SetPlayerVisibility(bool _visible)
    {
        for (int i = 0; i < m_SpriteRenderers.Length; i++)
        {
            Color newColor = m_OriginalColors[i];
            newColor.a = _visible ? m_OriginalColors[i].a : 0f;
            m_SpriteRenderers[i].color = newColor;
        }
    }
    #endregion
} 