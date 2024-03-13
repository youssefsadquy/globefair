using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;

[FirestoreData]
public class BoothData
{
    [FirestoreProperty]
    public bool Taken { get; set; }
}

public class Booth : MonoBehaviour
{
    [SerializeField] private string boothId;
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        CreateOrUpdateBoothDocument();
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