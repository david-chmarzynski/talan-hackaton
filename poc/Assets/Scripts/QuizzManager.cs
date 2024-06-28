using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using static QuizzManager;

public class QuizzManager : MonoBehaviour
{
    [System.Serializable]
    public class Option
    {
        public string answer;
        public bool correct;
    }

    [System.Serializable]
    public class APIResponse
    {
        public string model;
        public string created_at;
        public string response;
        public bool done;
        public string done_reason;
        public List<int> context;
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

    public Color correctColor = Color.green;  // Couleur pour la bonne r�ponse
    public Color incorrectColor = Color.red;  // Couleur pour la mauvaise r�ponse
    public Color defaultColor = Color.white;  // Couleur par d�faut des boutons

    RequestBody requestBody;

    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    public List<string> LIST_TOPIC = new List<string>
    {
        "R�duction des d�chets",
        "�nergie durable",
        "Transport �coresponsable",
        "Achats responsables",
        "RSE",
        "HSE",
        "�conomie circulaire",
        "�coconception",
        "�nergie renouvelable",
        "�nergie verte",
        "�conomie verte",
        "D�veloppement durable",
        "�cologie",
        "Consommation durable"
    };

    private System.Random random = new System.Random();

    void Start()
    {
        Debug.Log("QuizManager Start");
        int index = random.Next(LIST_TOPIC.Count);
        string selectedTopic = LIST_TOPIC[index];
        requestBody = new RequestBody
        {
            model = "llama3",
            prompt = @$"Tout est en francais uniquement ! 
    Cr�e une question sur l'�coresponsabilit� en entreprise avec le format JSON sur le th�me de {selectedTopic}. 
    Il doit y avoir 1 question et 4 r�ponses possible � chaque fois !
    Il faut que tu me donne une explication � la r�ponse de la question dans la cl� ""explanation"".

    Format JSON � respecter imp�rativement et obligatoirement :
    ```json
        {{
            ""questions"": [
                {{
                    ""question"": ""La Question sur lecoresponsabilit� et la hse, rse en entreprise"",
                    ""options"": [
                        {{
                            ""answer"": ""La r�ponse 1"",
                            ""correct"": false
                        }},
                        {{
                            ""answer"": ""La r�ponse 2"",
                            ""correct"": true
                        }},
                        {{
                            ""answer"": ""La r�ponse 3"",
                            ""correct"": false
                        }},
                        {{
                            ""answer"": ""La r�ponse 4"",
                            ""correct"": false
                        }}
                    ],
                    ""explanation"": ""Une courte explication � la r�ponse de la question.""
                }}
            ]
        }}
    ```

    Je veux uniquement la r�ponse JSON avec le format � respecter obligatoirement, ne met pas texte superflu dans ta r�ponse.
    Je veux uniquement la r�ponse JSON.
    Je veux un maximum de 1 question et 4 r�ponse, avec 1 vraie r�ponse et 3 fausses r�ponse.
    Je veux que tu varies la position de la bonne r�ponse.
    Ne g�n�re jamais 2 fois la m�me question ! Essaye de diversifier les questions au maximum.
    J'insiste, il faut absolument que tu respectes le format JSON que je t'ai donn�",
            stream = false
        };
        StartCoroutine(Generate(requestBody.prompt));
        if (questions != null)
        {
            DisplayQuestion();
        }
        else
        {
            Debug.LogError("No questions loaded or questions list is empty.");
        }
    }

    IEnumerator Generate(string prompt)
    {
        RequestBody body = new RequestBody
        {
            model = "llama3",
            prompt = prompt,
            stream = false
        };

        string json = JsonUtility.ToJson(body);

        using (UnityWebRequest request = new UnityWebRequest("http://86.201.147.102:11434/api/generate", "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            string responseJson = request.downloadHandler.text;

            // Remove leading and trailing curly braces
            if (responseJson.StartsWith("{"))
            {
                responseJson = responseJson.Substring(1);
            }
            if (responseJson.EndsWith("}"))
            {
                responseJson = responseJson.Substring(0, responseJson.Length - 1);
            }

            responseJson = "{" + responseJson + "}";

            Debug.Log(responseJson);

            APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(responseJson);
            Debug.Log("response : " + apiResponse.response);

            try
            {
                questions = JsonUtility.FromJson<QuestionList>(apiResponse.response);
            } catch
            {
                Generate(body.prompt);
            }
            

            if (questions != null)
            {
                DisplayQuestion();
            }
            else
            {
                Debug.LogError("No questions loaded or questions list is empty.");
            }

        }
    }
    /*void LoadQuestionsFromJSON()
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
    }*/

    void DisplayQuestion()
    {
        if (questions != null)
        {
                Question question = questions.questions[0];
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
                            if (buttonImage != null) buttonImage.color = defaultColor; 
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
            Debug.LogError("No questions loaded or questions list is empty.");
        }
    }


    public void OnAnswerSelected(int index)
    {
        StartCoroutine(Generate(requestBody.prompt));

        Debug.Log("Answer selected: " + index);
            Question question = questions.questions[0];
            bool isCorrect = question.options[index].correct;
            DisplayExplanation(isCorrect, question.explanation);

            // Changer la couleur du bouton s�lectionn�
            Image selectedButtonImage = answerButtons[index].targetGraphic as Image;
            if (selectedButtonImage != null)
            {
                Color selectedColor = isCorrect ? correctColor : incorrectColor;
                selectedButtonImage.color = selectedColor;
            }

            // D�sactiver les boutons apr�s la s�lection
            foreach (Button btn in answerButtons)
            {
                if (btn != null) btn.interactable = false;
            }

            // Passer � la question suivante apr�s une courte pause
            StartCoroutine(NextQuestion());
    }

    IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(8); // Attendre 8 secondes avant de passer � la question suivante

        currentQuestionIndex++;
        if (currentQuestionIndex < 10)
        {
            DisplayQuestion();
        }
        else
        {
            // Fin du quiz
            if (questionText != null) questionText.text = "F�licitations ! Vous avez termin� le quiz.";
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
