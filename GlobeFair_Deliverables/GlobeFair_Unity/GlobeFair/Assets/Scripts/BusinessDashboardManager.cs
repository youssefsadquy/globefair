using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Firebase.Extensions;

public class BusinessDashboardManager : MonoBehaviour
{
    public TextMeshProUGUI businessNameText;
    public TextMeshProUGUI emailText;
    public Button startGameButton;
    public Button logoutButton;
    public GameObject landingCanvas;
    public GameObject loadingCanvas;

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
                businessNameText.text = snapshot.GetValue<string>("BusinessName");
                emailText.text = snapshot.GetValue<string>("Email");
            }
            else
            {
                Debug.LogError("Business does not exist!");
            }
        });
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

    public void StartFair()
    {
        loadingCanvas.SetActive(true);
        Canvas.ForceUpdateCanvases();
        StartCoroutine(WaitAndLoadScene("GlobeFair"));
    }

    private void LogoutUser()
    {
        auth.SignOut();
        landingCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        startGameButton.onClick.RemoveListener(() => StartFair());
    }
}
