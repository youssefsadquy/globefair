using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ListingPrefabScript : MonoBehaviour
{
    public Text jobTitleText;
    public Text jobLocationText;
    public Text salaryText;
    public Text creationDateText;

    public void Setup(Dictionary<string, object> data)
    {
        Debug.Log("Setting up listing with data: " + data.ToString());

        if (data.TryGetValue("jobTitle", out object jobTitle))
        {
            Debug.Log("Job Title: " + (jobTitle as string));
            jobTitleText.text = jobTitle as string;
        }

        if (data.TryGetValue("creationDate", out object creationDate))
        {
            if (creationDate is Timestamp timestamp)
            {
                DateTime dateTime = timestamp.ToDateTime();
                string dateString = dateTime.ToString("dd-MM-yyyy HH:mm:ss");
                Debug.Log("Creation Date: " + dateString);
                creationDateText.text = dateString;
            }
        }

        if (data.TryGetValue("location", out object jobLocation))
        {
            Debug.Log("Location: " + (jobLocation as string));
            jobLocationText.text = jobLocation as string;
        }

        if (data.TryGetValue("salary", out object salary))
        {
            Debug.Log("Salary: " + (salary as string));
            salaryText.text = salary as string;
        }
    }
}