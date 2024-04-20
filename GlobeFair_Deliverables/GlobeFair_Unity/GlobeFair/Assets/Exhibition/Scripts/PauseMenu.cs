using UnityEngine;
using System.Collections;
using Firebase.Auth;
using UnityEngine.SceneManagement;

// Manager of the pause menu with controlls to pause the game, save the state to firebase
// exit the application and autosaving
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject saveConfirmationUI;
    public GameObject exitConfirmationUI;
    public GameObject settingsUI;
    private bool isPaused = false;
    public GameStateManager gameStateManager;
    [SerializeField] private GameObject standInfoCanvas;
    private Animator animator;

    // Pausing game
    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        if (saveConfirmationUI != null) saveConfirmationUI.SetActive(false);
        exitConfirmationUI.SetActive(false);
        settingsUI.SetActive(false);
        StartAutoSave();
        animator = GetComponent<Animator>();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        StartAutoSave();
    }

    // Detection of pause key
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (standInfoCanvas != null && standInfoCanvas.activeSelf)
            {
                return;
            }

            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Method for saving to firebase
    public void SaveProgress()
    {
        gameStateManager.SaveGameState();
        Resume();
        StartCoroutine(SaveConfirmation());
    }

    // Pop up for saving confirmation
    private IEnumerator SaveConfirmation()
    {
        if (saveConfirmationUI != null)
        {
            saveConfirmationUI.SetActive(true);
            yield return new WaitForSeconds(4);
            saveConfirmationUI.SetActive(false);
        }
    }

    // Method that redirects to the dashboard
    public void LoadDashboard()
    {
        gameStateManager.SaveGameState();
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        PlayerPrefs.SetInt("LoadDashboard", 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Landing");
    }

    // Exit the application
    public void ConfirmSaveAndExit()
    {
        gameStateManager.SaveGameState();
        pauseMenuUI.SetActive(false);
        exitConfirmationUI.SetActive(true);
    }

    public void Stay()
    {
        exitConfirmationUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        Resume();
        StartCoroutine(SaveConfirmation());
    }

    public void Leave()
    {
        gameStateManager.SaveGameState();

        StartCoroutine(SaveConfirmation());

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignOut();

        Application.Quit();
    }

    // Application Settings
    public void OpenSettings()
    {
        gameStateManager.SaveGameState();
        pauseMenuUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // Autosave method to automatically call savegamestate every 4 minutes
    private void StartAutoSave()
    {
        StartCoroutine(AutoSaveRoutine());
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(240);
            gameStateManager.SaveGameState();
            StartCoroutine(SaveConfirmation());
        }
    }

}