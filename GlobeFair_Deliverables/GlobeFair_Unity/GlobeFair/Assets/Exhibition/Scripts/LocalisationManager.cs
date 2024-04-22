using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LocalisationData
{
    public string languageCode;
    public List<string> keys;
    public List<string> values;
}

// Local localisation to translate all the default text into the 4 select languages
public class LocalisationManager : MonoBehaviour
{
    public List<LocalisationData> localisationData;
    private Dictionary<string, Dictionary<string, string>> translations;
    public static LocalisationManager Instance;

    // Components to translate
    // Main Screen
    public Text companyName;
    public Text welcomeTextLabel;
    public Text businessBioButton;
    public Text businessFAQButton;
    public Text businessListingsButton;
    public Text exitButton;
    public Text languageSelector;

    // Business Listings Screen
    public Text screenlistingTitle;
    public Text screenlistingsubTitle;

    // Business FAQ Screen
    public Text screenFAQTitle;
    public Text screenFAQsubTitle;

    // Business Bio Screen
    public Text screenBioMainTitle;
    public Text screenBioTitle;
    public Text screenBioName;
    public Text screenBioDescription;
    public Text screenBioMission;
    public Text screenBioIndustry;
    public Text screenBioEmployees;
    public Text screenBioFounded;
    public Text screenBioHeadquarters;
    public Text screenBioEmail;
    public Text screenBioWebsite;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        BuildDictionaries();
    }

    // Method to create translations for each language
    void BuildDictionaries()
    {
        translations = new Dictionary<string, Dictionary<string, string>>();

        foreach (var locData in localisationData)
        {
            Dictionary<string, string> languageDict = new Dictionary<string, string>();
            for (int i = 0; i < locData.keys.Count; i++)
            {
                if (i < locData.values.Count)
                {
                    languageDict.Add(locData.keys[i], locData.values[i]);
                }
            }
            translations.Add(locData.languageCode, languageDict);
        }
    }

    // Fetching the translated value with the selected value
    public string GetLocalisedValue(string key, string languageCode)
    {
        if (translations.TryGetValue(languageCode, out var languageDict))
        {
            if (languageDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        return key;
    }


    public void UpdateUILocalisation(string languageCode)
    {

        // Main Screen
        companyName.text = GetLocalisedValue("CompanyName", languageCode);
        welcomeTextLabel.text = GetLocalisedValue("WelcomeText", languageCode);
        businessBioButton.text = GetLocalisedValue("BusinessBioButton", languageCode);
        businessFAQButton.text = GetLocalisedValue("BusinessFAQButton", languageCode);
        businessListingsButton.text = GetLocalisedValue("BusinessListingsButton", languageCode);
        exitButton.text = GetLocalisedValue("ExitButton", languageCode);
        languageSelector.text = GetLocalisedValue("languageSelector", languageCode);

        // Business Listings
        screenlistingTitle.text = GetLocalisedValue("screenlistingTitle", languageCode);
        screenlistingsubTitle.text = GetLocalisedValue("screenlistingsubTitle", languageCode);

        // Business FAQ
        screenFAQTitle.text = GetLocalisedValue("screenFAQTitle", languageCode);
        screenFAQsubTitle.text = GetLocalisedValue("screenFAQsubTitle", languageCode);

        // Business Bio Screen
        screenBioMainTitle.text = GetLocalisedValue("screenBioMainTitle", languageCode);
        screenBioTitle.text = GetLocalisedValue("screenBioTitle", languageCode);
        screenBioName.text = GetLocalisedValue("screenBioName", languageCode);
        screenBioDescription.text = GetLocalisedValue("screenBioDescription", languageCode);
        screenBioMission.text = GetLocalisedValue("screenBioMission", languageCode);
        screenBioIndustry.text = GetLocalisedValue("screenBioIndustry", languageCode);
        screenBioEmployees.text = GetLocalisedValue("screenBioEmployees", languageCode);
        screenBioFounded.text = GetLocalisedValue("screenBioFounded", languageCode);
        screenBioHeadquarters.text = GetLocalisedValue("screenBioHeadquarters", languageCode);
        screenBioEmail.text = GetLocalisedValue("screenBioEmail", languageCode);
        screenBioWebsite.text = GetLocalisedValue("screenBioWebsite", languageCode);
    }
}