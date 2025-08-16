using UnityEngine;
using System.Collections.Generic;

public class BuildingClickSetup : MonoBehaviour
{
    #region Serialized Fields
    [System.Serializable]
    public class BuildingConfig
    {
        public string buildingId;
        public GameObject buildingObject;
        public bool enableClickInteraction = true;
        public bool enableHoverEffects = true;
    }

    [SerializeField] private BuildingConfig[] m_BuildingConfigs;
    [SerializeField] private bool m_AutoSetupOnStart = true;
    [SerializeField] private bool m_RemoveExistingHandlers = false;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (m_AutoSetupOnStart)
        {
            SetupAllBuildings();
        }
    }
    #endregion

    #region Public Methods
    public void SetupAllBuildings()
    {
        if (m_BuildingConfigs == null || m_BuildingConfigs.Length == 0)
        {
            Debug.LogWarning("No building configurations found in BuildingClickSetup!");
            return;
        }

        foreach (var config in m_BuildingConfigs)
        {
            if (config.buildingObject != null)
            {
                SetupBuilding(config);
            }
            else
            {
                Debug.LogError($"Building object is null for building ID: {config.buildingId}");
            }
        }

        Debug.Log($"Setup complete for {m_BuildingConfigs.Length} buildings");
    }

    public void SetupBuilding(BuildingConfig _config)
    {
        if (_config.buildingObject == null)
        {
            Debug.LogError("Building object is null!");
            return;
        }

        // Remove existing handler if requested
        if (m_RemoveExistingHandlers)
        {
            BuildingClickHandler existingHandler = _config.buildingObject.GetComponent<BuildingClickHandler>();
            if (existingHandler != null)
            {
                DestroyImmediate(existingHandler);
            }
        }

        // Add or get the BuildingClickHandler component
        BuildingClickHandler clickHandler = _config.buildingObject.GetComponent<BuildingClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = _config.buildingObject.AddComponent<BuildingClickHandler>();
            Debug.Log($"Added BuildingClickHandler to {_config.buildingObject.name}");
        }

        // Configure the handler
        clickHandler.SetBuildingId(_config.buildingId);
        clickHandler.SetClickInteractionEnabled(_config.enableClickInteraction);
        clickHandler.SetHoverEffectsEnabled(_config.enableHoverEffects);

        Debug.Log($"Configured {_config.buildingObject.name} with ID: {_config.buildingId}");
    }

    public void SetupBuilding(string _buildingId, GameObject _buildingObject)
    {
        BuildingConfig config = new BuildingConfig
        {
            buildingId = _buildingId,
            buildingObject = _buildingObject,
            enableClickInteraction = true,
            enableHoverEffects = true
        };

        SetupBuilding(config);
    }

    public void RemoveAllClickHandlers()
    {
        if (m_BuildingConfigs == null) return;

        foreach (var config in m_BuildingConfigs)
        {
            if (config.buildingObject != null)
            {
                BuildingClickHandler handler = config.buildingObject.GetComponent<BuildingClickHandler>();
                if (handler != null)
                {
                    DestroyImmediate(handler);
                    Debug.Log($"Removed BuildingClickHandler from {config.buildingObject.name}");
                }
            }
        }
    }
    #endregion

    #region Editor Methods
    #if UNITY_EDITOR
    [ContextMenu("Setup All Buildings")]
    private void EditorSetupAllBuildings()
    {
        SetupAllBuildings();
    }

    [ContextMenu("Remove All Click Handlers")]
    private void EditorRemoveAllClickHandlers()
    {
        RemoveAllClickHandlers();
    }

    [ContextMenu("Validate Building Configs")]
    private void ValidateBuildingConfigs()
    {
        if (m_BuildingConfigs == null)
        {
            Debug.LogError("Building configs array is null!");
            return;
        }

        for (int i = 0; i < m_BuildingConfigs.Length; i++)
        {
            var config = m_BuildingConfigs[i];
            if (string.IsNullOrEmpty(config.buildingId))
            {
                Debug.LogError($"Building config {i}: buildingId is null or empty!");
            }
            if (config.buildingObject == null)
            {
                Debug.LogError($"Building config {i}: buildingObject is null!");
            }
        }

        Debug.Log("Building config validation complete");
    }
    #endif
    #endregion
} 