using UnityEngine;

//________________________________\\
//  DIRECTION INDICATOR SCRIPT  \\
// This script is for the direction indicator functionality. \\
// When player capacity reaches 5, the direction indicator will point towards the disposal zone. \\
//________________________________\\

public class DirectionIndicator : MonoBehaviour
{
    public GameObject indicator; // Reference to the direction indicator GameObject

    [SerializeField] private Transform disposalZone; // Reference to the disposal zone transform

    private Roomba_Player roombaScript; // Reference to the Mover script

    private void Start()
    {
        roombaScript = GameObject.FindWithTag("Player").GetComponent<Roomba_Player>(); // Get the Mover script from the player
    }

    private void Update()
    {
        Indicate(); // Check if the indicator should be shown or hidden

        IndicatorPosition(); // Update the indicator's position to point towards the disposal zone
    }

    // === INDICATOR VISIBILITY LOGIC === \\
    void Indicate()
    {
        if (roombaScript.currentCapacity >= 5)
                    {
            indicator.SetActive(true); // Show the direction indicator
        }
        else
        {
            indicator.SetActive(false); // Hide the direction indicator
        }
    }

    // === INDICATOR POSITIONING LOGIC === \\
    void IndicatorPosition()
    {
        var targetPosition = disposalZone.position; // Get the position of the disposal zone
        targetPosition.y = transform.position.y; // Keep the indicator's y position unchanged
        transform.LookAt(targetPosition); // Make the indicator point towards the disposal zone
    }
}
