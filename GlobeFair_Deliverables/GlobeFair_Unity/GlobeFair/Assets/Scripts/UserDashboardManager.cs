using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using SFB;
using System.Collections;
using Firebase.Extensions;
using System;


public class UserDashboardManager : MonoBehaviour
{
    public Text nameText;
    public Text emailText;
    public Button uploadCVButton;
    public Button startGameButton;
    public Button logoutButton;
    public GameObject landingCanvas;
    public GameObject userDashboardCanvas;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    private FirebaseUser currentUser;

    public Image cvStatusImage;
    public Sprite greenTickSprite;
    public Sprite redCrossSprite;
    public Button removeCVButton;
    private IDisposable firestoreListener;
    public Text feedbackText;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;
        currentUser = auth.CurrentUser;
    }

    void Start()
    {
        if (currentUser != null)
        {
            FetchUserDetails();
            uploadCVButton.onClick.AddListener(() => UploadCV());
            startGameButton.onClick.AddListener(() => StartFair());
            logoutButton.onClick.AddListener(() => LogoutUser());
        }
        else
        {
            Debug.LogError("No user is logged in.");
        }

        SetupFirestoreListener();
        removeCVButton.onClick.AddListener(() => RemoveCV());
    }

    // Method to display user info on dashboard
    private void FetchUserDetails()
    {
        if (currentUser == null) return;

        DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching user details: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                nameText.text = snapshot.GetValue<string>("FirstName") ?? "Name";
                emailText.text = snapshot.GetValue<string>("Email") ?? "Email";
            }
            else
            {
                Debug.LogError("Document does not exist!");
                nameText.text = "Name";
                emailText.text = "Email";
            }
        });
    }

    void OnEnable()
    {
        currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            FetchUserDetails();
        }
    }

    // Onclick for upload CV button
    private void UploadCV()
    {
        var extensions = new[] { new ExtensionFilter("PDF Files", "pdf") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length > 0)
        {
            StartCoroutine(UploadPDF(paths[0]));
        }
    }

    // Method to upload button on storage and extract URL
    private IEnumerator UploadPDF(string path)
    {
        var storageRef = storage.GetReferenceFromUrl("gs://globefair-46eb2.appspot.com");
        var pdfRef = storageRef.Child("cvs/" + currentUser.UserId + ".pdf");

        byte[] fileBytes = null;
        try
        {
            fileBytes = File.ReadAllBytes(path);
        }
        catch (IOException e)
        {
            Debug.LogError("Error reading file: " + e.Message);
            yield break;
        }

        var uploadTask = pdfRef.PutBytesAsync(fileBytes);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.IsFaulted)
        {
            foreach (var error in uploadTask.Exception.Flatten().InnerExceptions)
            {
                Debug.LogError(error.Message);
            }
        }
        else if (uploadTask.IsCanceled)
        {
            Debug.LogError("Upload was canceled.");
        }
        else
        {
            Debug.Log("File uploaded successfully.");

            var getDownloadUrlTask = pdfRef.GetDownloadUrlAsync();
            yield return new WaitUntil(() => getDownloadUrlTask.IsCompleted);

            if (getDownloadUrlTask.IsFaulted || getDownloadUrlTask.IsCanceled)
            {
                Debug.LogError("Error getting download URL: " + getDownloadUrlTask.Exception);
            }
            else
            {
                string pdfUrl = getDownloadUrlTask.Result.ToString();
                SavePdfUrlToFirestore(pdfUrl);
            }
        }
    }

    // Method to set the CV url under the current user document
    private void SavePdfUrlToFirestore(string pdfUrl)
    {
        DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);
        userDocRef.UpdateAsync("cv", pdfUrl).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error updating Firestore with PDF URL: " + task.Exception);
            }
            else
            {
                Debug.Log("Firestore updated with PDF URL for user: " + currentUser.UserId);
            }
        });
    }

    // Live UI update for CV uploading status
    private void SetupFirestoreListener()
    {
        DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);
        firestoreListener = userDocRef.Listen(snapshot =>
        {
            if (snapshot.Exists && snapshot.TryGetValue("cv", out string cvLink))
            {
                cvStatusImage.sprite = greenTickSprite;
                ShowFeedback("CV is uploaded.", true);
            }
            else
            {
                cvStatusImage.sprite = redCrossSprite;
            }
        });
    }

    // Method to delete uploaded CV
    private void RemoveCV()
    {
        var storageRef = storage.GetReferenceFromUrl("gs://globefair-46eb2.appspot.com/cvs/" + currentUser.UserId + ".pdf");
        storageRef.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error deleting CV from storage: " + task.Exception);
                ShowFeedback("Error deleting CV from storage: " + task.Exception, false);
            }
            else
            {
                Debug.Log("CV deleted from storage successfully.");
                ShowFeedback("CV deleted from storage successfully.", true);

                DocumentReference userDocRef = db.Collection("users").Document(currentUser.UserId);
                userDocRef.UpdateAsync("cv", FieldValue.Delete).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Error removing CV from Firestore: " + task.Exception);
                        ShowFeedback("Error removing CV from Firestore: " + task.Exception, false);
                    }
                    else
                    {
                        Debug.Log("CV removed from Firestore successfully.");
                        ShowFeedback("CV removed from Firestore successfully.", true);
                    }
                });
            }
        });
    }

    // Onclick for start fair button
    public void StartFair()
    {
        LoadingManager.Instance.EnableLoading("Loading the fair...");

        StartCoroutine(LoadGameScene("GlobeFair"));

        if (userDashboardCanvas != null)
        {
            userDashboardCanvas.SetActive(false);
        }
    }

    private IEnumerator LoadGameScene(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        LoadingManager.Instance.DisableLoading();
    }

    // Onclick for logout button
    private void LogoutUser()
    {
        auth.SignOut();
        ResetDashboard();
        landingCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    private void ResetDashboard()
    {
        nameText.text = "Name";
        emailText.text = "Email";
    }

    // Method to display messages to user UI feedback field
    public void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = isSuccess ? Color.green : Color.red;
            StartCoroutine(ClearFeedbackAfterDelay(3));
        }
    }

    // Method to clear feedback
    private IEnumerator ClearFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void OnDestroy()
    {
        uploadCVButton.onClick.RemoveListener(() => UploadCV());
        startGameButton.onClick.RemoveListener(() => StartFair());
        firestoreListener?.Dispose();
    }
}
