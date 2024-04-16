using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Linq;

// Main script for the management of the booth monitor, that allows storing of the tag
// and passing to relevant scripts for fetching information and populating fields, prefabs
public class FetchSetInformation : MonoBehaviour
{
    [SerializeField] private Text businessNameText;

    private FirebaseFirestore db;
    private string currentMonitorTag;

    private void OnEnable()
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

    // Storing the current monitor being interacted with
    public void SetCurrentMonitorTag(string tag)
    {
        currentMonitorTag = tag;
        Debug.Log("SetCurrentMonitorTag called with tag: " + currentMonitorTag);
        FetchAndDisplayBusinessInfo();
    }

    // Fethcing the company information to display on the first screen using the monitor tag
    private void FetchAndDisplayBusinessInfo()
    {
        if (string.IsNullOrEmpty(currentMonitorTag))
        {
            Debug.LogError("Current monitor tag not set.");
            return;
        }
        Debug.Log("Fetching info for: " + currentMonitorTag);

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
                        string businessName = userSnapshot.GetValue<string>("businessName");
                        UpdateUIWithBusinessName(businessName);
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

    // Updating the business name field
    private void UpdateUIWithBusinessName(string businessName)
    {
        businessNameText.text = businessName;
    }
}