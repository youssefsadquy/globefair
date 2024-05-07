using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.Networking;

// Script that allows the management of booths, business selection, application on UI
// with selection/deselection and standard unselected pictures
public class StandSelectionManager : MonoBehaviour
{
    public GameObject standButtonPrefab;
    public Transform gridContent;
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string previouslySelectedStandId = null;
    public Image boothDisplayImage;
    private Sprite defaultBoothImage;
    private FirebaseUser currentUser;
    private bool isGridInitialized = false;
    public Image logoImage;
    public Sprite defaultLogoSprite;

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

    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        defaultBoothImage = boothDisplayImage.sprite;

        auth.StateChanged += AuthStateChanged;
    }

    void OnEnable()
    {
        if (auth.CurrentUser != null)
        {
            LoadBusinessLogo();
            InitializeOrRefreshCanvas();
        }
    }

    // Grid to display only the non taken booths fetched from firebase
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

    // Fetching the business's booth selected and display on the UI for reference to the user
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

    // Method that updates the UI and firebase user/booth document to update the selection
    void SelectStand(string standId)
    {
        isGridInitialized = false;
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
            { "UserId", FieldValue.Delete },
            { "uniqueVisitors", 0 },
            { "totalVisits", 0 }
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

    // Method to unselect the booth reset fields on booths and remove from current business
    public void UnselectStand()
    {
        isGridInitialized = false;
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
            { "UserId", FieldValue.Delete },
            { "uniqueVisitors", 0 },
            { "totalVisits", 0 }
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

    // Fetching images for each booth
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

    // Method to refresh the canvas for every action for a "live" interaction
    public void InitializeOrRefreshCanvas()
    {
        if (isGridInitialized)
        {
            return;
        }
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }
        if (auth.CurrentUser != null)
        {
            LoadBusinessLogo();
        }

        ClearGrid();
        LoadSelectedStandId();
        PopulateGridWithStands();
        isGridInitialized = true;
    }

    public void RefreshAfterStandSelection(string standId)
    {
        previouslySelectedStandId = standId;
        InitializeOrRefreshCanvas();
    }

    // Emptying the grid for a clean initilisation
    private void ClearGrid()
    {
        foreach (Transform child in gridContent)
        {
            Destroy(child.gameObject);
        }
    }

    // Method that checks if the user has changed and refreshes the information
    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            currentUser = auth.CurrentUser;

            if (currentUser != null)
            {
                isGridInitialized = false;
                InitializeOrRefreshCanvas();
            }
            else
            {
                previouslySelectedStandId = null;
                boothDisplayImage.sprite = defaultBoothImage;
                ClearGrid();
                isGridInitialized = false;
            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
    }

    // Method that allows to display the logo previously uploaded by the user on the UI
    // OR displaying placeholder
    public void LoadBusinessLogo()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("LoadBusinessLogo called but no user is logged in.");
            return;
        }

        DocumentReference userDocRef = db.Collection("bus_users").Document(auth.CurrentUser.UserId);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists && snapshot.ContainsField("logo"))
            {
                string logoUrl = snapshot.GetValue<string>("logo");
                if (!string.IsNullOrEmpty(logoUrl))
                {
                    StartCoroutine(DownloadAndSetLogoImage(logoUrl));
                }
                else
                {
                    ResetToDefaultLogo();
                }
            }
            else
            {
                Debug.LogError("User document does not exist or does not have a logo.");
                ResetToDefaultLogo();
            }
        });
    }

    private void ResetToDefaultLogo()
    {
        logoImage.sprite = defaultLogoSprite;
    }

    // Method to fetch the logo and download it for applcation
    private IEnumerator DownloadAndSetLogoImage(string logoUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(logoUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            logoImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            logoImage.preserveAspect = true;
        }
        else
        {
            Debug.LogError("Error downloading the logo image: " + www.error);
        }
    }
}