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
    public GameObject loadingCanvas;
    public Slider loadingProgressBar;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    private FirebaseUser currentUser;

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
    }

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
        else
        {
            Debug.LogError("No user is logged in.");
        }
    }

    private void UploadCV()
    {
        // Open file with SFB
        var extensions = new[] {
            new ExtensionFilter("PDF Files", "pdf")
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length > 0)
        {
            StartCoroutine(UploadPDF(paths[0]));
        }
    }

    private IEnumerator UploadPDF(string path)
    {
        // firebase App
        var storageRef = storage.GetReferenceFromUrl("gs://globefair-46eb2.appspot.com");

        var pdfRef = storageRef.Child("cvs/" + currentUser.UserId + ".pdf");

        byte[] fileBytes = null;
        try
        {
            fileBytes = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading file: " + e.Message);
            yield break;
        }

        var uploadTask = pdfRef.PutBytesAsync(fileBytes);

        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.IsFaulted)
        {
            Debug.LogError("Upload failed: " + uploadTask.Exception);
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
        }
    }

    private IEnumerator DelayedLoad(string sceneName)
    {
        yield return new WaitForSeconds(1f); // 1 second delay

        StartCoroutine(LoadGameScene(sceneName));
    }

    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForEndOfFrame();

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadGameScene(string sceneName)
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

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

        loadingCanvas.SetActive(false);
    }

    public void StartFair()
    {
        loadingCanvas.SetActive(true);

        Canvas.ForceUpdateCanvases();

        StartCoroutine(WaitAndLoadScene("GlobeFair"));
    }

    void OnDestroy()
    {
        uploadCVButton.onClick.RemoveListener(() => UploadCV());
        startGameButton.onClick.RemoveListener(() => StartFair());
    }
}
