using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonColorFade : MonoBehaviour
{
    #region Private Fields
    [SerializeField, Range(0.1f, 5f)] private float m_FadeDuration = 1f;
    [SerializeField] private bool m_StartWithHighlightedColor;
    
    private Button m_Button;
    private ColorBlock m_Colors;
    private Color m_CurrentColor;
    private bool m_IsFadingToHighlighted;
    private Coroutine m_FadeCoroutine;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        m_Button = GetComponent<Button>();
        m_Colors = m_Button.colors;
        m_CurrentColor = m_StartWithHighlightedColor ? m_Colors.highlightedColor : m_Colors.normalColor;
        m_IsFadingToHighlighted = !m_StartWithHighlightedColor;
        
        // Set initial color
        UpdateButtonColor(m_CurrentColor);
    }

    private void OnEnable()
    {
        StartFadeCoroutine();
    }

    private void OnDisable()
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
        }
    }
    #endregion

    #region Private Methods
    private void StartFadeCoroutine()
    {
        if (m_FadeCoroutine != null)
        {
            StopCoroutine(m_FadeCoroutine);
        }
        m_FadeCoroutine = StartCoroutine(FadeColor());
    }

    private IEnumerator FadeColor()
    {
        while (true)
        {
            Color startColor = m_CurrentColor;
            Color targetColor = m_IsFadingToHighlighted ? m_Colors.highlightedColor : m_Colors.normalColor;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < m_FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / m_FadeDuration;
                
                m_CurrentColor = Color.Lerp(startColor, targetColor, t);
                UpdateButtonColor(m_CurrentColor);
                
                yield return null;
            }
            
            m_IsFadingToHighlighted = !m_IsFadingToHighlighted;
            yield return null;
        }
    }

    private void UpdateButtonColor(Color _color)
    {
        ColorBlock colorBlock = m_Button.colors;
        colorBlock.normalColor = _color;
        colorBlock.selectedColor = _color;
        colorBlock.pressedColor = _color;
        colorBlock.disabledColor = _color;
        m_Button.colors = colorBlock;
    }
    #endregion
}
