using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roomba_Sense : MonoBehaviour
{
    [Header("Settings")]
    public float senseDuration = 5f;
    public float cooldown = 10f;
    public KeyCode activationKey = KeyCode.E;

    [Header("Target Tags")]
    // Add "Dust" & "Evidence" here in the Inspector
    public string[] tagsToScan = { "Dust", "Evidence" };

    private bool isSensing = false;
    private bool onCooldown = false;

    // Store original layers
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();

    // The ID of the special X-Ray layer
    private int scanLayerID;

    private void Start()
    {
        // Find the integer ID for the layer named "Scan"
        scanLayerID = LayerMask.NameToLayer("Scan");
    }

    private void Update()
    {
        if (Input.GetKeyDown(activationKey) && !isSensing && !onCooldown)
        {
            Debug.Log("Roomba Sense Activated!");
            StartCoroutine(ActivateSense());
        }
    }

    IEnumerator ActivateSense()
    {
        isSensing = true;

        // 1. Find Targets and Apply Layer
        HighlightObjects();

        // 2. Play Sound
        // Trigger dash UI cooldown in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerSenseCooldownUI(senseDuration, cooldown);
        }

        // 3. Wait
        yield return new WaitForSeconds(senseDuration);

        // 4. Revert Layers
        RevertObjects();

        isSensing = false;

        // 5. Start Cooldown
        onCooldown = true;
        Debug.Log("Roomba Sense on Cooldown.");
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    void HighlightObjects()
    {
        originalLayers.Clear();

        if (scanLayerID == -1)
        {
            Debug.LogError("CRITICAL ERROR: Layer 'Scan' does not exist in Project Settings");
            return;
        }

        // Loop through every tag
        foreach (string tag in tagsToScan)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            Debug.Log($"Found {targets.Length} objects with tag '{tag}' to highlight.");

            foreach (GameObject obj in targets)
            {
                // Save the original layer to restore it later
                // Check ContainsKey to avoid double-adding if an object has multiple tags
                if (!originalLayers.ContainsKey(obj))
                {
                    originalLayers.Add(obj, obj.layer);

                    // Apply the "Scan" layer
                    obj.layer = scanLayerID;
                }
            }
        }
    }

    void RevertObjects()
    {
        // Loop through saved list and restore everything
        foreach(KeyValuePair<GameObject, int> entry in originalLayers)
        {
            // Check if the object still exists
            if (entry.Key != null)
            {
                entry.Key.layer = entry.Value;
            }
        }

        originalLayers.Clear();
    }
}
