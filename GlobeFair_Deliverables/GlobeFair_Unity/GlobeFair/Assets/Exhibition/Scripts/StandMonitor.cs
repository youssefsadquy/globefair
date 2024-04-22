using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine.Events;

// Method that allows interaction with stands, with functionality of storing
// and passing tags to relevant canvases
public class StandMonitor : MonoBehaviour
{
    [SerializeField] private GameObject outlineObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactionDistance = 3.0f;
    [SerializeField] private Canvas standInfoCanvas;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private ThirdPersonController thirdPersonController;
    public Texture2D mouseIcon;
    [SerializeField] private BusinessFAQFetcher businessFaqFetcher;
    [SerializeField] private BusinessListingsFetcher businessListingsFetcher;
    [SerializeField] private Dropdown languageDropdown;
    public static StandMonitor Instance;

    private FirebaseFirestore db;
    private HashSet<string> uniqueUserIds = new HashSet<string>();

    public bool IsStandCanvasActive => standInfoCanvas.gameObject.activeSelf;

    private string currentMonitorTag;
    private bool isCanvasActive = false;

    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;

        Instance = this;
    }

    private void Start()
    {
        standInfoCanvas.gameObject.SetActive(false);
        ToggleCursor(false);
        languageDropdown.options.Clear();
        foreach (var language in languageCodeDictionary)
        {
            languageDropdown.options.Add(new Dropdown.OptionData(language.Key));
        }

        languageDropdown.value = languageDropdown.options.FindIndex(option => option.text == "English");
        PlayerPrefs.SetString("selectedLanguage", languageCodeDictionary["English"]);
        PlayerPrefs.Save();

        languageDropdown.onValueChanged.AddListener(delegate
        {
            LanguageChanged(languageDropdown.value);
        });
    }

    // Method that allows changing of language with Google API and default localisation
    private void LanguageChanged(int index)
    {
        Debug.Log("Language Changed triggered.");

        string selectedLanguage = languageDropdown.options[index].text;
        string selectedLanguageCode = languageCodeDictionary.ContainsKey(selectedLanguage) ? languageCodeDictionary[selectedLanguage] : "en";

        PlayerPrefs.SetString("selectedLanguage", selectedLanguageCode);
        PlayerPrefs.Save();

        LocalisationManager.Instance.UpdateUILocalisation(selectedLanguageCode);
    }

    // Allow register and unregister monitors on distance
    private void OnEnable()
    {
        InteractionPromptManager.Instance.RegisterMonitor(transform);
    }

    private void OnDisable()
    {
        InteractionPromptManager.Instance.UnregisterMonitor(transform);
    }

    private string GetUserIdFromAuthenticationContext()
    {
        Firebase.Auth.FirebaseUser user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        return user != null ? user.UserId : string.Empty;
    }


    private void Update()
    {
        // Checking player proximity and that pause menu is not active for clashing
        bool isPlayerClose = Vector3.Distance(playerTransform.position, transform.position) < interactionDistance;
        bool isPauseMenuActive = pauseMenuUI.activeSelf;

        if (isPlayerClose && !isPauseMenuActive)
        {
            outlineObject.SetActive(true);

            // Fetch when the interaction starts and pass tag to relevant canvas
            if (Input.GetKeyDown(KeyCode.X))
            {
                isCanvasActive = !isCanvasActive;
                standInfoCanvas.gameObject.SetActive(isCanvasActive);
                ToggleCursor(isCanvasActive);

                if (isCanvasActive)
                {
                    currentMonitorTag = gameObject.tag;
                    Debug.Log("Current Monitor Tag: " + currentMonitorTag);
                    thirdPersonController.SetInputEnabled(!isCanvasActive);
                    string userId = GetUserIdFromAuthenticationContext();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Call to increment total visits
                        IncrementTotalVisits(currentMonitorTag);

                        // Call to update unique visitors
                        UpdateUniqueVisitors(currentMonitorTag, userId);
                    }
                    else
                    {
                        Debug.LogWarning("User ID is null or empty. The user mis not logged in.");
                    }

                    FetchSetInformation fetchSetInformation = standInfoCanvas.GetComponentInChildren<FetchSetInformation>(true);
                    if (fetchSetInformation != null)
                    {
                        fetchSetInformation.SetCurrentMonitorTag(currentMonitorTag);
                    }

                    BusinessInformationFetcher infoFetcher = standInfoCanvas.GetComponentInChildren<BusinessInformationFetcher>(true);
                    if (infoFetcher != null)
                    {
                        infoFetcher.InitializeWithMonitorTag(currentMonitorTag);
                    }

                    BusinessFAQFetcher faqFetcher = standInfoCanvas.GetComponentInChildren<BusinessFAQFetcher>(true);
                    if (faqFetcher != null)
                    {
                        faqFetcher.InitializeWithMonitorTag(currentMonitorTag);
                    }

                    BusinessListingsFetcher businessListingsFetcher = standInfoCanvas.GetComponentInChildren<BusinessListingsFetcher>(true);
                    if (businessListingsFetcher != null)
                    {
                        businessListingsFetcher.InitializeWithMonitorTag(currentMonitorTag);
                    }
                }

                // Clearing all the business info fields for next monitor
                if (!isCanvasActive)
                {
                    BusinessInformationFetcher infoFetcher =
                        standInfoCanvas.GetComponentInChildren<BusinessInformationFetcher>(true);
                    if (infoFetcher != null)
                    {
                        infoFetcher.ClearBusinessInformation();
                    }
                }
            }
        }
        else
        {
            if (isCanvasActive)
            {
                isCanvasActive = false;
                standInfoCanvas.gameObject.SetActive(false);
                ToggleCursor(false);
                currentMonitorTag = "";
                thirdPersonController.SetInputEnabled(true);
            }
            outlineObject.SetActive(false);
        }
    }

    // Controller for the cursor with custom cursor application
    // locking character and camera on enabling
    private void ToggleCursor(bool isActive)
    {
        if (isActive)
        {
            Cursor.SetCursor(mouseIcon, new Vector2(16, 16), CursorMode.Auto);
        }

        if (!isActive)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        thirdPersonController.SetInputEnabled(!isActive);
    }

    public string GetCurrentMonitorTag()
    {
        return currentMonitorTag;
    }

    // Method to increment total visits for current booth on firebase
    private void IncrementTotalVisits(string standTag)
    {
        // The document ID from the stand tag
        string documentId = standTag.Replace("_Monitor_1", "");
        DocumentReference docRef = db.Collection("booths").Document(documentId);

        // Increment the totalVisits field
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(docRef).ContinueWith(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                long newTotalVisits = snapshot.ContainsField("totalVisits") ? snapshot.GetValue<long>("totalVisits") + 1 : 1;
                transaction.Update(docRef, "totalVisits", newTotalVisits);
                return newTotalVisits;
            });
        });
    }
    // Method to increment unique visits for current booth on firebase
    private void UpdateUniqueVisitors(string standTag, string userId)
    {
        // If not unique do not count
        if (uniqueUserIds.Contains(userId))
        {
            return;
        }

        // Add the user ID to a set of unique IDs
        uniqueUserIds.Add(userId);

        // The document ID from the stand tag
        string documentId = standTag.Replace("_Monitor_1", "");
        DocumentReference docRef = db.Collection("booths").Document(documentId);

        // Increment the uniqueVisitors field
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(docRef).ContinueWith(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                long newUniqueVisitors = snapshot.ContainsField("uniqueVisitors") ? snapshot.GetValue<long>("uniqueVisitors") + 1 : 1;
                transaction.Update(docRef, "uniqueVisitors", newUniqueVisitors);
                return newUniqueVisitors;
            });
        });
    }

    // Dictionary to map language names to their codes
    private Dictionary<string, string> languageCodeDictionary = new Dictionary<string, string>
    {
        { "English", "en" },
        { "Spanish", "es" },
        { "French", "fr" },
        { "Italian", "it" },
    };
}