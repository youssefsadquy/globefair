using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class LoginManager : MonoBehaviour
{
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    public Text feedbackText;
    public GameObject userDashboardCanvas;
    public GameObject businessDashboardCanvas;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    void Start()
    {
        loginButton.onClick.AddListener(LoginWithEmailPassword);
    }

    private void LoginWithEmailPassword()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and Password are required.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                feedbackText.text = "SignInWithEmailAndPasswordAsync was canceled.";
                return;
            }
            if (task.IsFaulted)
            {
                feedbackText.text = "SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception;
                return;
            }

            // Firebase user signed in successfully
            CheckUserCollection(task.Result.User);
        });
    }

    private void CheckUserCollection(FirebaseUser user)
    {
        DocumentReference userDocRef = db.Collection("users").Document(user.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
        {
            if (userTask.IsFaulted || userTask.IsCanceled)
            {
                feedbackText.text = "Error getting user document: " + userTask.Exception;
            }
            else if (userTask.Result.Exists)
            {
                ShowDashboard(userDashboardCanvas);
            }
            else
            {
                DocumentReference busUserDocRef = db.Collection("bus_users").Document(user.UserId);
                busUserDocRef.GetSnapshotAsync().ContinueWithOnMainThread(busUserTask =>
                {
                    if (busUserTask.IsFaulted || busUserTask.IsCanceled)
                    {
                        feedbackText.text = "Error getting business user document: " + busUserTask.Exception;
                    }
                    else if (busUserTask.Result.Exists)
                    {
                        ShowDashboard(businessDashboardCanvas);
                    }
                    else
                    {
                        feedbackText.text = "User does not exist!";
                    }
                });
            }
        });
    }

    private void ShowDashboard(GameObject dashboardCanvas)
    {
        dashboardCanvas.SetActive(true);
        gameObject.SetActive(false);
        ResetFieldsAndFeedback();
    }

    private void ResetFieldsAndFeedback()
    {
        // Reset input fields
        emailInputField.text = "";
        passwordInputField.text = "";

        // Reset feedback message
        feedbackText.text = "";
    }

    void OnDestroy()
    {
        loginButton.onClick.RemoveListener(LoginWithEmailPassword);
    }
}
