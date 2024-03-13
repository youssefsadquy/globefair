using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class StandSelectionManager : MonoBehaviour
{
    public GameObject standButtonPrefab;
    public Transform gridContent;
    private FirebaseFirestore db;
    private FirebaseAuth auth;

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

                // Load the stand image from the Resources folder
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

        DocumentReference standRef = db.Collection("booths").Document(standId);
        standRef.UpdateAsync("Taken", true).ContinueWithOnMainThread(updateTask =>
        {
            if (updateTask.IsFaulted || updateTask.IsCanceled)
            {
                Debug.LogError("Error marking stand as taken: " + updateTask.Exception);
                return;
            }

            // Assign the stand to the current user in another field
            standRef.UpdateAsync("UserId", currentUser.UserId).ContinueWithOnMainThread(userAssignTask =>
            {
                if (userAssignTask.IsFaulted || userAssignTask.IsCanceled)
                {
                    Debug.LogError("Error assigning stand to user: " + userAssignTask.Exception);
                    return;
                }

                Debug.Log("Stand selected and marked as taken: " + standId);
            });
        });
    }
}
