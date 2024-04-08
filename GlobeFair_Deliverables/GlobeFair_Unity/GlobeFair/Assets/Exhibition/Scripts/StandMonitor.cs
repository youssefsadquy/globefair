using Firebase.Firestore;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using StarterAssets;

public class StandMonitor : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject outlineObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactionDistance = 3.0f;
    [SerializeField] private Canvas standInfoCanvas;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private ThirdPersonController thirdPersonController;
    public Texture2D mouseIcon;
    [SerializeField] private BusinessFAQFetcher businessFaqFetcher;

    public bool IsStandCanvasActive => standInfoCanvas.gameObject.activeSelf;

    private string currentMonitorTag;
    private bool isCanvasActive = false;

    private void Start()
    {
        interactionPrompt.SetActive(false);
        standInfoCanvas.gameObject.SetActive(false);
        ToggleCursor(false);
    }

    private void Update()
    {
        bool isPlayerClose = Vector3.Distance(playerTransform.position, transform.position) < interactionDistance;
        bool isPauseMenuActive = pauseMenuUI.activeSelf;

        if (isPlayerClose && !isPauseMenuActive)
        {
            outlineObject.SetActive(true);
            interactionPrompt.SetActive(true);

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
                }

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
            interactionPrompt.SetActive(false);
        }
    }

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
}