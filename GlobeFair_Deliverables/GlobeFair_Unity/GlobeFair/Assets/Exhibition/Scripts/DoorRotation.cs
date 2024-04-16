using UnityEngine;

// Script that uses collider to activate a rotating door on player proximity
public class DoorRotation : MonoBehaviour
{
    public int Speed;
    private bool isPlayerNearby = false;

    // Checking every frame for the player
    void Update()
    {
        if (isPlayerNearby)
        {
            transform.Rotate(0, Speed * Time.deltaTime, 0);
        }
    }

    // Activating or deactivating the rotation
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
