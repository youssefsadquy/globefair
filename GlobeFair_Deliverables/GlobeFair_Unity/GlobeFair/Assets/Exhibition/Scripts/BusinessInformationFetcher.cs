using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Linq;

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

    void Start()
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

    public void InitializeWithMonitorTag(string tag)
    {
        currentMonitorTag = tag;
        FetchAndDisplayBusinessInformation();
    }

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
    }

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