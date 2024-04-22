using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Networking;

// Script to fecth and display on each booth monitor the appropriate business information
public class BusinessInformationFetcher : MonoBehaviour
{
    [SerializeField] private Text businessNameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text missionText;
    [SerializeField] private Text industryText;
    [SerializeField] private Text employeesText;
    [SerializeField] private Text foundedText;
    [SerializeField] private Text headquartersText;
    [SerializeField] private Text contactEmailText;
    [SerializeField] private Text websiteText;

    private FirebaseFirestore db;
    private string currentMonitorTag;

    void OnEnable()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Exception}");
                return;
            }
            db = FirebaseFirestore.DefaultInstance;
        });
    }

    public void RefreshBusinessInformation()
    {
        FetchAndDisplayBusinessInformation();
    }


    public void InitializeWithMonitorTag(string tag)
    {
        currentMonitorTag = tag;
        FetchAndDisplayBusinessInformation();
    }

    // Using the monitor tag to identify the current monitor and populate fields
    private void FetchAndDisplayBusinessInformation()
    {
        if (string.IsNullOrEmpty(currentMonitorTag))
        {
            Debug.LogError("Current monitor tag not set.");
            return;
        }

        string standId = currentMonitorTag.Split('_').Take(2).Aggregate((current, next) => $"{current}_{next}");

        db.Collection("booths").Document(standId).GetSnapshotAsync().ContinueWithOnMainThread(boothTask =>
        {
            if (boothTask.IsFaulted || boothTask.IsCanceled)
            {
                Debug.LogError("Error fetching booth data: " + boothTask.Exception);
                return;
            }

            DocumentSnapshot boothSnapshot = boothTask.Result;
            if (boothSnapshot.Exists)
            {
                string userId = boothSnapshot.GetValue<string>("UserId");

                db.Collection("bus_users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
                {
                    if (userTask.IsFaulted || userTask.IsCanceled)
                    {
                        Debug.LogError("Error fetching business user data: " + userTask.Exception);
                        return;
                    }

                    DocumentSnapshot userSnapshot = userTask.Result;
                    if (userSnapshot.Exists)
                    {
                        UpdateUIWithBusinessInformation(userSnapshot);
                    }
                    else
                    {
                        Debug.LogError($"No business user found for UserId: {userId}");
                    }
                });
            }
            else
            {
                Debug.LogError($"Booth document does not exist: {standId}");
            }
        });
    }

    // Updating predefined fields
    private void UpdateUIWithBusinessInformation(DocumentSnapshot snapshot)
    {
        businessNameText.text = snapshot.GetValue<string>("businessName") ?? "N/A";
        descriptionText.text = snapshot.GetValue<string>("description") ?? "N/A";
        missionText.text = snapshot.GetValue<string>("mission") ?? "N/A";
        industryText.text = snapshot.GetValue<string>("industry") ?? "N/A";
        employeesText.text = snapshot.GetValue<string>("employees") ?? "N/A";
        foundedText.text = snapshot.GetValue<string>("founded") ?? "N/A";
        headquartersText.text = snapshot.GetValue<string>("headquarters") ?? "N/A";
        contactEmailText.text = snapshot.GetValue<string>("contactEmail") ?? "N/A";
        websiteText.text = snapshot.GetValue<string>("website") ?? "N/A";

        TranslateFieldsIfNeeded();
    }

    // Translate fields if the selected language is not English
    private void TranslateFieldsIfNeeded()
    {
        string targetLanguage = PlayerPrefs.GetString("selectedLanguage", "en");

        if (targetLanguage != "en")
        {
            TranslateField(descriptionText, targetLanguage);
            TranslateField(missionText, targetLanguage);
            TranslateField(industryText, targetLanguage);
            TranslateField(foundedText, targetLanguage);
            TranslateField(headquartersText, targetLanguage);
        }
    }

    // Translate only selected fields if not empty
    private void TranslateField(Text uiElement, string targetLanguage)
    {
        if (uiElement.text == "N/A")
        {
            Debug.Log("No text to translate");
            return;
        }

        // Perform the translation
        TranslationManager.Instance.TranslateText(uiElement.text, targetLanguage, translatedText =>
        {
            uiElement.text = UnityWebRequest.UnEscapeURL(translatedText);
        });
    }

    // Clearing on exit as the business coulld change and since all the monitors use
    // a shared canvas to display the info
    public void ClearBusinessInformation()
    {
        businessNameText.text = "N/A";
        descriptionText.text = "N/A";
        missionText.text = "N/A";
        industryText.text = "N/A";
        employeesText.text = "N/A";
        foundedText.text = "N/A";
        headquartersText.text = "N/A";
        contactEmailText.text = "N/A";
        websiteText.text = "N/A";
    }
}