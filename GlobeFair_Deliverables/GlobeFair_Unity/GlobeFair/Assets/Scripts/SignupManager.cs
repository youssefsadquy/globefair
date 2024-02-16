using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;

public class SignupManager : MonoBehaviour
{
    public InputField firstNameInputField;
    public InputField emailInputField;
    public InputField passwordInputField;
    public InputField confirmPasswordInputField;
    public Button signupButton;
    public Text feedbackText;
    public GameObject userDashboardCanvas;
    public GameObject signupCanvas;

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
                // Sign in the user
                UpdateFeedback("Signup successful! Logging in...");
                SignInUser(email, password);
            });
        });
    }

    private void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // User signed in successfully, redirect to user dashboard
            Debug.Log("User signed in successfully");
            ShowUserDashboard();
        });
    }

    private void ShowUserDashboard()
    {
        userDashboardCanvas.SetActive(true);
        signupCanvas.SetActive(false);
        ResetFieldsAndFeedback();
    }

    private void ResetFieldsAndFeedback()
    {
        // Reset input fields
        firstNameInputField.text = "";
        emailInputField.text = "";
        passwordInputField.text = "";
        confirmPasswordInputField.text = "";

        // Reset feedback message
        feedbackText.text = "";
    }

    private void UpdateFeedback(string message)
    {
        feedbackText.text = message;
    }

    void OnDestroy()
    {
        signupButton.onClick.RemoveListener(TrySignup);
    }
}
