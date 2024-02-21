using UnityEngine;
using System.Collections;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject saveConfirmationUI;
    public GameObject exitConfirmationUI;
    private bool isPaused = false;
    public GameStateManager gameStateManager;


    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        if (saveConfirmationUI != null) saveConfirmationUI.SetActive(false);
        exitConfirmationUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SaveProgress()
    {
        gameStateManager.SaveGameState();
        Resume();
        StartCoroutine(SaveConfirmation());
    }

    private IEnumerator SaveConfirmation()
    {
        if (saveConfirmationUI != null)
        {
            saveConfirmationUI.SetActive(true);
            yield return new WaitForSeconds(4);
            saveConfirmationUI.SetActive(false);
        }
    }



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
}