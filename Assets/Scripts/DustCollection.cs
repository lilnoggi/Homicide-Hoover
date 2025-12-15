using UnityEngine;

//_____________________________________\\
//  DUST COLLECTION SCRIPT  \\
// This script handles the collection of dust objects by the player. \\
//_____________________________________\\

public class DustCollection : MonoBehaviour
{
    private Roomba_Player roombaScript; // Reference to the Mover script

    private void Start()
    {
        roombaScript = GameObject.FindWithTag("Player").GetComponent<Roomba_Player>(); // Get the Mover script from the player
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && roombaScript.isFull == false) // Check if the collider is the player and capacity is not full
        {
            Destroy(gameObject); // Destroy the dust object upon collision with the player
        }
        else
        {
            Debug.Log("Cannot collect dust, capacity full!"); // Log message if capacity is full
        }
    }
}
