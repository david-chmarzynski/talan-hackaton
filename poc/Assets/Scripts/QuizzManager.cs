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

    public Color correctColor = Color.green;  // Couleur pour la bonne réponse
    public Color incorrectColor = Color.red;  // Couleur pour la mauvaise réponse
    public Color defaultColor = Color.white;  // Couleur par défaut des boutons

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
        "Réduction des déchets",
        "Énergie durable",
        "Transport écoresponsable",
        "Achats responsables",
        "RSE",
        "HSE",
        "Économie circulaire",
        "Écoconception",
        "Énergie renouvelable",
        "Énergie verte",
        "Économie verte",
        "Développement durable",
        "Écologie",
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
    Crée une question sur l'écoresponsabilité en entreprise avec le format JSON sur le thème de {selectedTopic}. 
    Il doit y avoir 1 question et 4 réponses possible à chaque fois !
    Il faut que tu me donne une explication à la réponse de la question dans la clé ""explanation"".

    Format JSON à respecter impérativement et obligatoirement :
    ```json
        {{
            ""questions"": [
                {{
                    ""question"": ""La Question sur lecoresponsabilité et la hse, rse en entreprise"",
                    ""options"": [
                        {{
                            ""answer"": ""La réponse 1"",
                            ""correct"": false
                        }},
                        {{
                            ""answer"": ""La réponse 2"",
                            ""correct"": true
                        }},
                        {{
                            ""answer"": ""La réponse 3"",
                            ""correct"": false
                        }},
                        {{
                            ""answer"": ""La réponse 4"",
                            ""correct"": false
                        }}
                    ],
                    ""explanation"": ""Une courte explication à la réponse de la question.""
                }}
            ]
        }}
    ```

    Je veux uniquement la réponse JSON avec le format à respecter obligatoirement, ne met pas texte superflu dans ta réponse.
    Je veux uniquement la réponse JSON.
    Je veux un maximum de 1 question et 4 réponse, avec 1 vraie réponse et 3 fausses réponse.
    Je veux que tu varies la position de la bonne réponse.
    Ne génére jamais 2 fois la même question ! Essaye de diversifier les questions au maximum.
    J'insiste, il faut absolument que tu respectes le format JSON que je t'ai donné",
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

    IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(8); // Attendre 8 secondes avant de passer à la question suivante

        currentQuestionIndex++;
        if (currentQuestionIndex < 10)
        {
            DisplayQuestion();
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
