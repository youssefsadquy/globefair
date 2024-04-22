using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Method that uses the Google cloud API to translate the string of text
public class TranslationManager : MonoBehaviour
{
    public static TranslationManager Instance;
    private string apiKey = "YOUR_GOOGLE_TRANSLATE_API_KEY";

    // Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Method to perform translation
    public void TranslateText(string textToTranslate, string targetLanguage, Action<string> callback)
    {
        string url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}&q={Uri.EscapeDataString(textToTranslate)}&target={targetLanguage}";
        StartCoroutine(SendTranslationRequest(url, textToTranslate, callback));
    }

    // Translating the text and returning the translated string to the caller
    private IEnumerator SendTranslationRequest(string url, string textToTranslate, Action<string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Translation success");
                try
                {
                    var jsonResponse = JsonUtility.FromJson<TranslationResponse>(webRequest.downloadHandler.text);
                    string translatedText = jsonResponse.data.translations[0].translatedText;
                    callback(translatedText);
                }
                catch (Exception e)
                {
                    Debug.LogError("Translation failed");
                    callback(textToTranslate);
                }
            }
            else
            {
                Debug.LogError("Translation request failed: " + webRequest.error);
                callback(textToTranslate);
            }
        }
    }
}

// Classes to match the JSON response from Google API
[Serializable]
public class TranslationResponse
{
    public Data data;
}

[Serializable]
public class Data
{
    public List<Translation> translations;
}

[Serializable]
public class Translation
{
    public string translatedText;
}