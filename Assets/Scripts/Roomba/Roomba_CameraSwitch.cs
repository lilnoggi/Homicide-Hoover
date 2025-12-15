using UnityEngine;
using Cinemachine;

public class Roomba_CameraSwitch : MonoBehaviour
{
    // === ROOMBA_CAMERASWITCH SCRIPT === \\
    // This script switches the active camera when the player enters or exits the area. \\
    // Place this script in the trigger area where you want the camera switch to occur. \\
    // Use Ctrl + Shift + F to focus on the selected camera in the scene view. \\
    // __________________________________________\\


    // === VARIABLES === \\
    public Transform roombaCharacter; // Reference to the Roomba character
    public CinemachineVirtualCamera activeCam; // Reference to the active virtual camera

    private void Start()
    {
        // Ensure the camera starts with low priority
        activeCam.Priority = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCam.Priority = 1; // Set high priority to activate this camera
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeCam.Priority = 0; // Set low priority to deactivate this camera
        }
    }
}
