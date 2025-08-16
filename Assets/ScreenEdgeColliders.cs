using UnityEngine;

public class ScreenEdgeColliders : MonoBehaviour
{
    #region Private Fields
    [SerializeField, Range(0.1f, 2f)] 
    private float m_ColliderThickness = 0.5f;
    
    [SerializeField] 
    private bool m_UseTriggers = false;
    
    private BoxCollider2D m_TopCollider;
    private BoxCollider2D m_BottomCollider;
    private BoxCollider2D m_LeftCollider;
    private BoxCollider2D m_RightCollider;
    private Camera m_MainCamera;
    private int m_PreviousWidth;
    private int m_PreviousHeight;
    private float m_PreviousOrthoSize;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (!TryGetMainCamera())
        {
            Debug.LogError("ScreenEdgeColliders: Main camera not found!");
            enabled = false;
            return;
        }

        InitializeColliders();
        UpdateColliderPositions();
    }

    private void Update()
    {
        // Only update if screen dimensions or camera orthographic size changed
        if (Screen.width != m_PreviousWidth || 
            Screen.height != m_PreviousHeight ||
            !Mathf.Approximately(m_MainCamera.orthographicSize, m_PreviousOrthoSize))
        {
            UpdateColliderPositions();
            CacheCurrentDimensions();
        }
    }
    #endregion

    #region Private Methods
    private bool TryGetMainCamera()
    {
        m_MainCamera = Camera.main;
        return m_MainCamera != null;
    }

    private void InitializeColliders()
    {
        m_TopCollider = CreateEdgeCollider("TopEdge");
        m_BottomCollider = CreateEdgeCollider("BottomEdge");
        m_LeftCollider = CreateEdgeCollider("LeftEdge");
        m_RightCollider = CreateEdgeCollider("RightEdge");
    }

    private BoxCollider2D CreateEdgeCollider(string _name)
    {
        GameObject edge = new GameObject(_name);
        edge.transform.SetParent(transform, false);
        var collider = edge.AddComponent<BoxCollider2D>();
        collider.isTrigger = m_UseTriggers;
        return collider;
    }

    private void UpdateColliderPositions()
    {
        // Calculate screen dimensions based on camera's orthographic size
        float screenWidth = m_MainCamera.orthographicSize * Screen.width / Screen.height;
        float screenHeight = m_MainCamera.orthographicSize;

        // Position colliders at screen edges
        SetColliderTransform(m_TopCollider, 
            new Vector2(screenWidth * 2, m_ColliderThickness),
            new Vector2(0, screenHeight + m_ColliderThickness / 2));

        SetColliderTransform(m_BottomCollider,
            new Vector2(screenWidth * 2, m_ColliderThickness),
            new Vector2(0, -screenHeight - m_ColliderThickness / 2));

        SetColliderTransform(m_LeftCollider,
            new Vector2(m_ColliderThickness, screenHeight * 2),
            new Vector2(-screenWidth - m_ColliderThickness / 2, 0));

        SetColliderTransform(m_RightCollider,
            new Vector2(m_ColliderThickness, screenHeight * 2),
            new Vector2(screenWidth + m_ColliderThickness / 2, 0));
    }

    private void SetColliderTransform(BoxCollider2D _collider, Vector2 _size, Vector2 _offset)
    {
        _collider.size = _size;
        _collider.offset = _offset;
    }

    private void CacheCurrentDimensions()
    {
        m_PreviousWidth = Screen.width;
        m_PreviousHeight = Screen.height;
        m_PreviousOrthoSize = m_MainCamera.orthographicSize;
    }
    #endregion

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && m_TopCollider != null)
        {
            UpdateColliderPositions();
        }
    }
    #endif
}
