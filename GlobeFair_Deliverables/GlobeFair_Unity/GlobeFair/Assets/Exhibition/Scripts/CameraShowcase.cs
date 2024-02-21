using UnityEngine;
using System.Collections;
using Cinemachine;
using UnityEngine.UI;

public class CameraShowcase : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject thirdPersonCharacter;
    public CinemachineVirtualCamera thirdPersonCinemachineCamera;
    public float speed = 0.1f;
    public float rotationSpeed = 5f;
    private int waypointIndex = 0;
    private bool isMoving = true;
    private Vector3 currentVelocity = Vector3.zero;
    public GameObject skipPanel;
    private bool canSkip = true;

    void Awake()
    {
        thirdPersonCharacter.SetActive(false);
        thirdPersonCinemachineCamera.gameObject.SetActive(false);

        if (skipPanel != null)
        {
            skipPanel.SetActive(true);
        }
    }

    void Start()
    {
        if (!GameStateManager.GameStateLoaded)
        {
            StartShowcase();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void StartShowcase()
    {
        if (!GameStateManager.GameStateLoaded)
        {
            gameObject.SetActive(true);
            isMoving = true;
            canSkip = true;
            waypointIndex = 0;
            StartCoroutine(ShowcaseRoutine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator ShowcaseRoutine()
    {
        yield return null;

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            LookAtTarget(0);
        }
    }

    void Update()
    {
        if (isMoving)
        {
            MoveThroughWaypoints();
        }

        if (canSkip && Input.GetKeyDown(KeyCode.S))
        {
            SkipShowcase();
        }
    }

    void MoveThroughWaypoints()
    {
        if (waypointIndex < waypoints.Length)
        {
            Vector3 targetPosition = waypoints[waypointIndex].position;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, speed);
            LookAtTarget(waypointIndex);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                waypointIndex++;
                if (waypointIndex == waypoints.Length)
                {
                    FinishShowcase();
                }
            }
        }
    }

    void SkipShowcase()
    {
        FinishShowcase();
    }

    void FinishShowcase()
    {
        isMoving = false;
        canSkip = false;

        thirdPersonCharacter.SetActive(true);
        thirdPersonCinemachineCamera.gameObject.SetActive(true);
        thirdPersonCinemachineCamera.Priority = 11;

        if (skipPanel != null)
        {
            skipPanel.SetActive(false);
        }

        gameObject.SetActive(false); // Disable the showcase
    }

    void LookAtTarget(int index)
    {
        if (waypoints.Length > index && waypoints[index].childCount > 0)
        {
            Transform target = waypoints[index].GetChild(0);
            transform.LookAt(target);
        }
    }

    public void DeactivateShowcase()
    {
        StopAllCoroutines();
        isMoving = false;
        canSkip = false;
        gameObject.SetActive(false);
        thirdPersonCinemachineCamera.gameObject.SetActive(true);
        thirdPersonCharacter.SetActive(true);
    }

    Transform GetLookAtTarget(int index)
    {
        return waypoints.Length > index && waypoints[index].childCount > 0 ? waypoints[index].GetChild(0) : null;
    }
}
