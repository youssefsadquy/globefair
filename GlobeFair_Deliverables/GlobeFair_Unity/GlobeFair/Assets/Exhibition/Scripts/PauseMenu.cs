using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;
    public GameStateManager gameStateManager;

    void Start()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
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

    public void LoadDashboard()
    {
        // Call the method to save the game state
        gameStateManager.SaveGameState();

        // Hide the pause menu
        pauseMenuUI.SetActive(false);

        // Unpause the game before switching scenes
        Time.timeScale = 1f;

        // Ensure the cursor is unlocked and made invisible before leaving the scene
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Set the PlayerPrefs flag to load the dashboard
        PlayerPrefs.SetInt("LoadDashboard", 1);

        // Load the Landing scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Landing");
    }
}
