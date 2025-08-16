using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class typewriter : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;  // Time between each character
    [SerializeField] private AudioSource typeSound;      // Optional typing sound effect
    
    private TextMeshProUGUI tmpText;    // Reference to TextMeshPro component
    private Text legacyText;            // Reference to Legacy UI Text component
    private string fullText;            // Stores the complete text
    private bool isTyping = false;      // Flag to check if currently typing
    
    void Start()
    {
        // Try to get either TextMeshPro or legacy Text component
        tmpText = GetComponent<TextMeshProUGUI>();
        legacyText = GetComponent<Text>();
        
        if (tmpText != null)
        {
            fullText = tmpText.text;
            tmpText.text = "";
            StartTyping();
        }
        else if (legacyText != null)
        {
            fullText = legacyText.text;
            legacyText.text = "";
            StartTyping();
        }
        else
        {
            Debug.LogError("No text component found on this GameObject!");
        }
    }

    public void StartTyping()
    {
        if (!isTyping)
        {
            StartCoroutine(TypeText());
        }
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        string currentText = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            
            // Update the appropriate text component
            if (tmpText != null)
                tmpText.text = currentText;
            else if (legacyText != null)
                legacyText.text = currentText;

            // Play typing sound if available
            if (typeSound != null)
                typeSound.Play();

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    // Public method to skip the typing animation
    public void SkipTyping()
    {
        StopAllCoroutines();
        if (tmpText != null)
            tmpText.text = fullText;
        else if (legacyText != null)
            legacyText.text = fullText;
        isTyping = false;
    }
}
