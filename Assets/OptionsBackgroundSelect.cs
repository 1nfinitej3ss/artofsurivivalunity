using UnityEngine;
using UnityEngine.UI;

public class OptionsBackgroundSelect : MonoBehaviour
{
    [SerializeField] private Sprite[] backgroundImages; // Array to store background images
    [SerializeField] private Image imageComponent; // Reference to the Image component
    
    [Tooltip("If left empty, will try to find Image component on this GameObject")]
    [SerializeField] private GameObject imageTargetObject; // Optional: GameObject that has the Image component

    private void Awake()
    {
        // First try to use the explicitly assigned image component
        if (imageComponent == null)
        {
            // If imageTargetObject is assigned, try to get Image from there
            if (imageTargetObject != null)
            {
                imageComponent = imageTargetObject.GetComponent<Image>();
            }
            
            // If still null, try to get it from this GameObject
            if (imageComponent == null)
            {
                imageComponent = GetComponent<Image>();
            }
        }

        if (imageComponent == null)
        {
            Debug.LogError("No Image component found! Please assign an Image component in the inspector.", this);
        }
    }

    private void Start()
    {
        SetMatchingBackground();
    }

    private void SetMatchingBackground()
    {
        if (backgroundImages == null || backgroundImages.Length == 0)
        {
            Debug.LogWarning("No background images assigned to OptionsBackgroundSelect script!");
            return;
        }

        if (imageComponent == null)
        {
            Debug.LogError("No Image component assigned!", this);
            return;
        }

        int index = questionVideoSelect.LastSelectedIndex;
        if (index >= 0 && index < backgroundImages.Length)
        {
            imageComponent.sprite = backgroundImages[index];
        }
        else
        {
            Debug.LogWarning("No image has been selected yet or invalid index!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
