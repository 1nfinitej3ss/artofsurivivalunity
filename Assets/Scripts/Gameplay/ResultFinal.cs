using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultFinal : MonoBehaviour
{
    /// <summary>
    /// This script is the main backbone of the results, it is attached to all Result Objects
    /// It has the values (attributes) which we will set in inspector which will be saved if player chooses this result
    /// </summary>

    [Header("Outcome No. 1")]
    [SerializeField] private TitleType title1;
    [SerializeField] private bool isAvailable1;
    [SerializeField] private int value1;

    [Header("Outcome No. 2")]
    [SerializeField] private TitleType title2;
    [SerializeField] private bool isAvailable2;
    [SerializeField] private int value2;

    [Header("Outcome No. 3")]
    [SerializeField] private TitleType title3;
    [SerializeField] private bool isAvailable3;
    [SerializeField] private int value3;

    [Header("Outcome No. 4")]
    [SerializeField] private TitleType title4;
    [SerializeField] private bool isAvailable4;
    [SerializeField] private int value4;

    [Header ("Extra Event")]
    [SerializeField] private UnityEvent resultEvent;

    [Header ("Extra Event Items")]
    [SerializeField] private TextMeshProUGUI workQuestion2OptionCResultValuesText;
    [SerializeField] private TextMeshProUGUI workQuestion2OptionDResultValuesText;
    [SerializeField] private TextMeshProUGUI workQuestion5OptionDResultValuesText;
    [SerializeField] private TextMeshProUGUI studioQuestion1OptionAResultValuesText;

    private PlayerState playerState;

    public enum TitleType { Money, Career, Energy, Creativity, Time }

    private void Start()
    {
        // Save reference to our Player
        playerState = PlayerState.Instance;

        // Update our PlayerStats values
        UpdatePlayerState();
    }

    private void UpdatePlayerState()
    {
        // Check if our Outcome 1 is enabled
        if (isAvailable1)
        {
            playerState.SetPlayerValue(title1.ToString(), playerState.GetPlayerValue(title1.ToString()) + value1, true);
            Debug.Log("Player's " + title1.ToString() + " Has Been Changed By " + value1);
        }

        // Check if our Outcome 2 is enabled
        if (isAvailable2)
        {
            playerState.SetPlayerValue(title2.ToString(), playerState.GetPlayerValue(title2.ToString()) + value2, true);
            Debug.Log("Player's " + title2.ToString() + " Has Been Changed By " + value2);
        }

        // Check if our Outcome 3 is enabled
        if (isAvailable3)
        {
            playerState.SetPlayerValue(title3.ToString(), playerState.GetPlayerValue(title3.ToString()) + value3, true);
            Debug.Log("Player's " + title3.ToString() + " Has Been Changed By " + value3);
        }

        // Check if our Outcome 4 is enabled
        if (isAvailable4)
        {
            playerState.SetPlayerValue(title4.ToString(), playerState.GetPlayerValue(title4.ToString()) + value4, true);
            Debug.Log("Player's " + title4.ToString() + " Has Been Changed By " + value4);
        }

        // Run the result event if it is not empty
        if (resultEvent != null)
        {
            resultEvent.Invoke();
        }
    }

    // Result Events
    public void HomeQuestion2OptionA()
    {
        // Save that we would open special game over screen in game over scene; check returnToMain script for more details
        PlayerPrefs.HasKey("SpecialGameOver");

        // Start Game Over Scene !
        SceneManager.LoadScene("GameOver");
    }

    public void HomeQuestion4OptionB()
    {
        playerState.SetMonthlyCharges(playerState.GetMonthlyCharges("Rent") , playerState.GetMonthlyCharges("Utilities") , 30);
    }

    public void HomeQuestion4OptionE()
    {
        // Carreer drops to 0
        playerState.SetPlayerValue("Career" , 0 , true);
    }

    public void WorkQuestion2OptionC()
    {
        // Chose +30 or -30 Money
        int random = Random.Range(0 , 2);

        // +30
        if(random == 0)
        {
            playerState.SetPlayerValue("Money", playerState.GetPlayerValue("Money") + 30, true);
            Debug.Log("Player's Money Has Been Changed By +30");

            workQuestion2OptionCResultValuesText.text = "–30 Energy\n+30 Money";
        }

        // -30
        else
        {
            playerState.SetPlayerValue("Money", playerState.GetPlayerValue("Money") - 30, true);
            Debug.Log("Player's Money Has Been Changed By -30");

            workQuestion2OptionCResultValuesText.text = "–30 Energy\n-30 Money";
        }
    }

    public void WorkQuestion2OptionD()
    {
        // Chose +20 Career or Money
        int random = Random.Range(0, 2);

        // +20 Career
        if (random == 0)
        {
            playerState.SetPlayerValue("Career", playerState.GetPlayerValue("Career") + 20, true);
            Debug.Log("Player's Career Has Been Changed By +20");

            workQuestion2OptionDResultValuesText.text = "+10 Time\n+20 Career";
        }

        // +20 Money
        else
        {
            playerState.SetPlayerValue("Money", playerState.GetPlayerValue("Money") + 20, true);
            Debug.Log("Player's Money Has Been Changed By +20");

            workQuestion2OptionDResultValuesText.text = "+10 Time\n+20 Money";
        }
    }

    public void WorkQuestion5OptionD()
    {
        // Chose +100 or -100 Money
        int random = Random.Range(0, 2);

        // +100
        if (random == 0)
        {
            playerState.SetPlayerValue("Money", playerState.GetPlayerValue("Money") + 100, true);
            Debug.Log("Player's Money Has Been Changed By +100");

            workQuestion5OptionDResultValuesText.text = "-70 Energy\n+100 Money";
        }

        // -100
        else
        {
            playerState.SetPlayerValue("Money", playerState.GetPlayerValue("Money") - 100, true);
            Debug.Log("Player's Money Has Been Changed By -100");

            workQuestion5OptionDResultValuesText.text = "-70 Energy\n-100 Money";
        }
    }

    public void StudioQuestion1OptionA()
    {
        // Chose +30 or -30 Career
        int random = Random.Range(0, 2);

        // +100
        if (random == 0)
        {
            playerState.SetPlayerValue("Career", playerState.GetPlayerValue("Career") + 30, true);
            Debug.Log("Player's Career Has Been Changed By +30");

            studioQuestion1OptionAResultValuesText.text = "-40 Money\n-30 Energy\n+40 Creativity\n+30 Career";
        }

        // -100
        else
        {
            playerState.SetPlayerValue("Career", playerState.GetPlayerValue("Career") - 30, true);
            Debug.Log("Player's Career Has Been Changed By -30");

            studioQuestion1OptionAResultValuesText.text = "-40 Money\n-30 Energy\n+40 Creativity\n-30 Career";
        }
    }
}