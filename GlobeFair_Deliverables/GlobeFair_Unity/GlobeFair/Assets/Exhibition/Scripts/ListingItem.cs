using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

// Method attached to Listing prefab to populate the fields
public class ListingItem : MonoBehaviour
{
    [SerializeField] private Text jobTitleText;
    [SerializeField] private Text jobDescriptionText;
    [SerializeField] private Text locationText;
    [SerializeField] private Text salaryText;
    [SerializeField] private Text howToApplyText;
    [SerializeField] private Text creationDateText;
    [SerializeField] private Button applyButton;
    [SerializeField] private Text feedbackText;

    private FirebaseFirestore db;
    private string listingId;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        applyButton.onClick.AddListener(OnApplyClicked);
    }

    public void SetListingInfo(string jobTitle, string jobDescription, string location, string salary, string howToApply, string creationDate, string listingId)
    {
        jobTitleText.text = jobTitle;
        jobDescriptionText.text = jobDescription;
        locationText.text = location;
        salaryText.text = salary;
        howToApplyText.text = howToApply;
        creationDateText.text = creationDate;
        this.listingId = listingId;
    }

    // Method to check user type and send application with ApplyForJob()
    private void OnApplyClicked()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        // Check if it's a business user
        db.Collection("bus_users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                Debug.Log("Please create a private account to apply.");
                ShowFeedback("Please create a private account to apply.", false);
            }
            else
            {
                // Checking both collections of users
                db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
                {
                    if (userTask.Result.Exists)
                    {
                        ApplyForJob(userId);
                    }
                    else
                    {
                        Debug.Log("User not found in normal users either. Authentication issue.");
                        ShowFeedback("User not found in normal users either. Authentication issue.", false);
                    }
                });
            }
        });
    }

    // Method to send application and store in candidate collection
    private void ApplyForJob(string userId)
    {
        DocumentReference candidateDoc = db.Collection("candidates").Document();

        candidateDoc.SetAsync(new
        {
            userId = userId,
            listingId = this.listingId,
            applicationDate = Timestamp.GetCurrentTimestamp()
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Application sent successfully.");
                ShowFeedback("Application sent successfully.", true);
            }
            else
            {
                Debug.LogError("Error applying for the job: " + task.Exception);
                ShowFeedback("Error applying for the job: " + task.Exception, false);
            }
        });
    }

    // Method to display messages to user UI feedback field
    public void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(3));
        }
    }

    // Method to clear feedback
    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
}