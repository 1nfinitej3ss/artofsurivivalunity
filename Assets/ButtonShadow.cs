using UnityEngine;
using UnityEngine.UI;

public class ButtonShadow : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private float m_ShadowOffset = 8f;
    [SerializeField] private Color m_ShadowColor = new Color(0, 0, 0, 0.25f);
    
    private Image m_ShadowImage;
    private RectTransform m_ButtonTransform;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Get the button's RectTransform
        m_ButtonTransform = GetComponent<RectTransform>();
        
        // Create shadow game object
        GameObject shadowObj = new GameObject("ButtonShadow");
        shadowObj.transform.SetParent(transform);
        shadowObj.transform.SetAsFirstSibling(); // Place shadow behind the button
        
        // Add and setup the shadow image component
        m_ShadowImage = shadowObj.AddComponent<Image>();
        m_ShadowImage.sprite = GetComponent<Image>().sprite;
        m_ShadowImage.color = m_ShadowColor;
        
        // Setup shadow RectTransform
        RectTransform shadowTransform = shadowObj.GetComponent<RectTransform>();
        shadowTransform.anchorMin = Vector2.zero;
        shadowTransform.anchorMax = Vector2.one;
        shadowTransform.sizeDelta = Vector2.zero;
        shadowTransform.anchoredPosition = new Vector2(m_ShadowOffset, -m_ShadowOffset);
    }
    #endregion
}
