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
    // UI references
    public InputField jobTitleInput;
    public InputField jobDescriptionInput;
    public InputField jobLocationInput;
    public InputField salaryInput;
    public InputField howToApplyInput;
    public Button submitButton;
    public Text feedbackText;
    public Scrollbar verticalScrollbar;

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
            if (feedbackText == null)
            {
                feedbackText = GameObject.FindGameObjectWithTag("FeedbackText1").GetComponent<Text>();
                if (feedbackText == null)
                {
                    Debug.LogError("Failed to find the FeedbackText1 object in the scene.");
                    return;
                }
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
            ShowFeedback("Please log in to create a listing.", false);
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
            {"businessUid", auth.CurrentUser.UserId},
            {"creationDate", Timestamp.GetCurrentTimestamp()}
        };

        docRef.SetAsync(newListing).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error adding listing: " + task.Exception);
                ShowFeedback("Failed to create listing. Please try again.", false);
            }
            else
            {
                Debug.Log("Listing added with ID: " + docRef.Id);
                ClearInputFields();
                ShowFeedback("Listing created successfully.", true);
            }
        });
    }

    private void ClearInputFields()
    {
        jobTitleInput.text = "";
        jobDescriptionInput.text = "";
        jobLocationInput.text = "";
        salaryInput.text = "";
        howToApplyInput.text = "";
    }

    private void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(5));
        }
        else
        {
            Debug.LogError("Feedback text object not found!");
        }
    }

    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
        ScrollToTop();
    }

    private void ScrollToTop()
    {
        if (verticalScrollbar)
        {
            verticalScrollbar.value = 1;
        }
    }
}