using UnityEngine;
using Cinemachine;

public class Roomba_CameraSwitch : MonoBehaviour
{
    public Transform roombaCharacter; // Reference to the Roomba character
    public CinemachineVirtualCamera activeCam; // Reference to the active virtual camera

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
