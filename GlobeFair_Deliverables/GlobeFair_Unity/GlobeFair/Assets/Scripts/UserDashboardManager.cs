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
    }

    private void UploadCV()
    {
        var extensions = new[] { new ExtensionFilter("PDF Files", "pdf") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length > 0)
        {
            StartCoroutine(UploadPDF(paths[0]));
        }
    }

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
        }
    }
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

    void OnDestroy()
    {
        uploadCVButton.onClick.RemoveListener(() => UploadCV());
        startGameButton.onClick.RemoveListener(() => StartFair());
    }
}
