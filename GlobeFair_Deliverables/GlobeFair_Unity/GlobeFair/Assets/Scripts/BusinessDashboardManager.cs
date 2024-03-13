using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase.Extensions;

public class BusinessDashboardManager : MonoBehaviour
{
    public Text businessNameText;
    public Text emailText;
    public Button startGameButton;
    public Button logoutButton;
    public GameObject landingCanvas;
    public GameObject businessDashboardCanvas;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseUser currentUser;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        currentUser = auth.CurrentUser;
    }

    void Start()
    {
        if (currentUser != null)
        {
            FetchBusinessDetails();
            startGameButton.onClick.AddListener(() => StartFair());
            logoutButton.onClick.AddListener(() => LogoutUser());
        }
        else
        {
            Debug.LogError("No business user is logged in.");
        }
    }

    private void FetchBusinessDetails()
    {
        if (currentUser == null) return;

        DocumentReference businessDocRef = db.Collection("bus_users").Document(currentUser.UserId);
        businessDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Error fetching business details: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                businessNameText.text = snapshot.GetValue<string>("BusinessName") ?? "Business Name";
                emailText.text = snapshot.GetValue<string>("Email") ?? "Email";
            }
            else
            {
                Debug.LogError("Business does not exist!");
                businessNameText.text = "Business Name";
                emailText.text = "Email"; 
            }
        });
    }

    void OnEnable()
    {
        currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            FetchBusinessDetails();
        }
        else
        {
            Debug.LogError("No user is logged in.");
        }
    }

    public void StartFair()
    {
        LoadingManager.Instance.EnableLoading("Loading the fair...");

        StartCoroutine(LoadGameScene("GlobeFair"));

        if (businessDashboardCanvas != null)
        {
            businessDashboardCanvas.SetActive(false);
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
        businessNameText.text = "Business Name";
        emailText.text = "Email";
    }

    void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(() => StartFair());
    }
}
