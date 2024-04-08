using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using System.Linq;
using UnityEngine.UI;

// Method to fetch business Q&As using current tag monitor/unique identifier
public class BusinessFAQFetcher : MonoBehaviour
{
    [SerializeField] private GameObject faqPrefab;
    [SerializeField] private Transform faqListParent;

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
                FetchAndDisplayFAQs(userId);
            }
            else
            {
                Debug.LogError($"Stand document does not exist: {standId}");
            }
        });
    }

    // Method to use the business ID to fetch their Q&As repository
    private void FetchAndDisplayFAQs(string userId)
    {
        db.Collection("businessQA").WhereEqualTo("businessUid", userId)
            .GetSnapshotAsync().ContinueWithOnMainThread(faqTask =>
            {
                if (faqTask.IsFaulted || faqTask.IsCanceled)
                {
                    Debug.LogError("Error fetching business QA data: " + faqTask.Exception);
                    return;
                }

                QuerySnapshot faqSnapshot = faqTask.Result;
                ClearFAQs();

                foreach (DocumentSnapshot document in faqSnapshot.Documents)
                {
                    if (document.Exists)
                    {
                        string question = document.GetValue<string>("question");
                        string answer = document.GetValue<string>("answer");

                        // Populating the parent with the prefabs
                        GameObject faqItem = Instantiate(faqPrefab, faqListParent);
                        faqItem.GetComponent<FAQItem>().SetQuestionAndAnswer(question, answer);
                    }
                    else
                    {
                        Debug.LogError("Document does not exist");
                    }
                }
            });
    }

    // Clearing parent content on fetching Q&As
    public void ClearFAQs()
    {
        foreach (Transform child in faqListParent)
        {
            Destroy(child.gameObject);
        }
    }
}