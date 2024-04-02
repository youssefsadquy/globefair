using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CreateBusinessListing : MonoBehaviour
{
    public InputField jobTitleInput;
    public InputField jobDescriptionInput;
    public InputField jobLocationInput;
    public InputField salaryInput;
    public InputField howToApplyInput;
    public Button submitButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            db = FirebaseFirestore.DefaultInstance;

            auth = FirebaseAuth.DefaultInstance;

            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(CreateListing);
        });
    }

    void CreateListing()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No authenticated user found.");
            return;
        }

        DocumentReference docRef = db.Collection("businessListings").Document();
        Dictionary<string, object> newListing = new Dictionary<string, object>
        {
            {"jobTitle", jobTitleInput.text},
            {"jobDescription", jobDescriptionInput.text},
            {"location", jobLocationInput.text},
            {"salary", salaryInput.text},
            {"howToApply", howToApplyInput.text},
            {"businessUid", auth.CurrentUser.UserId}
        };

        docRef.SetAsync(newListing).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error adding listing: " + task.Exception);
            }
            else
            {
                Debug.Log("Listing added with ID: " + docRef.Id);
            }
        });
    }
}