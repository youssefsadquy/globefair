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

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[waypointIndex].position;
            if (GetLookAtTarget(waypointIndex) != null)
            {
                transform.LookAt(GetLookAtTarget(waypointIndex));
            }
        }

        thirdPersonCharacter.SetActive(false);
        thirdPersonCinemachineCamera.gameObject.SetActive(false);
        
        if (skipPanel != null)
        {
            skipPanel.gameObject.SetActive(true);
        }
    }


    void Update()
    {
        if (isMoving && waypoints.Length > 0)
        {
            MoveThroughWaypoints();
        }
        if (canSkip && Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(TransitionToThirdPersonCharacter());
            canSkip = false;
        }
    }

    void MoveThroughWaypoints()
    {
        if (waypointIndex < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[waypointIndex];
            Vector3 targetPosition = targetWaypoint.position;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, speed);

            if (GetLookAtTarget(waypointIndex) != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(GetLookAtTarget(waypointIndex).position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                waypointIndex++;
                if (waypointIndex >= waypoints.Length)
                {
                    isMoving = false;
                    StartCoroutine(TransitionToThirdPersonCharacter());
                }
            }
        }
    }

    IEnumerator TransitionToThirdPersonCharacter()
    {
        yield return new WaitForSeconds(2f);

        thirdPersonCharacter.SetActive(true);
        thirdPersonCinemachineCamera.gameObject.SetActive(true);
        thirdPersonCinemachineCamera.Priority = 11;

        gameObject.SetActive(false);

        if (skipPanel != null)
        {
            skipPanel.gameObject.SetActive(false);
        }
    }

    Transform GetLookAtTarget(int index)
    {
        if (waypoints[index].childCount > 0)
        {
            return waypoints[index].GetChild(0);
        }
        return null;
    }
}
