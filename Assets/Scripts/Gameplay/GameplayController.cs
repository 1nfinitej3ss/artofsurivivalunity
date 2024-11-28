using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private Question[] questions; // Put all questions for this scene in this
    private List<Question> randomQuestionsList; // We will choose final random questions from this list
    private PlayerState playerState;
    private Question lastShownQuestion; // To keep track of the last shown question

    private void Start()
    {
        // Set playerState Reference
        playerState = PlayerState.Instance;

        // Set up the list of eligible questions
        FinalQuestionsList();

        // Start the random question system
        ShowNextQuestion();
    }

    private void FinalQuestionsList()
    {
        // Reset the list of eligible questions
        randomQuestionsList = new List<Question>();

        foreach (Question question in questions)
        {
            // Initialize PlayerPrefs Key for each question
            question.InitializeKeyForPlayerPrefs();

            // Filter questions based on type and conditions
            switch (question.GetQuestionType())
            {
                case "Black":
                    if (question.GetQuestionValue() < 1)
                    {
                        randomQuestionsList.Add(question);
                    }
                    break;

                case "Orange":
                    randomQuestionsList.Add(question);
                    break;

                case "Green":
                    if (question.GetQuestionValue() < 2)
                    {
                        randomQuestionsList.Add(question);
                    }
                    break;

                case "Blue":
                    if (question.GetQuestionValue() < 2 && playerState.totalDaysPassed <= (3 * 365))
                    {
                        randomQuestionsList.Add(question);
                    }
                    break;
            }
        }
    }

    private void ShowNextQuestion()
    {
        // Filter out the last shown question if there are other eligible questions
        List<Question> availableQuestions = new List<Question>(randomQuestionsList);
        if (lastShownQuestion != null && availableQuestions.Count > 1)
        {
            availableQuestions.Remove(lastShownQuestion);
        }

        // Choose a random question from the filtered list
        if (availableQuestions.Count > 0)
        {
            int randomIndex = Random.Range(0, availableQuestions.Count);
            Question nextQuestion = availableQuestions[randomIndex];

            // Activate the chosen question
            nextQuestion.gameObject.SetActive(true);

            // Update the last shown question
            lastShownQuestion = nextQuestion;
        }
        else
        {
            Debug.Log("No eligible questions to show.");
        }
    }

    // Call this method to proceed to the next question
    public void OnNextQuestionButtonClicked()
    {
        // Deactivate the last shown question (optional)
        if (lastShownQuestion != null)
        {
            lastShownQuestion.gameObject.SetActive(false);
        }

        // Show the next question
        ShowNextQuestion();
    }
}
