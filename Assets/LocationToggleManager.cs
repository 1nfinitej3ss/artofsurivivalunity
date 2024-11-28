using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationToggleManager : MonoBehaviour
{
    // Dictionary to manage toggles and their corresponding PlayerPrefs keys
    private Dictionary<string, Toggle> toggleDict;

    [Header("Location Toggles")]
    public Toggle homeToggle;
    public Toggle studioToggle;
    public Toggle galleryToggle;
    public Toggle socialToggle;
    public Toggle workToggle;

    // List of toggle keys
    private readonly string[] toggleKeys = { "HomeVisited", "StudioVisited", "GalleryVisited", "SocialVisited", "WorkVisited" };

    void Start()
    {
        // Initialize the dictionary
        toggleDict = new Dictionary<string, Toggle>
        {
            { "HomeVisited", homeToggle },
            { "StudioVisited", studioToggle },
            { "GalleryVisited", galleryToggle },
            { "SocialVisited", socialToggle },
            { "WorkVisited", workToggle }
        };

        // Load the toggle states when the scene starts
        LoadToggleStates();
    }

    // Method to mark a location as visited
    public void VisitLocation(string locationKey)
    {
        if (toggleDict.ContainsKey(locationKey))
        {
            // Mark the location as visited
            toggleDict[locationKey].isOn = true;
            PlayerPrefs.SetInt(locationKey, 1);  // Save to PlayerPrefs
            PlayerPrefs.Save();

            // After marking the location, check if all locations are visited
            if (AllLocationsVisited())
            {
                Debug.Log("All locations visited. Resetting toggles.");
                ResetToggles();  // Call ResetToggles if all locations are visited
            }
        }
    }

    // Method to check if all locations have been visited
    public bool AllLocationsVisited()
    {
        foreach (var key in toggleKeys)
        {
            // If any location is not visited, return false
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                return false;
            }
        }
        // If all locations are visited, return true
        return true;
    }

    // Method to load all toggle states from PlayerPrefs
    void LoadToggleStates()
    {
        foreach (var key in toggleKeys)
        {
            // Load each toggle state from PlayerPrefs
            bool isVisited = PlayerPrefs.GetInt(key, 0) == 1;
            if (toggleDict.ContainsKey(key))
            {
                toggleDict[key].isOn = isVisited;
            }
        }
    }

    // Method to reset all toggles
    public void ResetToggles()
    {
        foreach (var key in toggleKeys)
        {
            if (toggleDict.ContainsKey(key))
            {
                toggleDict[key].isOn = false;
                PlayerPrefs.SetInt(key, 0);  // Reset in PlayerPrefs
            }
        }
        PlayerPrefs.Save();
    }
}
