using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Extensions;


public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public Button loginButton;
    public GameObject userDashboardCanvas;

    private FirebaseAuth auth;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
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

            // Firebase user signed in successfully
            FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            // Redirect to the UserDashboard
            userDashboardCanvas.SetActive(true);
            gameObject.transform.parent.gameObject.SetActive(false);
        });
    }

    void OnDestroy()
    {
        loginButton.onClick.RemoveListener(LoginWithEmailPassword);
    }
}