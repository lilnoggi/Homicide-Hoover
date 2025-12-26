using UnityEngine;

public class Roomba_Cleaner : MonoBehaviour
{
    // === ROOMBA_CLEANER SCRIPT === \\
    // This script allows the Roomba to clean paintable floors by shooting
    // a ray downwards and interacting with the PaintableFloor script. \\

    [Header("Effects")]
    public ParticleSystem cleaningVFX;

    // State tracking for logging
    private bool wasCleaningBlood = false;

    // TIMER: Keeps the VFX alive for a momenet after cleaning stops
    private float vfxCooldownTimer = 0f;

    void Update()
    {
        // 1. Logic: Check the floor and calculate score
        bool isCleaningBlood = PerformCleaning();

        // 2. Debug: Log changes to console
        //HandleLogging(isCleaningBlood);

        // 3. Visuals: Toggle the mist effect
        HandleVFX(isCleaningBlood);
    }

    // === CORE CLEANING LOGIC === \\
    bool PerformCleaning()
    {
        // Create a Mask that includes all layers EXCEPT the Player layer
        // [The ~ symbol meant "NOT" or "Inverse"]
        int layerMask = ~LayerMask.GetMask("Player");
        RaycastHit hit;

        Vector3 startPos = transform.position + Vector3.up;
        Vector3 direction = Vector3.down;

        Debug.DrawRay(startPos, direction * 2.0f, Color.green);

        if (Physics.Raycast(startPos, direction, out hit, 2.0f, layerMask))
        {
            //Debug.Log("Laser hit: " + hit.collider.name);

            // Did we hit a Paintable Floor?
            PaintableFloor floor = hit.collider.GetComponent<PaintableFloor>();

            if (floor != null)
            {
                // 1. Clean the floor and get the pixel count
                int pixelsCleaned = floor.CleanAt(hit.textureCoord);

                // 2. Add score based on pixels cleaned
                if (pixelsCleaned > 0)
                {
                    // Balance: Divide by 10 to reduce score gain
                    int scoreGain = Mathf.Max(1, pixelsCleaned / 10);

                    GameManager.Instance.AddScore(scoreGain);

                    return true; // Returns TRUE: Currently cleaning blood
                }
            }
        }

        return false; // Returns FALSE: Not cleaning blood
    }

    /*
    // === LOGGING LOGIC === \\
    void HandleLogging(bool isCleaning)
    {
        // Only log if the state has CHANGED from the last frame
        if (isCleaning != wasCleaningBlood)
        {
            if (isCleaning)
            {
                Debug.Log("<color=red>Status: CLEANING BLOOD</color>");
            }
            else
            {
                Debug.Log("<color=grey>Status: Stopped Cleaning</color>");
            }

            // Update the history for the next frame
            wasCleaningBlood = isCleaning;
        }
    }
    */

    // === VFX Logic === \\
    void HandleVFX(bool isCleaningNow)
    {
        if (cleaningVFX == null) return;

        // 1. If cleaning, Reset the timer to keep VFX on
        if (isCleaningNow)
        {
            vfxCooldownTimer = 0.2f; // Keep VFX alive for 0.2 seconds after cleaning stops
        }

        // 2. Count down the timer
        vfxCooldownTimer -= Time.deltaTime;

        // 3. Decide to Play or Stop based on the TIMER
        if (vfxCooldownTimer > 0)
        {
            if (!cleaningVFX.isPlaying)
            {
                cleaningVFX.Play();
            }
        }
        else
        {
            if (cleaningVFX.isPlaying)
            {
                cleaningVFX.Stop();
            }
        }
    }
}
