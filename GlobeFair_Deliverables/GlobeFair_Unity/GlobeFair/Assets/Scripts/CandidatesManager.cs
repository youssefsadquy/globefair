using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class CandidatesManager : MonoBehaviour
{
    [SerializeField] private GameObject candidatePrefab;
    [SerializeField] private Transform candidatesContainer;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        LoadCandidatesForBusiness();
    }

    // Method to fetch all candidates for the listings
    // of the current business user
    private void LoadCandidatesForBusiness()
    {
        string businessId = auth.CurrentUser.UserId;

        db.Collection("businessListings").WhereEqualTo("businessUid", businessId).GetSnapshotAsync().ContinueWithOnMainThread(listingsTask =>
        {
            if (listingsTask.IsFaulted || listingsTask.IsCanceled)
            {
                Debug.LogError("Error fetching listings: " + listingsTask.Exception);
                return;
            }

            // Find all the listing IDs of the current business
            List<string> listingIds = new List<string>();
            foreach (DocumentSnapshot listingSnapshot in listingsTask.Result.Documents)
            {
                listingIds.Add(listingSnapshot.Id);
            }

            // Fetch candidates that applied to listings of the current business
            db.Collection("candidates").WhereIn("listingId", listingIds).GetSnapshotAsync().ContinueWithOnMainThread(candidatesTask =>
            {
                if (candidatesTask.IsFaulted || candidatesTask.IsCanceled)
                {
                    Debug.LogError("Error fetching candidates: " + candidatesTask.Exception);
                    return;
                }

                foreach (DocumentSnapshot candidateSnapshot in candidatesTask.Result.Documents)
                {
                    string userId = candidateSnapshot.GetValue<string>("userId");
                    string listingId = candidateSnapshot.GetValue<string>("listingId");
                    Timestamp applicationDate = candidateSnapshot.GetValue<Timestamp>("applicationDate");
                    string cvUrl = candidateSnapshot.GetValue<string>("cvUrl");

                    // Fetch user data for full name and email
                    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
                    {
                        if (userTask.IsFaulted || userTask.IsCanceled)
                        {
                            Debug.LogError("Error fetching user details: " + userTask.Exception);
                            return;
                        }

                        DocumentSnapshot userSnapshot = userTask.Result;
                        string firstName = userSnapshot.GetValue<string>("FirstName");
                        string email = userSnapshot.GetValue<string>("Email");

                        // Fetch listing data for listing title
                        db.Collection("businessListings").Document(listingId).GetSnapshotAsync().ContinueWithOnMainThread(listingTask =>
                        {
                            if (listingTask.IsFaulted || listingTask.IsCanceled)
                            {
                                Debug.LogError("Error fetching listing details: " + listingTask.Exception);
                                return;
                            }

                            DocumentSnapshot listingSnapshot = listingTask.Result;
                            string jobTitle = listingSnapshot.GetValue<string>("jobTitle");

                            // Create the prefab with all the components
                            GameObject candidateGO = Instantiate(candidatePrefab, candidatesContainer);
                            candidateGO.GetComponent<CandidateItem>().SetCandidateInfo(this, userId, candidateSnapshot.Id, firstName, email, jobTitle, applicationDate.ToDateTime().ToString("dd MMM yyyy HH:mm"), cvUrl);
                        });
                    });
                }
            });
        });
    }

    // On enter populating with candidates
    public void ViewCandidates()
    {
        ClearCandidatesContainer();
        LoadCandidatesForBusiness();
    }

    // On exit clearing the container
    public void ClearCandidatesContainer()
    {
        foreach (Transform child in candidatesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}