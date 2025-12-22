using UnityEngine;

public class Roomba_Cleaner : MonoBehaviour
{
    // === ROOMBA_CLEANER SCRIPT === \\
    // This script allows the Roomba to clean paintable floors by shooting
    // a ray downwards and interacting with the PaintableFloor script. \\

    [Header("Effects")]
    public ParticleSystem cleaningVFX;

    void Update()
    {
        // Create a Mask that includes all layers EXCEPT the Player layer
        // [The ~ symbol meant "NOT" or "Inverse"]
        int layerMask = ~LayerMask.GetMask("Player");

        // Shoot a ray DOWN from the Roomba
        RaycastHit hit;

        bool isCleaningBlood = false; // Track if cleaning blood this frame

        Vector3 startPos = transform.position + Vector3.up;
        Vector3 direction = Vector3.down;

        Debug.DrawRay(startPos, direction * 2.0f, Color.green);

        if (Physics.Raycast(startPos, direction, out hit, 2.0f, layerMask))
        {
            Debug.Log("Laser hit: " + hit.collider.name);

            // Did we hit a Paintable Floor?
            PaintableFloor floor = hit.collider.GetComponent<PaintableFloor>();

            if (floor != null)
            {
                // 1. Clean the floor and get the pixel count
                int pixelsCleaned = floor.CleanAt(hit.textureCoord);

                // 2. Add score based on pixels cleaned
                if (pixelsCleaned > 0)
                {
                    isCleaningBlood = true; // We are cleaning blood this frame

                    // Balance: Divide by 10 to reduce score gain
                    int scoreGain = Mathf.Max(1, pixelsCleaned / 10);

                    GameManager.Instance.AddScore(scoreGain);
                }
            }
        }

        // === VFX MANAGEMENT === \\
        HandleVFX(isCleaningBlood);
    }

    void HandleVFX(bool isActive)
    {
        if (cleaningVFX == null) return;

        if (isActive)
        {
            // If cleaning but VFX not playing, turn it on
            if (!cleaningVFX.isPlaying)
            {
                cleaningVFX.Play();
            }
        }
        else
        {
            // If stopped cleaning but VFX playing, turn it off
            if (cleaningVFX.isPlaying)
            {
                cleaningVFX.Stop();
            }
        }
    }
}
