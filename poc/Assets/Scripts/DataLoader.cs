using UnityEngine;
using System.IO;
using TMPro; // Ajoutez cette ligne pour TextMeshPro

[System.Serializable]
public class Data
{
    public string buttonAText;
    public string buttonBText;
}

[System.Serializable]
public class JsonData
{
    public Data[] data;
}


public class DataLoader : MonoBehaviour
{
    public TextMeshProUGUI buttonAText;
    public TextMeshProUGUI buttonBText;
    // Ajoutez d'autres composants UI si nécessaire

    void Start()
    {
        LoadJsonData();
    }

    void LoadJsonData()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "./data.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            ProcessJsonData(json);
        }
        else
        {
            Debug.LogError("Cannot find JSON file!");
        }
    }

    void ProcessJsonData(string json)
    {
        JsonData jsonData = JsonUtility.FromJson<JsonData>(json);

        if (jsonData != null && jsonData.data.Length > 0)
        {
            UpdateUI(jsonData.data[0]);
        }
    }

    void UpdateUI(Data data)
    {
        buttonAText.text = data.buttonAText;
        buttonBText.text = data.buttonBText;
        // Mettez à jour d'autres éléments de texte
    }
}
