using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Collections;

// Script to fetch associated business booth data Total Visit and Unique visitors with
// total applicant number
public class BusinessAnalyticsDashboard : MonoBehaviour
{
    [SerializeField] private Text totalVisitsText;
    [SerializeField] private Text uniqueVisitorsText;
    [SerializeField] private Text totalApplicantsText;

    [SerializeField] private Text resetTotalVisitsButtonText;
    [SerializeField] private Text resetUniqueVisitorsButtonText;
    public Text feedbackText;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private bool confirmResetTotalVisits = false;
    private bool confirmResetUniqueVisitors = false;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        RefreshAnalyticsData();
    }

    // Clearing all fields
    public void ClearAnalyticsData()
    {
        totalVisitsText.text = "";
        uniqueVisitorsText.text = "";
        totalApplicantsText.text = "";
    }

    // Re fetching all the data
    public void RefreshAnalyticsData()
    {
        if (auth.CurrentUser != null)
        {
            string userId = auth.CurrentUser.UserId;
            FetchSelectedStandId(userId);
            FetchTotalApplicantsForBusiness(userId);
        }
        else
        {
            ShowFeedback("Authentication error.", false);
        }
    }

    // Method to get selected booth id from business user collection
    private void FetchSelectedStandId(string userId)
    {
        DocumentReference userDocRef = db.Collection("bus_users").Document(userId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user data: " + task.Exception);
                ShowFeedback("Failed to fetch data.", false);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.TryGetValue("selectedStandId", out string standId))
            {
                FetchStandAnalyticsData(standId);
            }
            else
            {
                Debug.LogError("Selected stand ID is not available.");
            }
        });
    }

    // Using the booth id to fetch 2 fields
    private void FetchStandAnalyticsData(string standId)
    {
        DocumentReference standDocRef = db.Collection("booths").Document(standId);
        standDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                ShowFeedback("Failed to fetch data.", false);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                totalVisitsText.text = snapshot.TryGetValue("totalVisits", out long totalVisits) ? totalVisits.ToString() : "0";
                uniqueVisitorsText.text = snapshot.TryGetValue("uniqueVisitors", out long uniqueVisitors) ? uniqueVisitors.ToString() : "0";
                ShowFeedback("Succesfully updated analytics.", true);
            }
            else
            {
                Debug.LogError("Stand document does not exist.");
                ShowFeedback("No data found. Have you selected a booth?", false);
            }
        });
    }

    // Using businessUid to identify all business listings then fetching all applications and counting
    private void FetchTotalApplicantsForBusiness(string businessId)
    {
        db.Collection("businessListings").WhereEqualTo("businessUid", businessId).GetSnapshotAsync().ContinueWithOnMainThread(listingsTask =>
        {
            if (listingsTask.IsFaulted || listingsTask.IsCanceled)
            {
                Debug.LogError("Error fetching listings: " + listingsTask.Exception);
                ShowFeedback("Failed to fetch data.", false);
                return;
            }

            List<string> listingIds = new List<string>();
            foreach (DocumentSnapshot listingSnapshot in listingsTask.Result.Documents)
            {
                listingIds.Add(listingSnapshot.Id);
            }

            if (listingIds.Count > 0)
            {
                db.Collection("candidates").WhereIn("listingId", listingIds).GetSnapshotAsync().ContinueWithOnMainThread(candidatesTask =>
                {
                    if (candidatesTask.IsFaulted || candidatesTask.IsCanceled)
                    {
                        Debug.LogError("Error fetching candidates: " + candidatesTask.Exception);
                        return;
                    }

                    // Set the total applicants count
                    totalApplicantsText.text = candidatesTask.Result.Count.ToString();
                    ShowFeedback("Successfully updated analytics.", true);
                });
            }
            else
            {
                // If there are no listings, set the applicants count to 0
                totalApplicantsText.text = "0";
                ShowFeedback("Successfully updated analytics.", true);
            }
        });
    }

    // Fetching the data for the user
    private void FetchSelectedStandIdForReset(string userId, string fieldToReset)
    {
        DocumentReference userDocRef = db.Collection("bus_users").Document(userId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.TryGetValue("selectedStandId", out string standId))
            {
                ResetAnalyticsField(standId, fieldToReset);
            }
            else
            {
                ShowFeedback("Failed to fetch data.", false);
            }
        });
    }

    // Resetting the data and refreshing the canvas
    private void ResetAnalyticsField(string standId, string fieldToReset)
    {
        DocumentReference standDocRef = db.Collection("booths").Document(standId);
        var update = new Dictionary<string, object>
    {
        { fieldToReset, 0 }
    };

        standDocRef.UpdateAsync(update).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Error resetting {fieldToReset}: " + task.Exception);
                return;
            }
            RefreshAnalyticsData();
        });
    }

    // Method that allows removal of total visits with confirmation
    public void ConfirmResetTotalVisits()
    {
        if (!confirmResetTotalVisits)
        {
            resetTotalVisitsButtonText.text = "SURE?";
            confirmResetTotalVisits = true;
        }
        else
        {
            if (auth.CurrentUser == null)
            {
                Debug.LogError("No user is logged in.");
                return;
            }

            string userId = auth.CurrentUser.UserId;
            FetchSelectedStandIdForReset(userId, "totalVisits");
            resetTotalVisitsButtonText.text = "RESET TOTAL VISITIS";
            ShowFeedback("Successfully reset total visitors data.", true);
            confirmResetTotalVisits = false;
        }
    }

    // Method that allows removal of unique visits with confirmation
    public void ConfirmResetUniqueVisitors()
    {
        if (!confirmResetUniqueVisitors)
        {
            resetUniqueVisitorsButtonText.text = "SURE?";
            confirmResetUniqueVisitors = true;
        }
        else
        {
            if (auth.CurrentUser == null)
            {
                Debug.LogError("No user is logged in.");
                return;
            }

            string userId = auth.CurrentUser.UserId;
            FetchSelectedStandIdForReset(userId, "uniqueVisitors");
            resetUniqueVisitorsButtonText.text = "RESET UNIQUE VISITORS";
            ShowFeedback("Successfully reset unique visitors data.", true);
            confirmResetUniqueVisitors = false;
        }
    }

    // Displaying conditional feedback
    public void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(3));
        }
        else
        {
            Debug.LogError("Feedback text object not found!");
        }
    }

    // Clearing after determined time
    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
}