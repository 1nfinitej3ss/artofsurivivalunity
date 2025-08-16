using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LocationToggleManager : MonoBehaviour
{
    public static LocationToggleManager Instance { get; private set; }
    private Dictionary<string, bool> visitedStates = new Dictionary<string, bool>();
    private Dictionary<string, Toggle> toggleDict = new Dictionary<string, Toggle>();
    
    [Header("Location Toggles")]
    public Toggle homeToggle;
    public Toggle studioToggle;
    public Toggle galleryToggle;
    public Toggle socialToggle;
    public Toggle workToggle;

    private readonly string[] toggleKeys = { "HomeVisited", "StudioVisited", "GalleryVisited", "SocialVisited", "WorkVisited" };

    private bool isCurrentRoundComplete = false;

    void Awake()
    {
        Debug.Log("[LocationToggle] Awake called");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[LocationToggle] Set as DontDestroyOnLoad instance");
            InitializeVisitedStates();
        }
        else
        {
            Debug.Log("[LocationToggle] Destroying duplicate instance");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[LocationToggle] Starting initialization");
        
        // Initialize the dictionary
        toggleDict = new Dictionary<string, Toggle>
        {
            { "HomeVisited", homeToggle },
            { "StudioVisited", studioToggle },
            { "GalleryVisited", galleryToggle },
            { "SocialVisited", socialToggle },
            { "WorkVisited", workToggle }
        };

        // Verify all toggles are assigned
        foreach (var pair in toggleDict)
        {
            if (pair.Value == null)
            {
                Debug.LogError($"[LocationToggle] Toggle for {pair.Key} is not assigned in the inspector!");
                // Try to find the toggle in the scene
                var toggle = GameObject.Find(pair.Key)?.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggleDict[pair.Key] = toggle;
                    Debug.Log($"[LocationToggle] Found and assigned {pair.Key} toggle in scene");
                }
            }
        }

        // Load the toggle states
        LoadToggleStates();
    }

    private void InitializeVisitedStates()
    {
        LoadVisitedStates();
        // Initialize any missing states
        foreach (var key in toggleKeys)
        {
            if (!visitedStates.ContainsKey(key))
            {
                visitedStates[key] = false;
            }
        }
    }

    public void VisitLocation(string locationKey)
    {
        Debug.Log($"[LocationToggle] Attempting to visit location {locationKey}");
        
        if (toggleDict.ContainsKey(locationKey))
        {
            if (isCurrentRoundComplete)
            {
                Debug.Log("[LocationToggle] Current round is complete, starting new round");
                return;
            }

            toggleDict[locationKey].isOn = true;
            visitedStates[locationKey] = true;
            SaveVisitedStates(); // Save after updating state
            Debug.Log($"[LocationToggle] Successfully marked {locationKey} as visited");

            if (AllLocationsVisited())
            {
                Debug.Log("[LocationToggle] All locations visited - Round complete!");
                isCurrentRoundComplete = true;
                SaveVisitedStates();
            }
        }
        else
        {
            Debug.LogError($"[LocationToggle] Invalid location key {locationKey}");
        }
    }

    private void LoadToggleStates()
    {
        Debug.Log("[LocationToggle] Loading toggle UI states");
        foreach (var key in toggleKeys)
        {
            if (toggleDict.ContainsKey(key) && toggleDict[key] != null)
            {
                bool state = visitedStates[key];
                toggleDict[key].isOn = state;
                Debug.Log($"[LocationToggle] Set toggle UI {key} = {state}");
            }
            else
            {
                Debug.LogError($"[LocationToggle] Toggle {key} is missing from dictionary or null!");
                // Try to find the toggle again
                var toggle = FindObjectsOfType<Toggle>()
                    .FirstOrDefault(t => t.name == key);
                
                if (toggle != null)
                {
                    toggleDict[key] = toggle;
                    bool state = visitedStates[key];
                    toggle.isOn = state;
                    Debug.Log($"[LocationToggle] Found and set {key} toggle in scene = {state}");
                }
                else
                {
                    Debug.LogError($"[LocationToggle] Could not find toggle for {key} anywhere in scene");
                }
            }
        }
        
        isCurrentRoundComplete = AllLocationsVisited();
    }

    public bool AllLocationsVisited()
    {
        foreach (var state in visitedStates)
        {
            if (!state.Value)
            {
                return false;
            }
        }
        return true;
    }

    public void StartNewRound()
    {
        Debug.Log("[LocationToggle] Starting new round");
        isCurrentRoundComplete = false;  // Reset this first
        
        foreach (var key in toggleKeys)
        {
            if (toggleDict.ContainsKey(key) && toggleDict[key] != null)
            {
                toggleDict[key].isOn = false;
                visitedStates[key] = false;
                Debug.Log($"[LocationToggle] Reset {key}");
            }
        }
        
        SaveVisitedStates(); // Save the reset state
        Debug.Log("[LocationToggle] New round started - all states reset");
    }

    public bool IsCurrentRoundComplete()
    {
        return isCurrentRoundComplete;
    }

    // Add a public method to check if we can find the instance
    public static bool IsAvailable()
    {
        bool available = Instance != null;
        Debug.Log($"LocationToggleManager: Instance availability check - {available}");
        return available;
    }

    // Add these methods to persist states between scene loads
    private void SaveVisitedStates()
    {
        Debug.Log("[LocationToggle] Saving visited states to PlayerPrefs");
        foreach (var key in toggleKeys)
        {
            int value = visitedStates[key] ? 1 : 0;
            PlayerPrefs.SetInt(key, value);
            Debug.Log($"[LocationToggle] Saved {key} = {value} to PlayerPrefs");
        }
        PlayerPrefs.SetInt("IsRoundComplete", isCurrentRoundComplete ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("[LocationToggle] States saved to PlayerPrefs");
    }

    private void LoadVisitedStates()
    {
        Debug.Log("[LocationToggle] Loading visited states from PlayerPrefs");
        foreach (var key in toggleKeys)
        {
            bool wasVisited = PlayerPrefs.GetInt(key, 0) == 1;
            visitedStates[key] = wasVisited;
            Debug.Log($"[LocationToggle] Loaded {key} = {wasVisited} from PlayerPrefs");
        }
        isCurrentRoundComplete = PlayerPrefs.GetInt("IsRoundComplete", 0) == 1;
        Debug.Log($"[LocationToggle] Loaded round complete status = {isCurrentRoundComplete}");
    }

    // Add OnDestroy to save state when the object is destroyed
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveVisitedStates();
        }
    }

    // Add this method for debugging
    public void DebugStates()
    {
        Debug.Log("[LocationToggle] === Debug States ===");
        foreach (var key in toggleKeys)
        {
            Debug.Log($"[LocationToggle] {key}: PlayerPrefs={PlayerPrefs.GetInt(key, 0)}, visitedStates={visitedStates[key]}, toggle={toggleDict[key].isOn}");
        }
        Debug.Log($"[LocationToggle] Round Complete: {isCurrentRoundComplete}");
        Debug.Log("[LocationToggle] ===================");
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we're loading the main scene
        if (scene.name == "main")
        {
            Debug.Log($"[LocationToggle] Main scene loaded, current scene name: {scene.name}");
            
            // First check if PlayerState exists and if any attributes are below 0
            if (PlayerState.Instance != null && PlayerState.Instance.IsGameOverPending())
            {
                Debug.Log("[LocationToggle] Detected game over condition, redirecting to game over scene");
                // Add a small delay before triggering game over to ensure scene is fully loaded
                StartCoroutine(DelayedGameOver());
                return;
            }

            Debug.Log("[LocationToggle] Main scene loaded, reinitializing toggles");
            
            // Reinitialize toggle dictionary
            InitializeToggleDictionary();
            
            // First load the current states
            LoadToggleStates();
            
            // Debug current states
            Debug.Log("[LocationToggle] Current states:");
            foreach (var state in visitedStates)
            {
                Debug.Log($"[LocationToggle] - {state.Key}: {state.Value}");
            }
            
            // Check if all locations are visited
            bool allVisited = AllLocationsVisited();
            Debug.Log($"[LocationToggle] All locations visited: {allVisited}");
            
            if (allVisited)
            {
                Debug.Log("[LocationToggle] All locations visited, starting new round");
                isCurrentRoundComplete = true;
                StartNewRound();
            }
        }
    }

    private IEnumerator DelayedGameOver()
    {
        // Wait for a short delay to ensure scene is fully loaded
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("GameOver");
    }

    private void InitializeToggleDictionary()
    {
        Debug.Log("[LocationToggle] Initializing toggle dictionary");
        
        // Clear existing dictionary
        toggleDict.Clear();
        
        // Try to find toggles in the scene
        Toggle[] allToggles = FindObjectsOfType<Toggle>();
        Debug.Log($"[LocationToggle] Found {allToggles.Length} toggles in scene");
        
        foreach (var key in toggleKeys)
        {
            // First try to find by name
            var toggle = allToggles.FirstOrDefault(t => t.name == key);
            
            if (toggle == null)
            {
                // Try to find in children of the manager
                toggle = GetComponentsInChildren<Toggle>(true)
                    .FirstOrDefault(t => t.name == key);
            }
            
            if (toggle != null)
            {
                toggleDict[key] = toggle;
                Debug.Log($"[LocationToggle] Found and assigned {key} toggle: {toggle.name}");
            }
            else
            {
                Debug.LogError($"[LocationToggle] Could not find toggle for {key} in scene");
            }
        }
    }
}
