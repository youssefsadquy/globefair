using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;


public class NavigationManager : MonoBehaviour
{
    public GameObject canvasLanding;
    public GameObject canvasLogin;
    public GameObject canvasSignup;
    public GameObject canvasLoginBusiness;
    public GameObject canvasSignupBusiness;
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (PlayerPrefs.GetInt("LoadDashboard", 0) == 1)
        {
            PlayerPrefs.SetInt("LoadDashboard", 0);
            LoadAppropriateDashboard();
        }
        else
        {
            GoToCanvasLanding();
        }
    }

    public void LoadAppropriateDashboard()
    {
        FirebaseUser currentUser = auth.CurrentUser;
        if (currentUser != null)
        {
            db.Collection("users").Document(currentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Error fetching user document: " + task.Exception);
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    // Show the user dashboard
                    SetActiveCanvas(userDashboardCanvas);
                }
                else
                {
                    db.Collection("bus_users").Document(currentUser.UserId).GetSnapshotAsync().ContinueWithOnMainThread(busTask =>
                    {
                        if (busTask.IsFaulted || busTask.IsCanceled)
                        {
                            Debug.LogError("Error fetching business user document: " + busTask.Exception);
                            return;
                        }

                        DocumentSnapshot busSnapshot = busTask.Result;
                        if (busSnapshot.Exists)
                        {
                            // Show the business dashboard
                            SetActiveCanvas(businessDashboardCanvas);
                        }
                        else
                        {
                            Debug.LogError("No dashboard found for user!");
                            // Show the landing canvas if no dashboard is found
                            SetActiveCanvas(canvasLanding);
                        }
                    });
                }
            });
        }
        else
        {
            Debug.LogError("No user is logged in.");
            // Show the landing canvas if no user is logged in
            SetActiveCanvas(canvasLanding);
        }
    }

    private void SetActiveCanvas(GameObject canvas)
    {
        canvasLanding.SetActive(false);
        canvasLogin.SetActive(false);
        canvasSignup.SetActive(false);
        canvasLoginBusiness.SetActive(false);
        canvasSignupBusiness.SetActive(false);
        userDashboardCanvas.SetActive(false);
        businessDashboardCanvas.SetActive(false);

        canvas.SetActive(true);
    }


    // Method when the user clicks the user login button
    public void GoToCanvasLogin()
    {
        canvasLanding.SetActive(false);
        canvasLogin.SetActive(true);
        canvasSignup.SetActive(false);
        canvasLoginBusiness.SetActive(false);
        canvasSignupBusiness.SetActive(false);
    }

    // Method when the user clicks the user signup button
    public void GoToCanvasSignup()
    {
        canvasLanding.SetActive(false);
        canvasLogin.SetActive(false);
        canvasSignup.SetActive(true);
        canvasLoginBusiness.SetActive(false);
        canvasSignupBusiness.SetActive(false);
    }

    // Method when the user clicks the business login button
    public void GoToCanvasLoginBusiness()
    {
        canvasLanding.SetActive(false);
        canvasLogin.SetActive(false);
        canvasSignup.SetActive(false);
        canvasLoginBusiness.SetActive(true);
        canvasSignupBusiness.SetActive(false);
    }

    // Method when the user clicks the business signup button
    public void GoToCanvasSignupBusiness()
    {
        canvasLanding.SetActive(false);
        canvasLogin.SetActive(false);
        canvasSignup.SetActive(false);
        canvasLoginBusiness.SetActive(false);
        canvasSignupBusiness.SetActive(true);
    }

    // Method to return to the landing canvas from any other canvas
    public void GoToCanvasLanding()
    {
        canvasLanding.SetActive(true);
        canvasLogin.SetActive(false);
        canvasSignup.SetActive(false);
        canvasLoginBusiness.SetActive(false);
        canvasSignupBusiness.SetActive(false);
    }
}
