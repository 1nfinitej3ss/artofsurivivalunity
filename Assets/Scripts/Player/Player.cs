using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Player : MonoBehaviour
{
    #region Private Fields
    private static readonly string[] m_ScenesToDestroyOn = { "GameOver", "GameVictory" };
    private static readonly string[] m_ScenesToHideOn = { "gallery", "home", "work" };
    private SpriteRenderer m_SpriteRenderer;
    private CanvasGroup m_CanvasGroup;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);

        // Get either SpriteRenderer or CanvasGroup
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_CanvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Private Methods
    private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (m_ScenesToDestroyOn.Contains(_scene.name))
        {
            Destroy(gameObject);
            return;
        }

        // Set visibility based on scene
        bool shouldBeVisible = _scene.name.ToLower() == "main";
        SetPlayerVisibility(shouldBeVisible);
    }

    private void SetPlayerVisibility(bool visible)
    {
        if (m_SpriteRenderer != null)
        {
            Color color = m_SpriteRenderer.color;
            color.a = visible ? 1f : 0f;
            m_SpriteRenderer.color = color;
        }

        if (m_CanvasGroup != null)
        {
            m_CanvasGroup.alpha = visible ? 1f : 0f;
        }

        // Also disable collider if present
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = visible;
        }
    }
    #endregion
} 