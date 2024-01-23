using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    // References to canvases
    public GameObject canvasLanding;
    public GameObject canvasLogin;
    public GameObject canvasSignup;
    public GameObject canvasLoginBusiness;
    public GameObject canvasSignupBusiness;

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
