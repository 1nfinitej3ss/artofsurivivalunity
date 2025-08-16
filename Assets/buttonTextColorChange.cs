using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // Add this for pointer interfaces

public class ButtonTextColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Private Fields
    [SerializeField] private TextMeshProUGUI m_ButtonText;
    [SerializeField] private Color m_HoverTextColor = new Color(0.196f, 0.196f, 0.196f); // 323232 in hex
    
    private Color m_DefaultTextColor;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Try to get the TextMeshProUGUI component if not assigned
        if (m_ButtonText == null)
        {
            TryGetComponent(out m_ButtonText);
        }

        // Store the default color set in inspector
        if (m_ButtonText != null)
        {
            m_DefaultTextColor = m_ButtonText.color;
        }
    }
    #endregion

    #region Interface Implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_ButtonText != null)
        {
            m_ButtonText.color = m_HoverTextColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_ButtonText != null)
        {
            m_ButtonText.color = m_DefaultTextColor;
        }
    }
    #endregion
}
