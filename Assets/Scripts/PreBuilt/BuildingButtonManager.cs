using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuildingButtonManager : MonoBehaviour
{
    // Singleton instance
    public static BuildingButtonManager Instance { get; private set; }

    [System.Serializable]
    public class BuildingButtonPair
    {
        public string buildingId;
        public GameObject buttonUI;
    }

    // Array to be configured in inspector, mapping buildings to their buttons
    public BuildingButtonPair[] buildingButtons;
    
    // Dictionary for quick lookups during runtime
    private Dictionary<string, GameObject> buttonMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeButtonMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeButtonMap()
    {
        buttonMap = new Dictionary<string, GameObject>();
        foreach (var pair in buildingButtons)
        {
            if (pair.buttonUI != null)
            {
                // Ensure each button has required components
                var canvasGroup = EnsureButtonComponents(pair.buttonUI);
                buttonMap[pair.buildingId] = pair.buttonUI;
                
                // Initialize in hidden state
                pair.buttonUI.SetActive(true);  // First activate to ensure CanvasGroup works
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                
                Debug.Log($"Registered button for building: {pair.buildingId}");
            }
            else
            {
                Debug.LogError($"Missing button UI reference for building: {pair.buildingId}");
            }
        }
    }

    private CanvasGroup EnsureButtonComponents(GameObject buttonObject)
    {
        // Add CanvasGroup if it doesn't exist
        if (!buttonObject.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = buttonObject.AddComponent<CanvasGroup>();
            Debug.Log($"Added missing CanvasGroup to button: {buttonObject.name}");
        }
        return canvasGroup;
    }

    public void ShowButton(string buildingId)
    {
        if (buttonMap.TryGetValue(buildingId, out GameObject button))
        {
            Debug.Log($"Showing button for building: {buildingId}");
            
            // First, ensure the GameObject is active
            button.SetActive(true);
            
            // Get or add CanvasGroup
            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = button.AddComponent<CanvasGroup>();
            }

            // Reset all CanvasGroup properties to ensure visibility
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Force immediate canvas update
            Canvas.ForceUpdateCanvases();
            
            // If the button has a parent with RectTransform, force layout rebuild
            if (button.transform.parent != null && button.transform.parent.TryGetComponent<RectTransform>(out var parentRect))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            // Additional check to ensure all parent canvases are enabled
            var parentCanvases = button.GetComponentsInParent<Canvas>();
            foreach (var canvas in parentCanvases)
            {
                canvas.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError($"No button found for building: {buildingId}");
        }
    }

    public void HideButton(string buildingId)
    {
        if (buttonMap.TryGetValue(buildingId, out GameObject button))
        {
            button.SetActive(false);
            Debug.Log($"Hiding button for building: {buildingId}");
        }
    }

    public void HideAllButtons()
    {
        foreach (var button in buttonMap.Values)
        {
            button.SetActive(false);
        }
        Debug.Log("All buttons hidden");
    }

    /// <summary>
    /// Handles building click events from BuildingClickHandler
    /// </summary>
    /// <param name="_buildingId">The ID of the building that was clicked</param>
    public void OnBuildingClicked(string _buildingId)
    {
        Debug.Log($"Building clicked: {_buildingId}");
        ShowButton(_buildingId);
    }

    /// <summary>
    /// Checks if a building has a registered button
    /// </summary>
    /// <param name="_buildingId">The building ID to check</param>
    /// <returns>True if the building has a registered button</returns>
    public bool HasButtonForBuilding(string _buildingId)
    {
        return buttonMap.ContainsKey(_buildingId);
    }

    /// <summary>
    /// Gets all registered building IDs
    /// </summary>
    /// <returns>Array of all registered building IDs</returns>
    public string[] GetAllBuildingIds()
    {
        string[] ids = new string[buttonMap.Count];
        buttonMap.Keys.CopyTo(ids, 0);
        return ids;
    }
} 