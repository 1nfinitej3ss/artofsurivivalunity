using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChanceCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private GameObject[] scenarios;
    [SerializeField] private string nextScene;

    private void Start()
    {
        UpdateTitleText();
        StartScenario();
    }

    private void StartScenario()
    {
        scenarios[ChoseRandomScenario()].SetActive(true);
    }

    private int ChoseRandomScenario()
    {
        // Make temp var
        int value;

        // Chose a random scenario and save it in our int var
        value = Random.Range(0 , scenarios.Length);

        // Return our random value;
        return value;
    }

    private void UpdateTitleText()
    {
        titleTxt.text = $"Your {PlayerState.Instance.chanceCardKey.ToLower()} score is low";
    }

    public void CloseChanceCard()
    {
        // Close Chance Card and go back to globe scene
        SceneManager.LoadScene(nextScene);
    }
}