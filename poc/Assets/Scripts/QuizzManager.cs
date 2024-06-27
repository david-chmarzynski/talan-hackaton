using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class QuizzManager : MonoBehaviour
{
    [System.Serializable]
    public class Option
    {
        public string answer;
        public bool correct;
    }

    [System.Serializable]
    public class Question
    {
        public string question;
        public List<Option> options;
        public string explanation;
    }

    [System.Serializable]
    public class QuestionList
    {
        public List<Question> questions;
    }

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;
    public TextMeshProUGUI explanationText;
    public Button[] answerButtons;
    private QuestionList questions;
    private int currentQuestionIndex = 0;

    public Color correctColor = Color.green;  // Couleur pour la bonne réponse
    public Color incorrectColor = Color.red;  // Couleur pour la mauvaise réponse
    public Color defaultColor = Color.white;  // Couleur par défaut des boutons

    void Start()
    {
        Debug.Log("QuizManager Start");
        LoadQuestionsFromJSON();
        if (questions != null && questions.questions.Count > 0)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            Debug.LogError("No questions loaded or questions list is empty.");
        }
    }

    void LoadQuestionsFromJSON()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "data.json");
        Debug.Log("File path: " + filePath);

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            Debug.Log("JSON data: " + jsonData);
            questions = JsonUtility.FromJson<QuestionList>(jsonData);
            Debug.Log("Questions loaded: " + questions.questions.Count);
        }
        else
        {
            Debug.LogError("Cannot find file!");
        }
    }

    void DisplayQuestion(int questionIndex)
    {
        Debug.Log("Displaying question at index: " + questionIndex);
        if (questions != null && questions.questions.Count > 0)
        {
            if (questionIndex < questions.questions.Count)
            {
                Question question = questions.questions[questionIndex];
                if (questionText != null) questionText.text = question.question;
                Debug.Log("Question: " + question.question);

                for (int i = 0; i < answerTexts.Length; i++)
                {
                    if (i < question.options.Count)
                    {
                        if (answerTexts[i] != null) answerTexts[i].text = question.options[i].answer;
                        if (answerButtons[i] != null)
                        {
                            answerButtons[i].interactable = true;
                            Image buttonImage = answerButtons[i].targetGraphic as Image;
                            if (buttonImage != null) buttonImage.color = defaultColor;  // Réinitialiser la couleur du bouton
                        }
                        Debug.Log("Answer " + i + ": " + question.options[i].answer);
                    }
                    else
                    {
                        if (answerTexts[i] != null) answerTexts[i].text = "";
                        if (answerButtons[i] != null) answerButtons[i].interactable = false;
                    }
                }
                if (explanationText != null) explanationText.text = "";
            }
            else
            {
                Debug.LogError("Question index out of range.");
            }
        }
        else
        {
            Debug.LogError("No questions loaded or questions list is empty.");
        }
    }

    public void OnAnswerSelected(int index)
    {
        Debug.Log("Answer selected: " + index);
        if (questions != null && currentQuestionIndex < questions.questions.Count)
        {
            Question question = questions.questions[currentQuestionIndex];
            bool isCorrect = question.options[index].correct;
            DisplayExplanation(isCorrect, question.explanation);

            // Changer la couleur du bouton sélectionné
            Image selectedButtonImage = answerButtons[index].targetGraphic as Image;
            if (selectedButtonImage != null)
            {
                Color selectedColor = isCorrect ? correctColor : incorrectColor;
                selectedButtonImage.color = selectedColor;
            }

            // Désactiver les boutons après la sélection
            foreach (Button btn in answerButtons)
            {
                if (btn != null) btn.interactable = false;
            }

            // Passer à la question suivante après une courte pause
            StartCoroutine(NextQuestion());
        }
    }

    IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(8); // Attendre 8 secondes avant de passer à la question suivante

        currentQuestionIndex++;
        if (currentQuestionIndex < questions.questions.Count)
        {
            DisplayQuestion(currentQuestionIndex);
        }
        else
        {
            // Fin du quiz
            if (questionText != null) questionText.text = "Félicitations ! Vous avez terminé le quiz.";
            for (int i = 0; i < answerTexts.Length; i++)
            {
                if (answerTexts[i] != null) answerTexts[i].text = "";
                if (answerButtons[i] != null) answerButtons[i].interactable = false;
            }
            if (explanationText != null) explanationText.text = "";
        }
    }

    void DisplayExplanation(bool isCorrect, string explanation)
    {
        if (isCorrect)
        {
            if (explanationText != null) explanationText.text = "Correct! " + explanation;
        }
        else
        {
            if (explanationText != null) explanationText.text = "Incorrect. " + explanation;
        }
    }
}
