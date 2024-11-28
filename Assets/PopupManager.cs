using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public Text popupText;
    public Button openButton;
    public Button closeButton;

    void Start()
    {
        // Hide the popup panel at start
        popupPanel.SetActive(false);

        // Add click listeners to the buttons
        openButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);
    }

    void OpenPopup()
    {
        popupPanel.SetActive(true);
        popupText.text = "This is a simple popup!";
    }

    void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}