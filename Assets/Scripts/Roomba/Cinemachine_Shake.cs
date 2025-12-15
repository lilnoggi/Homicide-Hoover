using UnityEngine;
using Cinemachine;
using System.Collections;

public class Cinemachine_Shake : MonoBehaviour
{
    // === CINEMACHINE_SHAKE SCRIPT === \\
    // This script adds a camera shake effect using Cinemachine. \\
    // Attach this script to a Cinemachine Virtual Camera. \\
    // This script is used for when the Roomba disposes of trash. \\
    // __________________________________\\

    private CinemachineVirtualCamera virtualCamera3; // Reference to the Cinemachine Virtual Camera
    private CinemachineBasicMultiChannelPerlin perlinNoise; // Reference to the Perlin Noise component

    private void Awake()
    {
        virtualCamera3 = GetComponent<CinemachineVirtualCamera>(); // Get the Cinemachine Virtual Camera component

        perlinNoise = virtualCamera3.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); // Get the Perlin Noise component

        ResetIntensity(); // Initialize the shake intensity to zero
    }

    public void ShakeCamera(float intensity, float time)
    {
        perlinNoise.m_AmplitudeGain = intensity; // Set the shake intensity
        StartCoroutine(WaitTime(time)); // Start the coroutine to reset the intensity after the specified time
    }

    IEnumerator WaitTime(float shakeTime)
    {
        yield return new WaitForSeconds(shakeTime); // Wait for the specified shake time
        perlinNoise.m_AmplitudeGain = 0f; // Reset the shake intensity to zero
    }

    void ResetIntensity()
    {
        perlinNoise.m_AmplitudeGain = 0f; // Reset the shake intensity to zero
    }
}
