using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using Firebase.Extensions;

// Script that allows to fetch and populate preafabs for Q&As
public class QAPrefabScript : MonoBehaviour
{
    public Text questionText;
    public Text answerText;
    public Button editButton;
    public Button removeButton;

    private string documentId;
    private BusinessQAManager qaManager;

    // Using the QAmanager for editing, while the other for populating the 2 prefab fields
    public void Setup(DocumentSnapshot snapshot, BusinessQAManager manager)
    {
        qaManager = manager;
        documentId = snapshot.Id;
        questionText.text = snapshot.GetValue<string>("question");
        answerText.text = snapshot.GetValue<string>("answer");
        editButton.onClick.AddListener(() => qaManager.PopulateQAForEditing(documentId));
        removeButton.onClick.AddListener(DeleteQA);
    }

    // Button that uses the QA id to delete it from firebase
    private void DeleteQA()
    {
        if (!string.IsNullOrEmpty(documentId))
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            db.Collection("businessQA").Document(documentId).DeleteAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error deleting Q&A: " + task.Exception);
                    qaManager.ShowFeedback("Failed to delete Q&A.", false);
                }
                else
                {
                    Debug.Log("Q&A deleted successfully.");
                    qaManager.ShowFeedback("Q&A deleted successfully.", true);
                    Destroy(gameObject);
                }
            });
        }
        else
        {
            Debug.LogError("Document ID is null or empty.");
            qaManager.ShowFeedback("Cannot delete: No Q&A selected.", false);
        }
    }

    private void OnDestroy()
    {
        editButton.onClick.RemoveListener(() => qaManager.PopulateQAForEditing(documentId));
    }
}