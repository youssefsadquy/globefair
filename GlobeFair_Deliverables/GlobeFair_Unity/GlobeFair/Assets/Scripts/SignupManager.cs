using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class SignupManager : MonoBehaviour
{
    public TMP_InputField firstNameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_InputField confirmPasswordInputField;
    public Button signupButton;
    public TextMeshProUGUI feedbackText;
    public GameObject userDashboardCanvas;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    void Start()
    {
        signupButton.onClick.AddListener(TrySignup);
    }

    public void TrySignup()
    {
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;
        string firstName = firstNameInputField.text.Trim();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
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

            // Firebase user has been created then adding to Firestore
            FirebaseUser newUser = task.Result.User;
            DocumentReference docRef = db.Collection("users").Document(newUser.UserId);
            Dictionary<string, object> newUserMap = new Dictionary<string, object>
        {
            { "FirstName", firstName },
            { "Email", email }
        };

            docRef.SetAsync(newUserMap).ContinueWithOnMainThread(firestoreTask =>
            {
                if (firestoreTask.IsCanceled)
                {
                    UpdateFeedback("Failed to add user details to Firestore (canceled).");
                    return;
                }
                if (firestoreTask.IsFaulted)
                {
                    UpdateFeedback("Failed to add user details to Firestore: " + firestoreTask.Exception?.GetBaseException().Message);
                    return;
                }
                UpdateFeedback("Signup successful!");

                userDashboardCanvas.SetActive(true);
                gameObject.transform.parent.gameObject.SetActive(false);
            });

        });
    }


    private void UpdateFeedback(string message)
    {
        // Method for error messages
        feedbackText.text = message;
    }

    void OnDestroy()
    {
        signupButton.onClick.RemoveListener(TrySignup);
    }
}
