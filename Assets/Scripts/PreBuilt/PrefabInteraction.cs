using UnityEngine;
using UnityEngine.UI; // Add this for UI components
using UnityEngine.EventSystems; // Add this for IPointerEnterHandler and IPointerExitHandler
using PlanetRunner;
using UnityEngine.SceneManagement; // Replace "PlanetRunner" with the actual namespace of CameraController

public class PrefabInteraction : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private string m_BuildingId;  // e.g., "Hospital", "School", etc.
    [SerializeField] private string m_SceneToLoad; // Scene to load when button is clicked
    [SerializeField] private string m_DialogTag;   // Tag to identify which dialog content to show
    #endregion

    #region Properties
    public string BuildingId => m_BuildingId;
    public string DialogTag => m_DialogTag;
    #endregion

    #region Trigger Methods
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered 2D trigger for building: {m_BuildingId}");
            BuildingButtonManager.Instance.ShowButton(m_BuildingId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered 3D trigger for building: {m_BuildingId}");
            BuildingButtonManager.Instance.ShowButton(m_BuildingId);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited 2D trigger for building: {m_BuildingId}");
            BuildingButtonManager.Instance.HideButton(m_BuildingId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited 3D trigger for building: {m_BuildingId}");
            BuildingButtonManager.Instance.HideButton(m_BuildingId);
        }
    }
    #endregion
}
