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
    public float rotationSpeed = 100f;
    public float followSpeed = 5f;

    private void LateUpdate()
    {
        if (targetToFollow == null) return;

        // 1. Follow the Roomba (Pos only, NOT rotation)
        transform.position = Vector3.Lerp(transform.position, targetToFollow.position, followSpeed * Time.deltaTime);

        // 2. Rotate based on K and L keys
        float rotateInput = 0f;
        if (Input.GetKey(KeyCode.K))
            rotateInput = -1f; // Rotate Left
        if (Input.GetKey(KeyCode.L))
            rotateInput = 1f; // Rotate Right

        // Apply rotation to the RIG (parent)
        transform.Rotate(Vector3.up, rotateInput * rotationSpeed * Time.deltaTime);
    }
}
