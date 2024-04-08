using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class NavigationPC : MonoBehaviour
{
    public GameObject canvasHomeScreen;
    public GameObject canvasBusinessInfo;
    public GameObject canvasBusinessQA;

    private void SetActiveCanvas(GameObject canvas)
    {
        canvasHomeScreen.SetActive(false);
        canvasBusinessInfo.SetActive(false);

        //canvas.SetActive(true);
    }

    // Method to navigate to Main Screen Canvas
    public void GoToHomeScreen()
    {
        canvasBusinessInfo.SetActive(false);
        canvasHomeScreen.SetActive(true);
        canvasBusinessQA.SetActive(false);
    }

    // Method to navigate to Business Information Canvas
    public void GoToBusinessInfo()
    {
        canvasBusinessInfo.SetActive(true);
        canvasHomeScreen.SetActive(false);
        canvasBusinessQA.SetActive(false);
    }

    // Method to navigate to Business faq Canvas
    public void GoToBusinessQA()
    {
        canvasBusinessQA.SetActive(true);
        canvasHomeScreen.SetActive(false);
    }
}
