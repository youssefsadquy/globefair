using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SignupBusinessManager : MonoBehaviour
{
    public TMP_InputField businessNameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField confirmPasswordInputField;
    public Button signupButton;
    public TextMeshProUGUI feedbackText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    void Start()
    {
        signupButton.onClick.AddListener(TryBusinessSignup);
    }

    public void TryBusinessSignup()
    {
        string businessName = businessNameInputField.text.Trim();
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;

        if (string.IsNullOrEmpty(businessName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateFeedback("All fields are required.");
            return;
        }

        if (password != confirmPassword)
        {
            UpdateFeedback("Passwords do not match.");
            return;
        }

        // Firebase Authentication sign-up
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                UpdateFeedback("Signup was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UpdateFeedback("Signup encountered an error: " + task.Exception?.GetBaseException().Message);
                return;
            }

            // Firebase user created then add to Firestore
            FirebaseUser newBusinessUser = task.Result.User;
            DocumentReference docRef = db.Collection("bus_users").Document(newBusinessUser.UserId);
            Dictionary<string, object> businessUserData = new Dictionary<string, object>
            {
                { "BusinessName", businessName },
                { "Email", email }
            };

            docRef.SetAsync(businessUserData).ContinueWithOnMainThread(firestoreTask =>
            {
                if (firestoreTask.IsCanceled)
                {
                    UpdateFeedback("Failed to add business user details to Firestore (canceled).");
                    return;
                }
                if (firestoreTask.IsFaulted)
                {
                    UpdateFeedback("Failed to add business user details to Firestore: " + firestoreTask.Exception?.GetBaseException().Message);
                    return;
                }
                UpdateFeedback("Business signup successful!");
            });
        });
    }

    private void UpdateFeedback(string message)
    {
        feedbackText.text = message;
    }

    void OnDestroy()
    {
        signupButton.onClick.RemoveListener(TryBusinessSignup);
    }
}
