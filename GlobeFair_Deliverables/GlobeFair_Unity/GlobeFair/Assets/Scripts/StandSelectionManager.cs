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
    public Image boothDisplayImage;
    private Sprite defaultBoothImage;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        defaultBoothImage = boothDisplayImage.sprite;

        if (auth.CurrentUser != null)
        {
            InitializeOrRefreshCanvas();
        }
        else
        {
            Debug.LogError("No user is logged in.");
        }
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

    private void LoadSelectedStandId()
    {
        DocumentReference userDocRef = db.Collection("bus_users").Document(auth.CurrentUser.UserId);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                previouslySelectedStandId = snapshot.GetValue<string>("selectedStandId");
                Debug.Log("Loaded previously selected stand ID: " + previouslySelectedStandId);
                UpdateSelectedBoothImage(previouslySelectedStandId);

                string spriteName = previouslySelectedStandId;
                Sprite standSprite = Resources.Load<Sprite>("Stands/" + spriteName);
                if (standSprite != null)
                {
                    boothDisplayImage.sprite = standSprite;
                }
                else
                {
                    Debug.LogWarning("Stand sprite not found for: " + spriteName);
                }
            }
            else
            {
                Debug.LogError("User document does not exist.");
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
            DocumentReference previousStandRef = db.Collection("booths").Document(previouslySelectedStandId);
            var updateDataPrevious = new Dictionary<string, object>
        {
            { "Taken", false },
            { "UserId", FieldValue.Delete }
        };
            previousStandRef.UpdateAsync(updateDataPrevious).ContinueWithOnMainThread(previousUpdateTask =>
            {
                if (previousUpdateTask.IsFaulted || previousUpdateTask.IsCanceled)
                {
                    Debug.LogError("Error marking previous stand as not taken: " + previousUpdateTask.Exception);
                    return;
                }
                Debug.Log("Previous stand updated successfully.");
            });
        }

        DocumentReference newStandRef = db.Collection("booths").Document(standId);
        var updateDataNew = new Dictionary<string, object>
    {
        { "Taken", true },
        { "UserId", currentUser.UserId }
    };
        newStandRef.UpdateAsync(updateDataNew).ContinueWithOnMainThread(newStandUpdateTask =>
        {
            if (newStandUpdateTask.IsFaulted || newStandUpdateTask.IsCanceled)
            {
                Debug.LogError("Error marking new stand as taken: " + newStandUpdateTask.Exception);
                return;
            }
            Debug.Log("New stand selected and marked as taken: " + standId);

            DocumentReference userDocRef = db.Collection("bus_users").Document(currentUser.UserId);
            userDocRef.UpdateAsync("selectedStandId", standId).ContinueWithOnMainThread(userUpdateTask =>
            {
                if (userUpdateTask.IsFaulted || userUpdateTask.IsCanceled)
                {
                    Debug.LogError("Error updating user's selected stand ID: " + userUpdateTask.Exception);
                }
                else
                {
                    Debug.Log("User's selected stand ID updated in Firestore.");
                    previouslySelectedStandId = standId;
                    InitializeOrRefreshCanvas();
                }
            });
        });
    }

    public void UnselectStand()
    {
        var currentUser = auth.CurrentUser;
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        if (!string.IsNullOrEmpty(previouslySelectedStandId))
        {
            DocumentReference standRef = db.Collection("booths").Document(previouslySelectedStandId);
            var updateData = new Dictionary<string, object>
        {
            { "Taken", false },
            { "UserId", FieldValue.Delete }
        };
            standRef.UpdateAsync(updateData).ContinueWithOnMainThread(standUpdateTask =>
            {
                if (standUpdateTask.IsFaulted || standUpdateTask.IsCanceled)
                {
                    Debug.LogError("Error marking stand as not taken: " + standUpdateTask.Exception);
                    return;
                }
                Debug.Log("Stand marked as not taken successfully.");

                DocumentReference userDocRef = db.Collection("bus_users").Document(currentUser.UserId);
                userDocRef.UpdateAsync("selectedStandId", FieldValue.Delete).ContinueWithOnMainThread(userUpdateTask =>
                {
                    if (userUpdateTask.IsFaulted || userUpdateTask.IsCanceled)
                    {
                        Debug.LogError("Error removing user's selected stand ID: " + userUpdateTask.Exception);
                        return;
                    }
                    Debug.Log("User's selected stand ID removed from Firestore.");
                    previouslySelectedStandId = null;

                    boothDisplayImage.sprite = defaultBoothImage;
                    InitializeOrRefreshCanvas();
                });
            });
        }
    }


    private void UpdateSelectedBoothImage(string standId)
    {
        string spriteName = standId;
        Sprite standSprite = Resources.Load<Sprite>("Stands/" + spriteName);
        if (standSprite != null)
        {
            boothDisplayImage.sprite = standSprite;
        }
        else
        {
            Debug.LogWarning("Stand sprite not found for: " + spriteName);
        }
    }

    public void InitializeOrRefreshCanvas()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        ClearGrid();
        LoadSelectedStandId();
        PopulateGridWithStands();
    }

    public void RefreshAfterStandSelection(string standId)
    {
        previouslySelectedStandId = standId;
        InitializeOrRefreshCanvas();
    }

    private void ClearGrid()
    {
        foreach (Transform child in gridContent)
        {
            Destroy(child.gameObject);
        }
    }
}