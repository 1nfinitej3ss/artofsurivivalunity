using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Serialized Fields
    [SerializeField] private string m_BuildingId;
    [SerializeField] private bool m_EnableClickInteraction = true;
    [SerializeField] private bool m_EnableHoverEffects = true;
    #endregion

    #region Private Fields
    private SpriteRenderer m_SpriteRenderer;
    private Color m_OriginalColor;
    private bool m_IsHovered = false;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        // Ensure we have a Collider2D for click detection
        EnsureColliderExists();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        // Get or add SpriteRenderer for visual feedback
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        if (m_SpriteRenderer != null)
        {
            m_OriginalColor = m_SpriteRenderer.color;
        }

        // Ensure we have a Collider2D for click detection
        EnsureColliderExists();
    }

    private void EnsureColliderExists()
    {
        // Check if we have a Collider2D component
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // Add a BoxCollider2D if none exists
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            
            // If we have a SpriteRenderer, size the collider to match the sprite
            if (m_SpriteRenderer != null && m_SpriteRenderer.sprite != null)
            {
                boxCollider.size = m_SpriteRenderer.sprite.bounds.size;
                boxCollider.offset = m_SpriteRenderer.sprite.bounds.center;
            }
            
            Debug.Log($"Added BoxCollider2D to {gameObject.name} for click detection");
        }
    }
    #endregion

    #region Pointer Event Handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!m_EnableClickInteraction) return;

        Debug.Log($"Building clicked: {m_BuildingId}");
        
        // Show the corresponding UI button using the dedicated method
        if (BuildingButtonManager.Instance != null)
        {
            BuildingButtonManager.Instance.OnBuildingClicked(m_BuildingId);
        }
        else
        {
            Debug.LogError("BuildingButtonManager.Instance is null! Make sure it exists in the scene.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!m_EnableHoverEffects) return;

        m_IsHovered = true;
        
        // Visual feedback on hover
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = new Color(1.2f, 1.2f, 1.2f, 1f); // Brighten the sprite
        }

        // Change cursor to indicate clickable
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetClickableCursor();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!m_EnableHoverEffects) return;

        m_IsHovered = false;
        
        // Restore original color
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.color = m_OriginalColor;
        }

        // Restore default cursor
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetDefaultCursor();
        }
    }
    #endregion

    #region Public Methods
    public void SetBuildingId(string _buildingId)
    {
        m_BuildingId = _buildingId;
    }

    public string GetBuildingId()
    {
        return m_BuildingId;
    }

    public void SetClickInteractionEnabled(bool _enabled)
    {
        m_EnableClickInteraction = _enabled;
    }

    public void SetHoverEffectsEnabled(bool _enabled)
    {
        m_EnableHoverEffects = _enabled;
    }
    #endregion

    #region Editor Methods
    #if UNITY_EDITOR
    [ContextMenu("Debug Building Info")]
    private void DebugBuildingInfo()
    {
        Debug.Log($"Building: {gameObject.name}, ID: {m_BuildingId}, Click Enabled: {m_EnableClickInteraction}");
    }
    #endif
    #endregion
} 