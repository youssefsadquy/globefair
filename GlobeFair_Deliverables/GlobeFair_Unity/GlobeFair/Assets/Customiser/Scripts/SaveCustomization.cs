using UMA.CharacterSystem;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using System;

// Method that allows to save the custmisation into storage and save the download URL
// for each user gameState
public class SaveCustomization : MonoBehaviour
{
    public Button saveButton;
    public DynamicCharacterAvatar avatar;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    private FirebaseUser currentUser;

    void Awake()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveAvatarCustomization);
        }

        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
        currentUser = auth.CurrentUser;
    }

    // Using the current authentication state to save the customisation to storage and firebase
    public void SaveAvatarCustomization()
    {
        if (currentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string avatarDataString = avatar.GetAvatarDefinition(true).ToCompressedString("|");

        SaveAvatarDataToStorage(avatarDataString);
    }

    // Save the avatarDataString to Firebase Storage
    private void SaveAvatarDataToStorage(string avatarDataString)
    {
        var avatarDataRef = storage.GetReference($"umaData/{currentUser.UserId}.json");

        byte[] avatarDataBytes = System.Text.Encoding.UTF8.GetBytes(avatarDataString);

        avatarDataRef.PutBytesAsync(avatarDataBytes).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error saving avatar data to Storage: " + task.Exception);
                return;
            }

            avatarDataRef.GetDownloadUrlAsync().ContinueWithOnMainThread((urlTask) =>
            {
                if (urlTask.IsFaulted || urlTask.IsCanceled)
                {
                    Debug.LogError("Error getting avatar data download URL: " + urlTask.Exception);
                    return;
                }

                string downloadUrl = urlTask.Result.ToString();
                SaveDownloadUrlToFirestore(downloadUrl);
            });
        });
    }

    // then saving the URL in Firestore gameState
    private void SaveDownloadUrlToFirestore(string downloadUrl)
    {
        DocumentReference userDocRef = db.Collection("gameStates").Document(currentUser.UserId);
        userDocRef.UpdateAsync("playerCustomisation", downloadUrl).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error saving download URL to Firestore: " + task.Exception);
            }
            else
            {
                Debug.Log("Download URL saved to Firestore successfully.");
            }
        });
    }
}