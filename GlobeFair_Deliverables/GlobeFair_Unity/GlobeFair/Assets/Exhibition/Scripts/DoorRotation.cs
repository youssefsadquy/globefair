using UnityEngine;

public class DoorRotation : MonoBehaviour
{
    public int Speed;
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby)
        {
            transform.Rotate(0, Speed * Time.deltaTime, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
