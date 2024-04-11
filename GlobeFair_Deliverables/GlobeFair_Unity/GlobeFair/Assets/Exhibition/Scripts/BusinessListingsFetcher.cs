using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using System.Linq;
using UnityEngine.UI;

// Method to fetch business listings using current tag monitor/unique identifier
public class BusinessListingsFetcher : MonoBehaviour
{
    [SerializeField] private GameObject listingPrefab;
    [SerializeField] private Transform listingListParent;

    private FirebaseFirestore db;
    private string currentMonitorTag;

    void OnEnable()
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

    // Assigning the tag passed by "StandMonitor" script and calling the fetch method
    public void InitializeWithMonitorTag(string tag)
    {
        currentMonitorTag = tag;
        FetchUserIdFromStand();
    }

    // Method to use current monitor tag to fetch business unique identifier
    private void FetchUserIdFromStand()
    {
        if (string.IsNullOrEmpty(currentMonitorTag))
        {
            Debug.LogError("Current monitor tag not set.");
            return;
        }

        string standId = currentMonitorTag.Split('_').Take(2).Aggregate((current, next) => $"{current}_{next}");

        // Fetching the UserId based on the stand ID
        db.Collection("booths").Document(standId).GetSnapshotAsync().ContinueWithOnMainThread(standTask =>
        {
            if (standTask.IsFaulted || standTask.IsCanceled)
            {
                Debug.LogError("Error fetching stand data: " + standTask.Exception);
                return;
            }

            DocumentSnapshot standSnapshot = standTask.Result;
            if (standSnapshot.Exists)
            {
                string userId = standSnapshot.GetValue<string>("UserId");
                FetchAndDisplayListings(userId);
            }
            else
            {
                Debug.LogError($"Stand document does not exist: {standId}");
            }
        });
    }

    // Method to use the business ID to fetch their listings repository
    private void FetchAndDisplayListings(string userId)
    {
        db.Collection("businessListings").WhereEqualTo("businessUid", userId)
            .GetSnapshotAsync().ContinueWithOnMainThread(listingsTask =>
            {
                if (listingsTask.IsFaulted || listingsTask.IsCanceled)
                {
                    Debug.LogError("Error fetching business listings data: " + listingsTask.Exception);
                    return;
                }

                QuerySnapshot listingsSnapshot = listingsTask.Result;
                ClearListings();

                foreach (DocumentSnapshot document in listingsSnapshot.Documents)
                {
                    if (document.Exists)
                    {
                        string jobTitle = document.GetValue<string>("jobTitle");
                        string jobDescription = document.GetValue<string>("jobDescription");
                        string location = document.GetValue<string>("location");
                        string salary = document.GetValue<string>("salary");
                        string howToApply = document.GetValue<string>("howToApply");
                        Timestamp creationDate = document.GetValue<Timestamp>("creationDate");

                        // Populating the parent with the prefabs
                        GameObject listingItem = Instantiate(listingPrefab, listingListParent);
                        ListingItem listingItemScript = listingItem.GetComponent<ListingItem>();
                        if (listingItemScript != null)
                        {
                            listingItemScript.SetListingInfo(jobTitle, jobDescription, location, salary, howToApply, creationDate.ToDateTime().ToString(), document.Id);
                        }
                        else
                        {
                            Debug.LogError("ListingItem script not found on instantiated prefab");
                        }
                    }
                    else
                    {
                        Debug.LogError("Document does not exist");
                    }
                }
            });
    }

    // Clearing parent content on fetching listings
    public void ClearListings()
    {
        foreach (Transform child in listingListParent)
        {
            Destroy(child.gameObject);
        }
    }
}