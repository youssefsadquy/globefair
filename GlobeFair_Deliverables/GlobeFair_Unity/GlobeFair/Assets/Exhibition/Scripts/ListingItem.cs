using UnityEngine;
using UnityEngine.UI;

// Method attached to Listing prefab to populate the fields
public class ListingItem : MonoBehaviour
{
    [SerializeField] private Text jobTitleText;
    [SerializeField] private Text jobDescriptionText;
    [SerializeField] private Text locationText;
    [SerializeField] private Text salaryText;
    [SerializeField] private Text howToApplyText;
    [SerializeField] private Text creationDateText;

    public void SetListingInfo(string jobTitle, string jobDescription, string location, string salary, string howToApply, string creationDate)
    {
        jobTitleText.text = jobTitle;
        jobDescriptionText.text = jobDescription;
        locationText.text = location;
        salaryText.text = salary;
        howToApplyText.text = howToApply;
        creationDateText.text = creationDate;
    }
}
