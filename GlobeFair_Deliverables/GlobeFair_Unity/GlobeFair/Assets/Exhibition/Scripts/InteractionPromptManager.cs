using UnityEngine;
using System.Collections.Generic;

// Method that allows to display the interaction prompt on proximity
public class InteractionPromptManager : MonoBehaviour
{
    public static InteractionPromptManager Instance { get; private set; }

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactionDistance = 1.0f;

    private List<Transform> monitors = new List<Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Updates on everyframe to find closest monitor to player
    private void Update()
    {
        CheckClosestMonitor();
    }

    public void RegisterMonitor(Transform monitor)
    {
        if (!monitors.Contains(monitor))
        {
            monitors.Add(monitor);
        }
    }

    public void UnregisterMonitor(Transform monitor)
    {
        if (monitors.Contains(monitor))
        {
            monitors.Remove(monitor);
        }
    }

    // Determine the closest monitor to player and shows the interaction prompt
    private void CheckClosestMonitor()
    {
        float closestDistance = float.MaxValue;
        Transform closestMonitor = null;

        foreach (Transform monitor in monitors)
        {
            float distance = Vector3.Distance(playerTransform.position, monitor.position);
            if (distance < closestDistance && distance < interactionDistance)
            {
                closestDistance = distance;
                closestMonitor = monitor;
            }
        }

        interactionPrompt.SetActive(closestMonitor != null);
    }
}