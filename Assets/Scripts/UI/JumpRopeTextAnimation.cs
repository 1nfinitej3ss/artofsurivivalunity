using UnityEngine;
using TMPro;
using System.Collections;

public class JumpRopeTextAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float amplitude = 10f; // Height of the bounce
    [SerializeField] private float frequency = 2f; // Speed of the animation
    [SerializeField] private float characterDelay = 0.1f; // Delay between each character's animation
    
    [Header("Text Settings")]
    [SerializeField] private string text = "Your Text Here";
    
    private TextMeshProUGUI textComponent;
    private Vector3[] originalCharacterPositions;
    private float startTime;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            Debug.LogError("JumpRopeTextAnimation requires a TextMeshProUGUI component!");
            enabled = false;
            return;
        }
        
        SetupText();
    }

    private void Start()
    {
        Debug.Log($"Canvas active: {transform.root.gameObject.activeSelf}");
        Debug.Log($"Text object active: {gameObject.activeSelf}");
        Debug.Log($"Text content: {textComponent.text}");
        Debug.Log($"Text color: {textComponent.color}");
        Debug.Log($"Text position: {GetComponent<RectTransform>().position}");
        
        // Set the text first
        textComponent.text = text;
        
        // Force the text to update its geometry
        textComponent.ForceMeshUpdate();
        
        // Verify text info
        if (textComponent.textInfo.characterCount == 0)
        {
            Debug.LogError("No characters found in text!");
            return;
        }
        
        Debug.Log($"Character count: {textComponent.textInfo.characterCount}");
        
        // Store the original positions of each character
        originalCharacterPositions = new Vector3[textComponent.textInfo.characterCount];
        for (int i = 0; i < textComponent.textInfo.characterCount; i++)
        {
            if (!textComponent.textInfo.characterInfo[i].isVisible) continue;
            int materialIndex = textComponent.textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textComponent.textInfo.characterInfo[i].vertexIndex;
            originalCharacterPositions[i] = (
                textComponent.textInfo.meshInfo[materialIndex].vertices[vertexIndex] +
                textComponent.textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] +
                textComponent.textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] +
                textComponent.textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3]
            ) / 4f;
        }

        startTime = Time.time;
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        while (true)
        {
            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                if (!textComponent.textInfo.characterInfo[i].isVisible) continue;

                int materialIndex = textComponent.textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textComponent.textInfo.characterInfo[i].vertexIndex;

                // Calculate the vertical offset for this character
                float timeOffset = Time.time - startTime - (i * characterDelay);
                float yOffset = amplitude * Mathf.Sin(timeOffset * frequency * Mathf.PI);

                // Get the vertices for this character
                Vector3[] vertices = textComponent.textInfo.meshInfo[materialIndex].vertices;

                // Update each vertex of the character
                for (int j = 0; j < 4; j++)
                {
                    vertices[vertexIndex + j] = originalCharacterPositions[i] + new Vector3(0, yOffset, 0);
                }
            }

            // Update the mesh with new vertex positions
            textComponent.UpdateVertexData();
            
            yield return null;
        }
    }

    private void OnValidate()
    {
        // Ensure reasonable values
        amplitude = Mathf.Max(0, amplitude);
        frequency = Mathf.Max(0.1f, frequency);
        characterDelay = Mathf.Max(0, characterDelay);
    }

    private void SetupText()
    {
        if (string.IsNullOrEmpty(textComponent.text))
        {
            textComponent.text = "Test Text";
        }
        
        // Ensure text is visible and large enough
        textComponent.color = Color.white;
        textComponent.fontSize = 72f; // Made larger for testing
        
        // Center the text and ensure it's visible
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(800f, 200f); // Made larger for testing
            
            // Set anchors to center
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        
        // Ensure the text is rendered on top
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 999;
        }
        
        // Set text alignment
        textComponent.alignment = TextAlignmentOptions.Center;
        
        Debug.Log($"Text setup complete. Text: {textComponent.text}, Color: {textComponent.color}, Size: {textComponent.fontSize}");
        Debug.Log($"RectTransform anchored position: {rectTransform.anchoredPosition}");
        Debug.Log($"Canvas sorting order: {(canvas != null ? canvas.sortingOrder.ToString() : "No Canvas")}");
    }
} 