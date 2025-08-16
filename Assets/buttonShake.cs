using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class buttonShake : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeSpeed = 50f;
    [SerializeField] private float scaleAmount = 1.2f;
    [SerializeField] private float animationInterval = 5f;
    [SerializeField] private bool usePulseInsteadOfShake = true;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 10f;
    [SerializeField] private bool changeColorOnHover = true;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isAnimating = false;
    private bool isHovered = false;
    private UnityEngine.UI.Image buttonImage;
    private Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        buttonImage = GetComponent<UnityEngine.UI.Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        StartCoroutine(AnimationLoop());
    }

    IEnumerator AnimationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(animationInterval);
            
            if (!isAnimating)
            {
                if (usePulseInsteadOfShake)
                    StartCoroutine(PulseAnimation());
                else
                    StartCoroutine(ShakeAndScaleAnimation());
            }
        }
    }

    IEnumerator PulseAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        float duration = 1.2f; // Slightly longer duration for smoother feel
        Vector3 startScale = transform.localScale;
        Vector3 baseScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth easing using SmoothStep
            float smoothT = Mathf.SmoothStep(0, 1, t);
            // Gentler sine wave
            float pulse = Mathf.Sin(smoothT * Mathf.PI * 2) * 0.15f;
            
            transform.localScale = baseScale * (1f + pulse);
            yield return null;
        }

        transform.localScale = baseScale;
        isAnimating = false;
    }

    IEnumerator ShakeAndScaleAnimation()
    {
        isAnimating = true;
        float elapsed = 0f;
        float duration = 0.8f;
        Vector3 baseScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Smooth easing
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Decreasing shake intensity
            float shakeIntensity = shakeAmount * (1f - smoothT);
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            transform.localPosition = originalPosition + randomOffset;
            
            // Gentler scale animation
            float scaleWave = Mathf.Sin(t * Mathf.PI * 3) * 0.1f;
            transform.localScale = baseScale * (1f + scaleWave);
            
            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localScale = baseScale;
        isAnimating = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleHoverEffect();
    }

    void HandleHoverEffect()
    {
        if (!isAnimating)  // Only handle hover scaling when not animating
        {
            Vector3 targetScale = isHovered ? 
                originalScale * hoverScaleMultiplier : 
                originalScale;

            // Smoothly interpolate to target scale
            transform.localScale = Vector3.Lerp(
                transform.localScale, 
                targetScale, 
                Time.deltaTime * hoverTransitionSpeed
            );
        }

        // Handle color transition if enabled (keep this separate from scale)
        if (changeColorOnHover && buttonImage != null)
        {
            buttonImage.color = Color.Lerp(
                buttonImage.color,
                isHovered ? hoverColor : originalColor,
                Time.deltaTime * hoverTransitionSpeed
            );
        }
    }

    // These functions are called automatically by Unity's Event System
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
