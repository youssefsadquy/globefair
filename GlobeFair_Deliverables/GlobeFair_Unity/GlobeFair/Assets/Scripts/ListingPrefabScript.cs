using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using Firebase.Extensions;

public class ListingPrefabScript : MonoBehaviour
{
    public Text jobTitleText;
    public Text jobLocationText;
    public Text salaryText;
    public Text creationDateText;
    private LoadBusinessListings loadBusinessListingsScript;

    public string documentId;
    private void Start()
    {
        loadBusinessListingsScript = FindObjectOfType<LoadBusinessListings>();
    }

    public void Setup(Dictionary<string, object> data, string docId)
    {
        Debug.Log("Setting up listing with data: " + data.ToString());
        documentId = docId;

        if (data.TryGetValue("jobTitle", out object jobTitle))
        {
            Debug.Log("Job Title: " + (jobTitle as string));
            jobTitleText.text = jobTitle as string;
        }

        if (data.TryGetValue("creationDate", out object creationDate))
        {
            if (creationDate is Timestamp timestamp)
            {
                DateTime dateTime = timestamp.ToDateTime();
                string dateString = dateTime.ToString("dd-MM-yyyy HH:mm:ss");
                Debug.Log("Creation Date: " + dateString);
                creationDateText.text = dateString;
            }
        }

        if (data.TryGetValue("location", out object jobLocation))
        {
            Debug.Log("Location: " + (jobLocation as string));
            jobLocationText.text = jobLocation as string;
        }

        if (data.TryGetValue("salary", out object salary))
        {
            Debug.Log("Salary: " + (salary as string));
            salaryText.text = salary as string;
        }
    }

    public void DeleteListing()
    {
        if (!string.IsNullOrEmpty(documentId))
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            db.Collection("businessListings").Document(documentId).DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error deleting listing: " + task.Exception);
                    loadBusinessListingsScript?.ShowFeedback("Failed to delete listing.", false);
                }
                else
                {
                    Debug.Log("Listing deleted successfully.");
                    loadBusinessListingsScript?.ShowFeedback("Listing deleted successfully.", true);
                    Destroy(gameObject);
                }
            });
        }
        else
        {
            Debug.LogError("Document ID is null or empty.");
            loadBusinessListingsScript?.ShowFeedback("Cannot delete: No listing selected.", false);
        }
    }
}