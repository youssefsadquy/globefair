using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Auth;

public class LoadBusinessListings : MonoBehaviour
{
    public GameObject listingPrefab;
    public Transform contentPanel;
    private FirebaseFirestore db;
    private string businessUid;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            businessUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            LoadListings();
        }
        else
        {
            Debug.LogError("No authenticated user found.");
        }
    }

    private void OnEnable()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            businessUid = auth.CurrentUser.UserId;
            LoadListings();
        }
        else
        {
            Debug.LogError("No authenticated user found.");
        }
    }

    public void LoadListings()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        db.Collection("businessListings").WhereEqualTo("businessUid", businessUid)
            .GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error getting listings: " + task.Exception);
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot documentSnapshot in snapshot.Documents)
                {
                    GameObject listingInstance = Instantiate(listingPrefab, contentPanel);
                    PopulateListing(listingInstance, documentSnapshot);
                }
            });
    }

    private void PopulateListing(GameObject listingInstance, DocumentSnapshot documentSnapshot)
    {
        ListingPrefabScript listingScript = listingInstance.GetComponent<ListingPrefabScript>();

        if (documentSnapshot.Exists)
        {
            Dictionary<string, object> listingData = documentSnapshot.ToDictionary();
            listingScript.Setup(listingData);
        }
        Canvas.ForceUpdateCanvases();
    }
}
