using UnityEngine;

public class Evidence_Collectable : MonoBehaviour
{
    // EVIDENCE COLLECTABLE SCRIPT \\
    // This script handles the collection of evidence items by the player.

    [Header("Evidence Settings")]
    public string evidenceName; // Name of the evidence item
    public int scoreValue; // Score value of the evidence item
    public int capacityValue; // Capacity value of the evidence item
    
    [Header("Evidence Type")]
    public bool isKnife; // Flag to indicate if the evidence is a knife

    private void OnTriggerEnter(Collider other)
    {
        // Check if the "thing" hitting the evidence is the player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Collided");
            Roomba_Player roomba_Player = other.GetComponent<Roomba_Player>();

            if (roomba_Player != null)
            {
                bool collected = roomba_Player.CollectEvidence(this);
                
                if (collected)
                {
                    Debug.Log("Evidence Collected: " + evidenceName);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Roomba is full! Cannot pick up evidence.");
                }
            }
        }
    }
}
