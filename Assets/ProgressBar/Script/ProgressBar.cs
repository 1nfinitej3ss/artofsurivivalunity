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
        txtTitle.text = Title;
        txtTitle.color = TitleColor;
        txtTitle.font = TitleFont;
        txtTitle.fontSize = TitleFontSize;

        bar.color = BarColor;
        barBackground.color = BarBackGroundColor;
        barBackground.sprite = BarBackGroundSprite;

        PlayerState.Instance.OnStateChange += UpdateUI;
        barValue = PlayerState.Instance.GetPlayerValue(titleType.ToString());
        //UpdateUI(titleType.ToString(), (int)barValue);
        UpdateUI(titleType.ToString(), (int)barValue);
    }

    private void UpdateUI(string key, int value)
    {
        if(SceneManager.GetActiveScene().name == "main")
        {
            if (key.ToLower() != titleType.ToString().ToLower()) return;
            barValue = value;

            bar.fillAmount = value / 100f;
            //txtTitle.text = Title + " " + value + "/100";
            txtTitle.text = value.ToString();

            if (Alert >= value)
            {
                bar.color = BarAlertColor;
            }
            else
            {
                bar.color = BarColor;
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
