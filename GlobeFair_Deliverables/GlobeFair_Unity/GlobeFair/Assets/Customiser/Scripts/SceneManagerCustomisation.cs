using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;

// Script that allows navigation to and from customisation scene
public class SceneManagerCustomisation : MonoBehaviour
{
    public Button backButton;
    public SaveCustomization saveCustomizationScript;

    private FirebaseAuth auth;

    void Start()
    {
        backButton.onClick.AddListener(SaveAndLoadDashboard);
        auth = FirebaseAuth.DefaultInstance;
    }

    // Saving the progress and changing scene with current authentication status
    private void SaveAndLoadDashboard()
    {
        saveCustomizationScript.SaveAvatarCustomization();
        Invoke(nameof(LoadDashboard), 2.0f);
    }

    private void LoadDashboard()
    {
        PlayerPrefs.SetInt("LoadDashboard", 1);
        SceneManager.LoadScene("Landing");
    }
}