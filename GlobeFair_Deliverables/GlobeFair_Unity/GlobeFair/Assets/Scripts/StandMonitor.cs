using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;

[FirestoreData]
public class StandData
{
    [FirestoreProperty]
    public string Information { get; set; }
}

public class StandMonitor : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject outlineObject;

    [SerializeField] private Transform playerTransform;

    [SerializeField] private float interactionDistance = 3.0f;

    [SerializeField] private Canvas standInfoCanvas;

    private FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        interactionPrompt.SetActive(false);
        standInfoCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        bool isPlayerClose = Vector3.Distance(playerTransform.position, transform.position) < interactionDistance;

        if (isPlayerClose)
        {
            outlineObject.SetActive(true);
            interactionPrompt.SetActive(true);

            if (Input.GetKeyDown(KeyCode.X))
            {
                FetchStandDataAndDisplayInformation();
            }
        }
        else
        {
            outlineObject.SetActive(false);
            interactionPrompt.SetActive(false);
        }
    }

    private void FetchStandDataAndDisplayInformation()
    {
        string standId = gameObject.tag.Replace("Stand_", "").Replace("_Monitor_1", "");
        DocumentReference standDocRef = db.Collection("booths").Document($"Stand_{standId}");
        standDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Error fetching data for stand {standId}: " + task.Exception);
                return;
            }
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                StandData standData = snapshot.ConvertTo<StandData>();
                DisplayInformation(standData.Information);
            }
            else
            {
                Debug.LogError($"Stand document for stand {standId} does not exist.");
            }
        });
    }

    private void DisplayInformation(string info)
    {
        Text infoText = standInfoCanvas.GetComponentInChildren<Text>();
        if (infoText != null)
        {
            infoText.text = info;
            standInfoCanvas.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Information Text component not found in StandInfoCanvas.");
        }
    }
}