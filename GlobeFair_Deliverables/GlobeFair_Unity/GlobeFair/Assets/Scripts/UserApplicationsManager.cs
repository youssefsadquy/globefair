using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

// Script that allows users to fetch all of their applications using prefabs
public class UserApplicationsManager : MonoBehaviour
{
    [SerializeField] private GameObject applicationPrefab;
    [SerializeField] private Transform applicationsContainer;
    [SerializeField] private GameObject noApplicationsPrefab;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        LoadUserApplications();
    }

    // Method to fetch all the listings applied for the user
    private void LoadUserApplications()
    {
        string userId = auth.CurrentUser.UserId;

        // Fetch applications
        db.Collection("candidates").WhereEqualTo("userId", userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user applications: " + task.Exception);
                return;
            }

            // No applications found
            var documents = task.Result.Documents;
            if (!documents.Any())
            {
                Instantiate(noApplicationsPrefab, applicationsContainer);
                return;
            }

            foreach (DocumentSnapshot applicationSnapshot in task.Result.Documents)
            {
                string listingId = applicationSnapshot.GetValue<string>("listingId");
                Timestamp applicationDate = applicationSnapshot.GetValue<Timestamp>("applicationDate");

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
                    string businessUid = listingSnapshot.GetValue<string>("businessUid");

                    // Fetch the business details name, email
                    db.Collection("bus_users").Document(businessUid).GetSnapshotAsync().ContinueWithOnMainThread(businessUserTask =>
                    {
                        if (businessUserTask.IsFaulted || businessUserTask.IsCanceled)
                        {
                            Debug.LogError("Error fetching business user details: " + businessUserTask.Exception);
                            return;
                        }

                        DocumentSnapshot businessUserSnapshot = businessUserTask.Result;
                        string businessName = businessUserSnapshot.GetValue<string>("businessName");
                        string contactEmail = businessUserSnapshot.GetValue<string>("contactEmail");

                        // Create the prefab with all the components
                        GameObject applicationGO = Instantiate(applicationPrefab, applicationsContainer);
                        applicationGO.GetComponent<ApplicationItem>().SetApplicationInfo(
                            this,
                            applicationSnapshot.Id,
                            listingId,
                            applicationDate.ToDateTime().ToString("dd MMM yyyy HH:mm"),
                            jobTitle,
                            businessName,
                            contactEmail
                        );
                    });
                });
            }
        });
    }

    // On enter populating with applications
    public void ViewApplications()
    {
        ClearApplicationsContainer();
        LoadUserApplications();
    }
    // On exit clearing the container
    public void ClearApplicationsContainer()
    {
        foreach (Transform child in applicationsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}