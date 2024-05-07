using UMA.CharacterSystem;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using System;
using System.Collections;

// Method that allows to save the custmisation into storage and save the download URL
// for each user gameState
public class SaveCustomization : MonoBehaviour
{
    public Button saveButton;
    public DynamicCharacterAvatar avatar;
    public Text feedbackText;

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
            ShowFeedback("No user is logged in.", false);
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
                ShowFeedback("Failed to save avatar data.", false);
                return;
            }

            avatarDataRef.GetDownloadUrlAsync().ContinueWithOnMainThread((urlTask) =>
            {
                if (urlTask.IsFaulted || urlTask.IsCanceled)
                {
                    Debug.LogError("Error getting avatar data download URL: " + urlTask.Exception);
                    ShowFeedback("Failed to retrieve customisation.", false);
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
        userDocRef.UpdateAsync("playerCustomisation", downloadUrl).ContinueWithOnMainThread((updateTask) =>
        {
            if (updateTask.IsFaulted || updateTask.IsCanceled)
            {
                // If the update fails, create the document instead
                var exception = updateTask.Exception.Flatten().InnerException as FirestoreException;
                if (exception != null && exception.ErrorCode == FirestoreError.NotFound)
                {
                    userDocRef.SetAsync(new { playerCustomisation = downloadUrl }).ContinueWithOnMainThread((setTask) =>
                    {
                        if (setTask.IsFaulted || setTask.IsCanceled)
                        {
                            Debug.LogError("Error creating new gameState in Firestore: " + setTask.Exception);
                            ShowFeedback("Failed to create customisation.", false);
                        }
                        else
                        {
                            Debug.Log("GameState created and customisation saved successfully.");
                            ShowFeedback("Customization saved successfully.", true);
                        }
                    });
                }
                else
                {
                    Debug.LogError("Error saving download URL to Firestore: " + updateTask.Exception);
                    ShowFeedback("Failed to save customisation.", false);
                }
            }
            else
            {
                Debug.Log("Download URL saved to Firestore successfully.");
                ShowFeedback("Customization saved successfully.", true);
            }
        });
    }

    // Method to display messages to user UI feedback field
    public void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(3));
        }
    }

    // Method to clear feedback
    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
}