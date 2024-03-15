using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections.Generic;

public class StandSelectionManager : MonoBehaviour
{
    public GameObject standButtonPrefab;
    public Transform gridContent;
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string previouslySelectedStandId = null;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        PopulateGridWithStands();
    }

    void PopulateGridWithStands()
    {
        db.Collection("booths").WhereEqualTo("Taken", false).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching stands: " + task.Exception);
                return;
            }

            foreach (DocumentSnapshot document in task.Result.Documents)
            {
                GameObject standButton = Instantiate(standButtonPrefab, gridContent);

                string imagePath = "Stands/" + document.Id;
                Sprite standSprite = Resources.Load<Sprite>(imagePath);
                if (standSprite != null)
                {
                    Image standImage = standButton.GetComponentInChildren<Image>();
                    standImage.sprite = standSprite;
                }
                else
                {
                    Debug.LogWarning("Stand image not found for: " + document.Id);
                }

                standButton.GetComponent<Button>().onClick.AddListener(() => SelectStand(document.Id));
            }
        });
    }

    void SelectStand(string standId)
    {
        var currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        if (!string.IsNullOrEmpty(previouslySelectedStandId) && previouslySelectedStandId != standId)
        {
            var previousStandRef = db.Collection("booths").Document(previouslySelectedStandId);
            var updateDataPrevious = new Dictionary<string, object>
        {
            { "Taken", false },
            { "UserId", FieldValue.Delete }
        };
            previousStandRef.UpdateAsync(updateDataPrevious).ContinueWithOnMainThread(previousUpdateTask =>
            {
                if (previousUpdateTask.IsFaulted || previousUpdateTask.IsCanceled)
                {
                    Debug.LogError("Error marking previous stand as taken: " + previousUpdateTask.Exception);
                }
                else
                {
                    Debug.Log("Previous stand updated successfully.");
                }
            });
        }

        DocumentReference standRef = db.Collection("booths").Document(standId);
        var updateDataNew = new Dictionary<string, object>
    {
        { "Taken", true },
        { "UserId", currentUser.UserId }
    };
        standRef.UpdateAsync(updateDataNew).ContinueWithOnMainThread(updateTask =>
        {
            if (updateTask.IsFaulted || updateTask.IsCanceled)
            {
                Debug.LogError("Error marking stand as taken: " + updateTask.Exception);
                return;
            }

            Debug.Log("Stand selected and marked as taken: " + standId);
            previouslySelectedStandId = standId;
        });
    }

}
