using UnityEngine;
using System.Collections;

public class CursorManager : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private Texture2D m_DefaultCursor;  // First hoof position
    [SerializeField] private Texture2D m_ClickableCursor;  // Second hoof position
    [SerializeField] private Vector2 m_DefaultHotSpot = new Vector2(0, 0);
    [SerializeField] private Vector2 m_ClickableHotSpot = new Vector2(0, 0);
    [SerializeField] private float m_CursorAnimationSpeed = 0.15f;
    [SerializeField] private float m_MovementThreshold = 0.1f;  // How much movement is needed to trigger animation

    private Vector3 m_LastMousePosition;
    private bool m_IsAnimating;
    private Coroutine m_AnimationCoroutine;
    private bool m_IsFirstCursor = true;
    private float m_TimeSinceLastMove;
    #endregion

    #region Singleton Pattern
    public static CursorManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Add this debug section at the start of the Start method
        Debug.Log($"Default Cursor null? {m_DefaultCursor == null}");
        if (m_DefaultCursor != null)
        {
            Debug.Log($"Default Cursor readable? {m_DefaultCursor.isReadable}");
            Debug.Log($"Default Cursor format: {m_DefaultCursor.format}");
        }
        
        Debug.Log($"Clickable Cursor null? {m_ClickableCursor == null}");
        if (m_ClickableCursor != null)
        {
            Debug.Log($"Clickable Cursor readable? {m_ClickableCursor.isReadable}");
            Debug.Log($"Clickable Cursor format: {m_ClickableCursor.format}");
        }

        // Ensure cursor is visible and not locked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Validate cursor textures
        if (m_DefaultCursor == null || m_ClickableCursor == null)
        {
            Debug.LogError("Cursor textures not assigned! Check CursorManager in inspector.");
            ResetToSystemCursor();
            return;
        }

        if (!m_DefaultCursor.isReadable || !m_ClickableCursor.isReadable)
        {
            Debug.LogError("Cursor textures must have Read/Write enabled in import settings!");
            ResetToSystemCursor();
            return;
        }
        
        SetDefaultCursor();
        m_LastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        float distance = Vector3.Distance(currentMousePosition, m_LastMousePosition);
        
        // If mouse has moved more than threshold
        if (distance > m_MovementThreshold)
        {
            m_TimeSinceLastMove = 0f;
            if (!m_IsAnimating)
            {
                StartCursorAnimation();
            }
        }
        else
        {
            m_TimeSinceLastMove += Time.deltaTime;
            // Stop animation after 0.1 seconds of no movement
            if (m_TimeSinceLastMove > 0.1f && m_IsAnimating)
            {
                StopCursorAnimation();
            }
        }

        m_LastMousePosition = currentMousePosition;
    }
    #endregion

    #region Private Methods
    private void StartCursorAnimation()
    {
        if (m_AnimationCoroutine != null)
        {
            StopCoroutine(m_AnimationCoroutine);
        }
        
        m_IsAnimating = true;
        m_AnimationCoroutine = StartCoroutine(AnimateCursor());
    }

    private void StopCursorAnimation()
    {
        if (m_AnimationCoroutine != null)
        {
            StopCoroutine(m_AnimationCoroutine);
            m_AnimationCoroutine = null;
        }
        
        m_IsAnimating = false;
        SetDefaultCursor();
    }

    private IEnumerator AnimateCursor()
    {
        while (m_IsAnimating)
        {
            if (m_IsFirstCursor)
            {
                Cursor.SetCursor(m_DefaultCursor, m_DefaultHotSpot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(m_ClickableCursor, m_ClickableHotSpot, CursorMode.Auto);
            }

            m_IsFirstCursor = !m_IsFirstCursor;
            yield return new WaitForSeconds(m_CursorAnimationSpeed);
        }
    }

    public void SetDefaultCursor()
    {
        if (m_DefaultCursor == null)
        {
            Debug.LogWarning("Default cursor texture is null! Falling back to system cursor.");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        try
        {
            Cursor.SetCursor(m_DefaultCursor, m_DefaultHotSpot, CursorMode.Auto);
            m_IsFirstCursor = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set cursor texture: {e.Message}");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void SetClickableCursor()
    {
        if (m_ClickableCursor == null)
        {
            Debug.LogWarning("Clickable cursor texture is null! Falling back to system cursor.");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            return;
        }

        try
        {
            Cursor.SetCursor(m_ClickableCursor, m_ClickableHotSpot, CursorMode.Auto);
            m_IsFirstCursor = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set clickable cursor texture: {e.Message}");
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    #endregion

    #region Public Methods
    public void ResetToSystemCursor()
    {
        StopCursorAnimation();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion
} 