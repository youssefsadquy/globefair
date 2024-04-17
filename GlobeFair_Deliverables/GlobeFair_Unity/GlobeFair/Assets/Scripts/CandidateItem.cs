using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

// Prefab script for each candidate application to populate
public class CandidateItem : MonoBehaviour
{
    [SerializeField] private Text firstNameText;
    [SerializeField] private Text emailText;
    [SerializeField] private Text jobTitleText;
    [SerializeField] private Text applicationDateText;
    [SerializeField] private Button discardButton;
    [SerializeField] private Button downloadCVButton;

    private FirebaseFirestore db;
    private string userId;
    private string candidateId;
    private string cvUrl;
    private bool confirmDiscard = false;

    private CandidatesManager candidatesManager;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        discardButton.onClick.AddListener(OnDiscardClicked);
        downloadCVButton.onClick.AddListener(OnDownloadCVClicked);
    }

    // Method to assign fields on prefab
    public void SetCandidateInfo(CandidatesManager manager, string userId, string candidateId, string firstName, string email, string jobTitle, string applicationDate, string cvUrl)
    {
        firstNameText.text = firstName;
        emailText.text = email;
        jobTitleText.text = jobTitle;
        applicationDateText.text = applicationDate;
        this.cvUrl = cvUrl;
        this.candidatesManager = manager;
        this.candidateId = candidateId;
    }

    // Method to discard candidates applications
    private void OnDiscardClicked()
    {
        if (!confirmDiscard)
        {
            // Confirmation step
            discardButton.GetComponentInChildren<Text>().text = "SURE?";
            confirmDiscard = true;
        }
        else
        {
            // Delete application document and refresh the list
            db.Collection("candidates").Document(candidateId).DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Candidate application discarded.");
                    Destroy(this.gameObject);
                    StartCoroutine(DelayedRefreshCandidates());
                }
                else
                {
                    Debug.LogError("Error discarding candidate application: " + task.Exception);
                    discardButton.GetComponentInChildren<Text>().text = "DISCARD";
                }
            });
            confirmDiscard = false;
        }
    }

    private IEnumerator DelayedRefreshCandidates()
    {
        yield return new WaitForSeconds(0.5f);
        candidatesManager.ViewCandidates();
    }

    private void OnDownloadCVClicked()
    {
        StartCoroutine(DownloadCV(cvUrl));
    }

    // Method to download and open CV to device
    private IEnumerator DownloadCV(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                byte[] bytes = webRequest.downloadHandler.data;
                string filePath = Path.Combine(Application.persistentDataPath, userId + ".pdf");
                File.WriteAllBytes(filePath, bytes);
                Debug.Log("CV downloaded to: " + filePath);

                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}