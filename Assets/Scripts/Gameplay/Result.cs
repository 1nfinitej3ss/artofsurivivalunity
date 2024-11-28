using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Result : MonoBehaviour
{
    /// <summary>
    /// This script is only enabling the final result the user has gotten.
    /// Out of all of the possible reults, it enables the random or the only result which is available
    /// </summary>


    [SerializeField] private Question question;

    [Header ("Main")]
    [SerializeField] private GameObject aObj;
    [SerializeField] private GameObject bObj;
    [SerializeField] private GameObject cObj;
    [SerializeField] private GameObject dObj;
    [SerializeField] private GameObject eObj;

    [Header ("Possible Results")]
    [SerializeField] private ResultFinal[] resultsA;
    [SerializeField] private ResultFinal[] resultsB;
    [SerializeField] private ResultFinal[] resultsC;
    [SerializeField] private ResultFinal[] resultsD;
    [SerializeField] private ResultFinal[] resultsE;

    private string optionSelected;

    private void Awake()
    {
        // Disable all items in start
        DisableAllItems();
    }

    private void Start()
    {
        // Get the option that we selected in the OptionsScreen
        optionSelected = question.optionSelected;

        // Start Results
        StartResult();
    }

    private void DisableAllItems()
    {
        // Main objects

        if(aObj != null)
        {
            aObj.SetActive(false);
        }

        if (bObj != null)
        {
            bObj.SetActive(false);
        }

        if (cObj != null)
        {
            cObj.SetActive(false);
        }

        if (dObj != null)
        {
            dObj.SetActive(false);
        }

        if (eObj != null)
        {
            eObj.SetActive(false);
        }

        // Answers of the main objects

        foreach (ResultFinal result in resultsA)
        {
            result.gameObject.SetActive(false);
        }

        foreach (ResultFinal result in resultsB)
        {
            result.gameObject.SetActive(false);
        }

        foreach (ResultFinal result in resultsC)
        {
            result.gameObject.SetActive(false);
        }

        foreach (ResultFinal result in resultsD)
        {
            result.gameObject.SetActive(false);
        }

        foreach (ResultFinal result in resultsE)
        {
            result.gameObject.SetActive(false);
        }
    }

    private void StartResult()
    {
        switch (optionSelected)
        {
            case "A":
                AResult();
                break;

            case "B":
                BResult();
                break;

            case "C":
                CResult();
                break;

            case "D":
                DResult();
                break;

            case "E":
                EResult();
                break;
        }
    }

    private void AResult()
    {
        // Enable the main obj
        aObj.SetActive(true);

        //-------------------

        // Now we chose a random result if we have more than 1 possible results
        if(resultsA.Length > 1)
        {
            // Chose Random Answer From Them
            int randomAnswer = Random.Range(0, resultsA.Length);

            // Enable that result
            resultsA[randomAnswer].gameObject.SetActive(true);
        }

        // Otherwise we jsut open the single reult
        else
        {
            resultsA[0].gameObject.SetActive(true);
        }
    }

    private void BResult()
    {
        // Enable the main obj
        bObj.SetActive(true);

        //-------------------

        // Now we chose a random result if we have more than 1 possible results
        if (resultsB.Length > 1)
        {
            // Chose Random Answer From Them
            int randomAnswer = Random.Range(0, resultsB.Length);

            // Enable that result
            resultsB[randomAnswer].gameObject.SetActive(true);
        }

        // Otherwise we jsut open the single reult
        else
        {
            resultsB[0].gameObject.SetActive(true);
        }
    }

    private void CResult()
    {
        // Enable the main obj
        cObj.SetActive(true);

        //-------------------

        // Now we chose a random result if we have more than 1 possible results
        if (resultsC.Length > 1)
        {
            // Chose Random Answer From Them
            int randomAnswer = Random.Range(0, resultsC.Length);

            // Enable that result
            resultsC[randomAnswer].gameObject.SetActive(true);
        }

        // Otherwise we jsut open the single reult
        else
        {
            resultsC[0].gameObject.SetActive(true);
        }
    }
    
    private void DResult()
    {
        // Enable the main obj
        dObj.SetActive(true);

        //-------------------

        // Now we chose a random result if we have more than 1 possible results
        if (resultsD.Length > 1)
        {
            // Chose Random Answer From Them
            int randomAnswer = Random.Range(0, resultsD.Length);

            // Enable that result
            resultsD[randomAnswer].gameObject.SetActive(true);
        }

        // Otherwise we jsut open the single reult
        else
        {
            resultsD[0].gameObject.SetActive(true);
        }
    }

    private void EResult()
    {
        // Enable the main obj
        eObj.SetActive(true);

        //-------------------

        // Now we chose a random result if we have more than 1 possible results
        if (resultsE.Length > 1)
        {
            // Chose Random Answer From Them
            int randomAnswer = Random.Range(0, resultsE.Length);

            // Enable that result
            resultsE[randomAnswer].gameObject.SetActive(true);
        }

        // Otherwise we jsut open the single reult
        else
        {
            resultsE[0].gameObject.SetActive(true);
        }
    }
}
