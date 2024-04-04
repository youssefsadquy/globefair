using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using System.Collections;

public class BusinessBioManager : MonoBehaviour
{
    public InputField businessNameInputField;
    public InputField descriptionInputField;
    public InputField missionInputField;
    public InputField industryInputField;
    public InputField foundedInputField;
    public InputField employeesInputField;
    public InputField websiteInputField;
    public InputField contactEmailInputField;
    public InputField headquartersInputField;
    public Text feedbackText;
    public Scrollbar verticalScrollbar;

    public Button saveButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string businessUid;
    private DocumentReference businessDocRef;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            businessUid = auth.CurrentUser.UserId;
            DocumentReference businessDocRef = db.Collection("bus_users").Document(businessUid);
            saveButton.onClick.AddListener(SaveBusinessBio);
            LoadBusinessBio();
        }
        else
        {
            ShowFeedback("No authenticated user found.", false);
        }
    }

    void OnEnable()
    {
        auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            DocumentReference businessDocRef = db.Collection("bus_users").Document(businessUid);
            businessUid = auth.CurrentUser.UserId;
            LoadBusinessBio();
        }
        else
        {
            ShowFeedback("No authenticated user found.", false);
        }
    }

    public void LoadBusinessBio()
    {
        DocumentReference businessDocRef = db.Collection("bus_users").Document(businessUid);

        businessDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                ShowFeedback("Error loading business bio: " + task.Exception, false);
                return;
            }
            DocumentSnapshot snapshot = task.Result;
            businessNameInputField.text = snapshot.GetValue<string>("businessName");
            descriptionInputField.text = snapshot.GetValue<string>("description");
            missionInputField.text = snapshot.GetValue<string>("mission");
            industryInputField.text = snapshot.GetValue<string>("industry");
            foundedInputField.text = snapshot.GetValue<string>("founded");
            employeesInputField.text = snapshot.GetValue<string>("employees");
            websiteInputField.text = snapshot.GetValue<string>("website");
            contactEmailInputField.text = snapshot.GetValue<string>("contactEmail");
            headquartersInputField.text = snapshot.GetValue<string>("headquarters");
        });
    }

    void SaveBusinessBio()
    {
        DocumentReference businessDocRef = db.Collection("bus_users").Document(businessUid);

        var updatedBio = new Dictionary<string, object>
        {
            { "businessName", businessNameInputField.text },
            { "description", descriptionInputField.text },
            { "mission", missionInputField.text },
            { "industry", industryInputField.text },
            { "founded", foundedInputField.text },
            { "employees", employeesInputField.text },
            { "website", websiteInputField.text },
            { "contactEmail", contactEmailInputField.text },
            { "headquarters", headquartersInputField.text }
        };
    }
}
