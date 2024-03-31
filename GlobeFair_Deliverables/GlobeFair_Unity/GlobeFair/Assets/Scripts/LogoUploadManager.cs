using Firebase;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Firestore;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using UnityEngine.UI;
using System;
using Firebase.Extensions;
using System.Collections;
using UnityEngine.Networking;
using SFB;

public class LogoUploadManager : MonoBehaviour
{
    public Button logoUploadButton;
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    private FirebaseUser currentUser;
    private string userBoothId;
    private string downloadUrl;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        currentUser = auth.CurrentUser;
        storage = FirebaseStorage.DefaultInstance;

        if (currentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }
        RetrieveUserBoothId();
    }

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
        currentUser = auth.CurrentUser;

        logoUploadButton.onClick.AddListener(() => StartCoroutine(GetFilePathFromUser()));
        RetrieveUserBoothId();
    }

    System.Collections.IEnumerator GetFilePathFromUser()
    {
        // Standalone File Browser
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Logo", "", extensions, false);
        if (paths.Length > 0)
        {
            string filePath = paths[0];
            if (!string.IsNullOrEmpty(filePath))
            {
                UploadLogo(filePath);
            }
        }
        yield return null;
    }

    void RetrieveUserBoothId()
    {
        db.Collection("booths").WhereEqualTo("UserId", currentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error getting booths: " + task.Exception);
                return;
            }

            var documents = task.Result.Documents;
            if (documents.Count() > 0)
            {
                var boothDocument = documents.First();
                userBoothId = boothDocument.Id;
                Debug.Log("Booth assignment found: " + userBoothId);
            }
            else
            {
                Debug.LogWarning("No booth assigned to the current user.");
            }
        });
    }

    void UploadLogo(string filePath)
    {
        Debug.Log($"Attempting to upload logo from path: {filePath}");
        RetrieveUserBoothId();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found at path: {filePath}");
            return;
        }

        byte[] fileContents = File.ReadAllBytes(filePath);
        if (fileContents == null || fileContents.Length == 0)
        {
            Debug.LogError($"Failed to read file contents or file is empty at path: {filePath}");
            return;
        }

        string fileName = Path.GetFileName(filePath);
        Debug.Log($"Filename extracted: {fileName}");

        try
        {
            var storageRef = FirebaseStorage.DefaultInstance.GetReference($"user_logos/{currentUser.UserId}/{fileName}");
            Debug.Log($"Storage reference created: {storageRef.Path}");

            storageRef.PutBytesAsync(fileContents).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error uploading file: " + task.Exception);
                    return;
                }

                Debug.Log("Upload successful, attempting to get download URL.");
                storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(downloadTask =>
                {
                    if (downloadTask.IsFaulted || downloadTask.IsCanceled)
                    {
                        Debug.LogError("Error getting download URL: " + downloadTask.Exception);
                        return;
                    }

                    downloadUrl = downloadTask.Result.ToString();
                    Debug.Log($"File available at: {downloadUrl}");

                    SaveLogoUrlToFirestore(downloadUrl);
                });
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while attempting to upload logo: {ex.Message}");
        }
    }


    void SaveLogoUrlToFirestore(string logoUrl)
    {
        var busUserRef = db.Collection("bus_users").Document(currentUser.UserId);
        busUserRef.UpdateAsync("logo", logoUrl).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error updating Firestore with logo URL: " + task.Exception);
                return;
            }

            Debug.Log("Firestore updated with logo URL for business user: " + currentUser.UserId);
        });
    }

    void ApplyLogoToBoothMaterial(string logoUrl)
    {
        GameObject boothObject = GameObject.FindWithTag(userBoothId);

        if (boothObject == null)
        {
            Debug.LogError($"Booth GameObject with tag '{userBoothId}' not found in the scene.");
            return;
        }
        string boothNumber = userBoothId.Split('_')[1];
        string logoTag = $"{userBoothId}_Logo_{boothNumber}";

        GameObject logoObject = GameObject.FindWithTag(logoTag);

        if (logoObject == null)
        {
            Debug.LogError($"Booth logo GameObject with tag '{logoTag}' not found.");
            return;
        }

        MeshRenderer logoRenderer = logoObject.GetComponent<MeshRenderer>();
        if (logoRenderer == null)
        {
            Debug.LogError($"MeshRenderer component not found on the logo GameObject with tag '{logoTag}'.");
            return;
        }

        StartCoroutine(DownloadAndApplyTexture(logoUrl, logoRenderer));
    }


    System.Collections.IEnumerator DownloadAndApplyTexture(string textureUrl, MeshRenderer renderer)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(textureUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading texture: " + uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                Material[] materials = renderer.materials;
                materials[0].mainTexture = texture;
                renderer.materials = materials;
            }
        }
    }
}