using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //_______________________\\_____________________\\
    // CAMERA MOVEMENT SCRIPT \\_____________________\\
    // This script handles the camera movement logic. \\
    // Holding K or L rotates the camera \\____________\\
    //__________________________________________________\\

    [Header("Settings")]
    public Transform targetToFollow; // Drag the pivot GameObject here (child of the Roomba)
    public Transform actualCamera;    // Drag the Main Camera here (child of the RIG)

    [Header("Speeds")]
    public float rotationSpeed = 100f;
    public float followSpeed = 5f;

    [Header("Collision")]
    public LayerMask collisionLayers; // Layers to consider for collision
    public float collisionRadius = 0.2f; // How far the camera WANTS to be
    public float minDistance = 1f; // Closet the camera can be
    public float camSmoothness = 10f; // How smooth the zoom is

    private Vector3 defaultLocalPos; // Initial setup position of the camera

    private void Start()
    {
        if (actualCamera != null)
        {
            defaultLocalPos = actualCamera.localPosition;
        }
    }

    private void LateUpdate()
    {
        if (targetToFollow == null) return;

        // 1. Follow the Roomba pos (The "Rig" moves to the player)
        transform.position = Vector3.Lerp(transform.position, targetToFollow.position, followSpeed * Time.deltaTime);

        // 2. Rotate based on K and L keys (The "Rig" rotates)
        float rotateInput = 0f;
        if (Input.GetKey(KeyCode.K))
            rotateInput = -1f; // Rotate Left
        if (Input.GetKey(KeyCode.L))
            rotateInput = 1f; // Rotate Right

        // Apply rotation to the RIG (parent)
        transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);

        // 3. HANDLE WALL COLLISIION (The "Camera" moves in/out)
        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        // Calculate the IDEAL position
        // Convert the local default position to world space
        Vector3 idealWorldPos = transform.TransformPoint(defaultLocalPos);
        Vector3 direction = (idealWorldPos - transform.position).normalized;
        float maxDistance = defaultLocalPos.magnitude;

        // Target distance starts at max
        float targetDist = maxDistance;
        RaycastHit hit;

        // Cast a thick sphere from the Rig towards the ideal camera position

        // If a wall is hit...
        if (Physics.SphereCast(transform.position, collisionRadius, direction, out hit, maxDistance, collisionLayers))
        {
            // Set target distance to hit point (minus a tiny buffer so no clipping occurs), clamped to minDistance
            targetDist = Mathf.Clamp(hit.distance - 0.1f, minDistance, maxDistance);
        }

        // Apply the distance to the Child Camera's local position
        Vector3 newLocalPos = defaultLocalPos.normalized * targetDist;

        actualCamera.localPosition = Vector3.Lerp(actualCamera.localPosition, newLocalPos, camSmoothness * Time.deltaTime);
    }
}
