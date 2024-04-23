using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script to allow messaging between users and recruiters initialisation only by users
public class ChatManager : MonoBehaviour
{
    private FirebaseFirestore db;

    public GameObject businessButtonPrefab;
    public Transform businessListParent;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadBoothsAndSetupBusinessList();
    }

    // Fetching only the recruiters that own a booth
    void LoadBoothsAndSetupBusinessList()
    {
        db.Collection("booths").WhereEqualTo("Taken", true).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching booths: " + task.Exception);
                return;
            }

            var snapshot = task.Result;
            foreach (var document in snapshot.Documents)
            {
                string userId = document.GetValue<string>("UserId");
                FetchBusinessUserInfo(userId);
            }
        });
    }

    // Fetching the businessName using the userId from booths
    void FetchBusinessUserInfo(string userId)
    {
        db.Collection("bus_users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching business user info: " + task.Exception);
                return;
            }

            var documentSnapshot = task.Result;
            if (documentSnapshot.Exists)
            {
                string businessName = documentSnapshot.GetValue<string>("businessName");
                CreateBusinessChatButton(businessName, userId);
            }
        });
    }

    // Using a prefab to populate a list of available recruiters to start chat
    void CreateBusinessChatButton(string businessName, string userId)
    {
        GameObject buttonObj = Instantiate(businessButtonPrefab, businessListParent);
        buttonObj.name = "Button_" + businessName;

        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = businessName;
        }

        Button button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(delegate { OnBusinessSelected(userId); });
    }

    // Initial method for correct selection
    void OnBusinessSelected(string businessUserId)
    {
        Debug.Log("Business with ID " + businessUserId + " was selected.");
    }
}