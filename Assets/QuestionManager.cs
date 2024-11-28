using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private Question[] questions; // Array of all questions
    private List<Question> randomQuestionsList; // List of eligible questions
    private Question lastShownQuestion; // Variable to track the last shown question

    private void Start()
    {
        // Initialize the questions list
        FinalQuestionsList();
        
        // Start the question selection process
        ShowNextQuestion();
    }

    private void FinalQuestionsList()
    {
        randomQuestionsList = new List<Question>();

        foreach (Question question in questions)
        {
            // Initialize PlayerPrefs Key For All Questions
            question.InitializeKeyForPlayerPrefs();

            // Add logic to filter questions based on their type and conditions
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
                    if (question.GetQuestionValue() < 2 && PlayerState.Instance.totalDaysPassed <= (3 * 365))
                    {
                        randomQuestionsList.Add(question);
                    }
                    break;
            }
        }
    }

    private void ShowNextQuestion()
    {
        // Filter out the last shown question from the eligible list if there are other questions available
        List<Question> availableQuestions = new List<Question>(randomQuestionsList);
        if (lastShownQuestion != null && availableQuestions.Count > 1)
        {
            availableQuestions.Remove(lastShownQuestion);
        }

        // Choose a random question from the available list
        if (availableQuestions.Count > 0)
        {
            int randomIndex = Random.Range(0, availableQuestions.Count);
            Question nextQuestion = availableQuestions[randomIndex];

            // Show the chosen question
            nextQuestion.gameObject.SetActive(true);

            // Update the last shown question
            lastShownQuestion = nextQuestion;
        }
        else
        {
            Debug.Log("No eligible questions to show.");
        }
    }
}
