using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public enum TitleType { Money, Career, Energy, Creativity, Time }
    public TitleType titleType;

    [Header("Title Setting")]
    public string Title;
    public Color TitleColor;
    public Font TitleFont;
    public int TitleFontSize = 10;

    [Header("Bar Setting")]
    public Color BarColor;
    public Color BarBackGroundColor;
    public Sprite BarBackGroundSprite;
    [Range(1f, 100f)]
    public int Alert = 20;
    public Color BarAlertColor;

    // Add max value setting
    [SerializeField] private float maxValue = 100f;

    [Header("Sound Alert")]
    public AudioClip sound;
    public bool repeat = false;
    public float RepeatRate = 1f;

    private Image bar, barBackground;
    private float nextPlay;
    private AudioSource audiosource;
    private Text txtTitle;
    private float barValue;

    private void Awake()
    {
        // Assigning UI elements
        bar = transform.Find("Bar").GetComponent<Image>();
        barBackground = GetComponent<Image>();
        txtTitle = transform.Find("Text").GetComponent<Text>();
        barBackground = transform.Find("BarBackground").GetComponent<Image>();
        audiosource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Ensure UI components are properly referenced
        if (bar == null)
        {
            Debug.LogError($"Progress Bar {titleType} is missing bar reference!");
            return;
        }

        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bar.color = BarColor;
        barBackground.color = BarBackGroundColor;
        barBackground.sprite = BarBackGroundSprite;

        // Subscribe to state changes
        if (PlayerState.Instance != null)
        {
            PlayerState.Instance.OnStateChange += UpdateUI;
            float currentValue = PlayerState.Instance.GetPlayerValue(titleType.ToString());
            UpdateUI(titleType.ToString(), (int)currentValue);
        }
        else
        {
            Debug.LogError("PlayerState.Instance is null!");
        }
    }

    private void UpdateUI(string key, int value)
    {
        if(SceneManager.GetActiveScene().name == "main")
        {
            if (key.ToLower() != titleType.ToString().ToLower()) return;
            barValue = value;

            if (bar == null)
            {
                Debug.LogError($"Progress bar Image component is null for {titleType}!");
                return;
            }

            bar.fillAmount = value / 100f;
            
            if (txtTitle != null)
            {
                txtTitle.text = value.ToString();
            }

            if (Alert >= value)
            {
                bar.color = BarAlertColor;
            }
            else
            {
                bar.color = BarColor;
            }

            // Add comprehensive console log for time progress bar
            if (titleType == TitleType.Time)
            {
                if (PlayerState.Instance != null)
                {
                    // Get the current time value that would be saved as final score
                    float finalTimeValue = PlayerState.Instance.GetPlayerValue("Time");
                    Debug.Log($"[TimeCheck] Current Time Attribute: {value}, Final Time Score: {finalTimeValue}, Days Passed: {PlayerState.Instance.totalDaysPassed}, Years Left: {(1825 - PlayerState.Instance.totalDaysPassed) / 365f:F1}");
                }
            }
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {           
            txtTitle.color = TitleColor;
            txtTitle.font = TitleFont;
            txtTitle.fontSize = TitleFontSize;

            bar.color = BarColor;
            barBackground.color = BarBackGroundColor;

            barBackground.sprite = BarBackGroundSprite;           
        }
        else
        {
            if (Alert >= barValue && Time.time > nextPlay)
            {
                nextPlay = Time.time + RepeatRate;
                audiosource.PlayOneShot(sound);
            }
        }
    }

    public void UnsubsribeUI()
    {
        // This is called in GameController's OnSceneChanged()

        // To avoid errors after changing scenes...
        // Unsubscribing becasue the PlayerState.Instance which we subscribed to - was present in previous scene, which is now destroyed
        // We will subscribe again from this new laoded scene. Look at last line of Start()
        PlayerState.Instance.OnStateChange -= UpdateUI;
    }
}
