using UnityEngine;

//_____________________________________\\
//  DUST SPAWNER SCRIPT  \\
// This script spawns dust objects randomly within a defined area, avoiding furniture. \\
//_____________________________________\\

public class DustSpawner : MonoBehaviour
{
    public GameObject dustPrefab;
    public int numOfDust = 10; // no. of dust to spawn
    public float spawnRadius = 20f;
    public float spawnHeight = 0.5f;

    public float dustRadius = 0.5f; // Adjust based on the size of the dust prefab's collider
    public LayerMask furnitureLayer; // layer for furniture

    void Start()
    {
        SpawnDust();
    }

    // === DUST SPAWNING LOGIC === \\
    void SpawnDust()
    {
        const int maxAttempts = 50; // Max attempts to find a clear position for each dust

        for (int i = 0; i < numOfDust; i++) // Loop to spawn the specified number of dust
        {
            Vector3 spawnPos = Vector3.zero; // Initialize spawn position
            bool positionFound = false; // Flag to indicate if a valid position is found

            for (int attempt = 0; attempt < maxAttempts; attempt++) // Try to find a valid position
            {
                spawnPos = GetRandomSpawnPosition(); // Get a random position within the spawn radius

                // Check if the random position is clear of furniture
                if (IsPositionClear(spawnPos))
                {
                    positionFound = true;
                    break; // Exit the attempt loop as a position is found!
                }
            }

            if (positionFound) // If a valid position was found, spawn the dust
            {
                Instantiate(dustPrefab, spawnPos, Quaternion.identity); // Spawn the dust prefab at the found position
            }
            else
            {
                Debug.Log("Couldn't find a clear position for dust to spawn!"); // Log if no valid position was found after max attempts
            }
        }
    }

    // === CHECK IF POSITION IS CLEAR OF FURNITURE === \\
    private bool IsPositionClear(Vector3 position) // Check if the position is clear of furniture
    {
        // This checks for colliders within a sphere at the given position,
        // only checking objects on the furnitureLayer
        bool isOverlapping = Physics.CheckSphere(position, dustRadius, furnitureLayer); // Check for overlap with furniture

        // Return true if it is NOT overlapping
        return !isOverlapping; // Return true if position is clear
    }

    ///<summary>
    ///Generated a random position within a circular area around the spawner's transform.
    /// </summary>
    /// <returns>A random Vector3 position for the new coin.</returns>
    Vector3 GetRandomSpawnPosition()
    {
        // Random.insideUnitCircle returns a random point inside a 2D circle with radius 1.
        // Multiply this by the spawnRadius to stretch it to the desired size.
        Vector2 randomPoint2D = Random.insideUnitCircle * spawnRadius;

        // Take the spawner's world position as the center
        Vector3 center = transform.position;

        // Convert the 2D point (X, Y) into a 3D point (X, Height, Z)
        Vector3 randomPosition = center + new Vector3(randomPoint2D.x, spawnHeight, randomPoint2D.y);

        return randomPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.DrawCube(transform.position, spawnHeight * Vector3.up);
    }
}
