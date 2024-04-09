using UnityEngine;
using UnityEngine.UI;

// Method to initialise scrollview with position 1(top)
public class AutoScrollToTop : MonoBehaviour
{
    private ScrollRect scrollRect;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();

        if (scrollRect == null)
        {
            Debug.LogError("ScrollRect component not found on the GameObject.");
            return;
        }

        ScrollToTop();
    }

    public void ScrollToTop()
    {
        // Set the vertical position to 1
        scrollRect.verticalNormalizedPosition = 1f;
    }
}