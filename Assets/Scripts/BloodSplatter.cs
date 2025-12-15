using UnityEngine;

//_______________________\\
// BLOOD SPLATTER SCRIPT \\
// This script manages the material and gradual cleaning process of a blood splatter. \\
// It allows for dynamic interaction with the blood splatter, enabling it to be cleaned over time.
//________________________________________________________________________________________\\
public class BloodSplatter : MonoBehaviour
{
    [Header("Settings")]
    public float cleanRate = 2.0f; // Rate at which the blood splatter is cleaned

    // === CAPACITY/SCORE DATA === \\
    private const int capacityCost = 2; // Capacity cost for the blood splatter
    private const int scoreValue = 50;    // Score value for cleaning the blood splatter

    // === INTERNAL DATA === \\
    private Renderer itemRenderer; // Renderer component of the blood splatter
    private Material bloodMaterial; // Material of the blood splatter
    private Roomba_Player roombaScript; // Reference to the Mover script
    private Coroutine cleanCoroutine; // Reference to the cleaning coroutine

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer == null)
        {
            Debug.LogError("Renderer component not found on BloodSplatter object.");
            return;
        }

        bloodMaterial = itemRenderer.material; // Create a unique instance of the material


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
