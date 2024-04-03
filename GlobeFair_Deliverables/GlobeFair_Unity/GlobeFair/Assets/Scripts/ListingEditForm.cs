using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections.Generic;

public class ListingEditForm : MonoBehaviour
{
    public InputField jobTitleInputField;
    public InputField jobDescriptionInputField;
    public InputField jobLocationInputField;
    public InputField salaryInputField;
    public InputField howToApplyInputField;

    public GameObject editFormCanvas;
    public LoadBusinessListings loadBusinessListingsScript;

    private string currentDocumentId;

    public static ListingEditForm Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        editFormCanvas.SetActive(false);
    }

    public void OpenEditForm(Dictionary<string, object> data, string documentId)
    {
        currentDocumentId = documentId;

        jobTitleInputField.text = data["jobTitle"] as string;
        jobDescriptionInputField.text = data["jobDescription"] as string;
        jobLocationInputField.text = data["location"] as string;
        salaryInputField.text = data["salary"] as string;
        howToApplyInputField.text = data["howToApply"] as string;

        editFormCanvas.SetActive(true);
    }

    public void SaveChanges()
    {
        var updatedData = new Dictionary<string, object>
        {
            {"jobTitle", jobTitleInputField.text},
            {"jobDescription", jobDescriptionInputField.text},
            {"location", jobLocationInputField.text},
            {"salary", salaryInputField.text},
            {"howToApply", howToApplyInputField.text},
        };

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("businessListings").Document(currentDocumentId);
        docRef.UpdateAsync(updatedData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error updating listing: " + task.Exception);
                loadBusinessListingsScript.ShowFeedback("Failed to update listing.", false);
            }
            else
            {
                Debug.Log("Listing updated successfully.");
                loadBusinessListingsScript.ShowFeedback("Listing updated successfully.", true);
                editFormCanvas.SetActive(false);
                loadBusinessListingsScript.LoadListings();
            }
        });
    }

    public void CancelEditing()
    {
        editFormCanvas.SetActive(false);
    }
}