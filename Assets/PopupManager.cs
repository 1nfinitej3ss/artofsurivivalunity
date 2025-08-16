using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopupManager : MonoBehaviour
{
    #region Serialized Fields
    [System.Serializable]
    public class PopupData
    {
        public string popupName;
        public GameObject popupPanel;
        public Text popupText;
        public Button openButton;
        public Button closeButton;
    }

    [SerializeField] private List<PopupData> m_Popups = new List<PopupData>();
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializePopups();
    }
    #endregion

    #region Private Methods
    private void InitializePopups()
    {
        foreach (var popup in m_Popups)
        {
            // Hide the popup panel at start
            popup.popupPanel.SetActive(false);

            // Add click listeners to the buttons
            if (popup.openButton != null)
                popup.openButton.onClick.AddListener(() => OpenPopup(popup.popupName));
            if (popup.closeButton != null)
                popup.closeButton.onClick.AddListener(() => ClosePopup(popup.popupName));
        }
    }
    #endregion

    #region Public Methods
    public void OpenPopup(string _popupName)
    {
        var popup = m_Popups.Find(p => p.popupName == _popupName);
        if (popup != null)
        {
            popup.popupPanel.SetActive(true);
            if (popup.popupText != null)
                popup.popupText.text = $"This is the {_popupName} popup!";
        }
    }

    public void ClosePopup(string _popupName)
    {
        var popup = m_Popups.Find(p => p.popupName == _popupName);
        if (popup != null)
        {
            popup.popupPanel.SetActive(false);
        }
    }

    public void CloseAllPopups()
    {
        foreach (var popup in m_Popups)
        {
            popup.popupPanel.SetActive(false);
        }
    }
    #endregion
}