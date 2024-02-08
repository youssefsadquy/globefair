using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Extensions;
using Firebase.Firestore;


public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public GameObject userDashboardCanvas;
    public GameObject businessDashboardCanvas;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser currentUser;

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

        // Validate the email and password not empty
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Email and Password are required.");
            return;
        }

        // Sign in with email and password
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

            else
            {
                // Firebase user signed in successfully
                FirebaseUser newUser = task.Result.User;
                // Check UID in firebase
                CheckUserCollection(newUser);
            }
        });
    }

    private void CheckUserCollection(FirebaseUser user)
    {
        // Check the 'users' collection
        DocumentReference userDocRef = db.Collection("users").Document(user.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(userTask =>
        {
            if (userTask.IsFaulted || userTask.IsCanceled)
            {
                Debug.LogError("Error getting user document: " + userTask.Exception);
            }
            else if (userTask.Result.Exists)
            {
                // Document exists in 'users' collection activate user dashboard
                userDashboardCanvas.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                // Document doesn't exist in 'users' collection check 'bus_users' collection
                DocumentReference busUserDocRef = db.Collection("bus_users").Document(user.UserId);
                busUserDocRef.GetSnapshotAsync().ContinueWithOnMainThread(busUserTask =>
                {
                    if (busUserTask.IsFaulted || busUserTask.IsCanceled)
                    {
                        Debug.LogError("Error getting business user document: " + busUserTask.Exception);
                    }
                    else if (busUserTask.Result.Exists)
                    {
                        // Document exists in 'bus_users' collection activate business dashboard
                        businessDashboardCanvas.SetActive(true);
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("User does not exist!");
                    }
                });
            }
        });
    }

    void OnDestroy()
    {
        loginButton.onClick.RemoveListener(LoginWithEmailPassword);
    }
}