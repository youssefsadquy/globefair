using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using System.Collections;
using UnityEngine.Networking;

[FirestoreData]
public class BoothData
{
    [FirestoreProperty]
    public bool Taken { get; set; }

    [FirestoreProperty]
    public string LogoUrl { get; set; }
}

public class Booth : MonoBehaviour
{
    [SerializeField] private string boothId;
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        FetchBoothDataAndApplyLogo();
    }

    private void CreateOrUpdateBoothDocument()
    {
        DocumentReference boothDocRef = db.Collection("booths").Document(boothId);
        boothDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error: " + task.Exception.ToString());
                return;
            }

            if (!task.Result.Exists)
            {
                BoothData newBooth = new BoothData { Taken = false };
                boothDocRef.SetAsync(newBooth).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Error creating document: " + task.Exception.ToString());
                    }
                    else
                    {
                        Debug.Log("Booth document created successfully.");
                    }
                });
            }
            else
            {
                Debug.Log("Booth document already exists.");
            }
        });
    }

    private void FetchBoothDataAndApplyLogo()
    {
        DocumentReference boothDocRef = db.Collection("booths").Document(boothId);
        boothDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Error fetching booth data for booth {boothId}: " + task.Exception);
                return;
            }
            DocumentSnapshot boothSnapshot = task.Result;
            if (boothSnapshot.Exists)
            {
                string boothUserId = boothSnapshot.GetValue<string>("UserId");
                if (!string.IsNullOrEmpty(boothUserId))
                {
                    DocumentReference busUserRef = db.Collection("bus_users").Document(boothUserId);
                    busUserRef.GetSnapshotAsync().ContinueWithOnMainThread(busUserTask =>
                    {
                        if (busUserTask.IsFaulted || busUserTask.IsCanceled)
                        {
                            Debug.LogError($"Error fetching business user data for user {boothUserId}: " + busUserTask.Exception);
                            return;
                        }
                        DocumentSnapshot busUserSnapshot = busUserTask.Result;
                        if (busUserSnapshot.Exists)
                        {
                            string logoUrl = busUserSnapshot.GetValue<string>("logo");
                            if (!string.IsNullOrEmpty(logoUrl))
                            {
                                StartCoroutine(DownloadAndApplyLogo(logoUrl));
                            }
                            else
                            {
                                Debug.Log($"No logo URL for business user {boothUserId}");
                            }
                        }
                        else
                        {
                            Debug.Log($"Business user document for user {boothUserId} does not exist.");
                        }
                    });
                }
                else
                {
                    Debug.Log($"No UserId associated with booth {boothId}");
                }
            }
            else
            {
                Debug.Log($"Booth document for booth {boothId} does not exist.");
            }
        });
    }


    IEnumerator DownloadAndApplyLogo(string logoUrl)
    {
        Debug.Log($"Fetching logo from URL: {logoUrl} for booth {boothId}");

        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(logoUrl);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
            Debug.Log($"Logo downloaded successfully for booth {boothId}");

            GameObject stand = GameObject.FindWithTag(boothId);
            GameObject logoObject = GameObject.FindWithTag($"{boothId}_Logo_1");

            if (stand != null && logoObject != null)
            {
                Renderer logoRenderer = logoObject.GetComponent<Renderer>();
                if (logoRenderer != null)
                {
                    Debug.Log($"Renderer found for booth {boothId}, applying texture");
                    logoRenderer.material.mainTexture = texture;
                }
                else
                {
                    Debug.LogError($"Renderer component not found for logo object with tag {logoObject.tag}");
                }
                GameObject logoObject2 = GameObject.FindWithTag($"{boothId}_Logo_2");
                if (logoObject2 != null)
                {
                    Renderer logoRenderer2 = logoObject2.GetComponent<Renderer>();
                    if (logoRenderer2 != null)
                    {
                        Debug.Log($"Renderer found for booth {boothId} logo 2, applying texture");
                        logoRenderer2.material.mainTexture = texture;
                    }
                    else
                    {
                        Debug.LogError($"Renderer component not found for logo object 2 with tag {logoObject2.tag}");
                    }
                }
                GameObject logoObject3 = GameObject.FindWithTag($"{boothId}_Logo_3");
                if (logoObject3 != null)
                {
                    Renderer logoRenderer3 = logoObject3.GetComponent<Renderer>();
                    if (logoRenderer3 != null)
                    {
                        Debug.Log($"Renderer found for booth {boothId} logo 3, applying texture");
                        logoRenderer3.material.mainTexture = texture;
                    }
                    else
                    {
                        Debug.LogError($"Renderer component not found for logo object 3 with tag {logoObject2.tag}");
                    }
                }
            }
            else
            {
                if (stand == null)
                    Debug.LogError($"Stand object with tag {boothId} not found in the scene.");
                if (logoObject == null)
                    Debug.LogError($"Logo object with tag {boothId}_Logo_1 not found in the scene.");
            }
        }
        else
        {
            Debug.LogError($"Error downloading logo texture: {uwr.error}");
        }
    }



    public void SetBoothAvailability(bool isAvailable)
    {
        DocumentReference boothDocRef = db.Collection("booths").Document(boothId);
        boothDocRef.UpdateAsync("Taken", !isAvailable).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error updating document: " + task.Exception.ToString());
            }
            else
            {
                Debug.Log($"Booth {boothId} availability set to {!isAvailable}.");
            }
        });
    }
}