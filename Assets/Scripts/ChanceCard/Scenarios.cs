using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Scenarios : MonoBehaviour
{
    /// <summary>
    /// Scenario's Outcomes - Max Possilbe are 4 - We can add more if needed...
    /// Just copy paste any header and change the numbers to 4, 5, 6, etc.
    /// </summary>
    
    [Header ("Outcome No. 1")]
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

    [Header("Outcome No. 5")]
    [SerializeField] private TitleType title5;
    [SerializeField] private bool isAvailable5;
    [SerializeField] private int value5;

    private PlayerState playerState;

    public enum TitleType { Money, Career, Energy, Creativity, Time }

    private void OnEnable()
    {
        // show in console that we have this scenario chose randomly
        Debug.Log("Scenario No. " + this.gameObject.name + " - Is Enabled!");

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
            playerState.SetPlayerValue(title1.ToString(), playerState.GetPlayerValue(title1.ToString()) + value1 , false);
            Debug.Log("Player's " + title1.ToString() + " Has Been Changed By " + value1);
        }

        // Check if our Outcome 2 is enabled
        if (isAvailable2)
        {
            playerState.SetPlayerValue(title2.ToString(), playerState.GetPlayerValue(title2.ToString()) + value2, false);
            Debug.Log("Player's " + title2.ToString() + " Has Been Changed By " + value2);
        }

        // Check if our Outcome 3 is enabled
        if (isAvailable3)
        {
            playerState.SetPlayerValue(title3.ToString(), playerState.GetPlayerValue(title3.ToString()) + value3, false);
            Debug.Log("Player's " + title3.ToString() + " Has Been Changed By " + value3);
        }

        // Check if our Outcome 4 is enabled
        if (isAvailable4)
        {
            playerState.SetPlayerValue(title4.ToString(), playerState.GetPlayerValue(title4.ToString()) + value4, false);
            Debug.Log("Player's " + title4.ToString() + " Has Been Changed By " + value4);
        }

        // Check if our Outcome 5 is enabled
        if (isAvailable5)
        {
            playerState.SetPlayerValue(title5.ToString(), playerState.GetPlayerValue(title5.ToString()) + value5, false);
            Debug.Log("Player's " + title5.ToString() + " Has Been Changed By " + value5);
        }
    }
}