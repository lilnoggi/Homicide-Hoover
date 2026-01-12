using System.Collections;
using UnityEngine;

public class Roomba_Player : MonoBehaviour
{
    // ======================================================================== \\
    // ROOMBA PLAYER CONTROLLER
    // Handles Movement, Interactions, and State Management
    // ======================================================================== \\

    #region VARIABLES

    // === VARIABLES === \\
    [Header("Movement Settings")]
    private const float baseMoveSpeed = 10f; // Base movement speed
    [SerializeField] private float moveSpeed = baseMoveSpeed; // How fast the Roomba can move
    [SerializeField] private float rotationSpeed = 150f; // How fast the Roomba can rotate (for tank controls)

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Control Settings")]
    public bool useTankControls = true;

    [Header("Status Variables")]
    public int currentCapacity;            // Current dust capacity
    public int maxCapacity = 10;            // Maximum dust capacity before Roomba needs to empty               

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeMagnitude = 0.05f; // Magnitude of the shake effect
    [SerializeField] private float shakeDuration = 4.5f; // Duration of the Shake effect

    [Header("State Flags")]
    public bool playerDetection = false; // Whether the player is in the disposal area
    public bool isDashing = false;
    public bool canDash = true;
    private bool isBroken = false; // Prevents speed from being reset while broken
    private bool isSlowed = false; // Prevents the next fix from being overridden
    public bool isFull = false;  // New flag for bag status
    private bool isEmptying = false; // Prevents multiple emptying actions


    // INPUT STORAGE
    private Vector3 currentMovementVector;
    private float currentTurnAmount;
    private bool shouldAutoRotate;

    [Header("References")]
    public GameObject disposalArea;     // Reference to the disposal area object
    public GameObject binBagPrefab;    // Prefab for the bin bag to instantiate
    public GameObject suctionVFXPrefab;        // VFX prefab for dust suction effect
    public GameObject promptCanvas;          // UI canvas for prompts 
    [SerializeField] private TrailRenderer dashTrail; // Reference to the dash trail renderer

    // Components
    private Roomba_Health healthScript;
    private Rigidbody rb; // Reference to the Rigidbody component
    private AudioSource audioSource;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip[] furnitureHitSounds;
    public AudioClip vacuumLoop, vacuumOff, vacuumOn, brokeDown, disposal, dash;

    #endregion

    private void Awake()
    {
        healthScript = GetComponent<Roomba_Health>();

        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component

        audioSource = GetComponent<AudioSource>();

        // === LOGIC FOR AUDIO LOOPING === \\
        if (audioSource != null && vacuumLoop != null)
        {
            audioSource.clip = vacuumLoop; // Set the clip to looping
            audioSource.loop = true;      // Enable looping
            audioSource.Play();          // Start the loop
        }
    }

    // UPDATE: Get Input & Decisions Here
    void Update()
    {
        HandleDisposalInput(); // Check for disposal input
        CalculateMovementInput();
    }

    // FIXED UPDATE: Apply Physics Here
    private void FixedUpdate()
    {
        ApplyMovementPhysics();
    }

    #region MOVEMENT LOGIC

    void CalculateMovementInput()
    {
        if (isBroken || isEmptying)
        {
            currentMovementVector = Vector3.zero;
            currentTurnAmount = 0;
            return;
        }

        // Get Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // --- DASH INPUT --- \\
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isBroken && !isEmptying)
        {
            StartCoroutine(DashCoroutine());
        }

        // --- SPEED CALCULATION --- \\
        float targetSpeed = baseMoveSpeed;
        if (currentCapacity >= maxCapacity) targetSpeed = 5f;
        if (isSlowed) targetSpeed = 2f;
        if (isDashing) targetSpeed = dashSpeed;

        moveSpeed = targetSpeed;

        // --- CALCULATE VECTORS --- \\
        if (useTankControls)
        {
            // TANK: Rotate directly, Move forward
            currentTurnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            currentMovementVector = transform.forward * v * moveSpeed;
            shouldAutoRotate = false;
        }
        else
        {
            // MODERN: Move relative to camera
            Transform cam = Camera.main.transform;
            Vector3 camFwd = cam.forward;
            Vector3 camRt = cam.right;
            camFwd.y = 0; camRt.y = 0;
            camFwd.Normalize(); camRt.Normalize();

            Vector3 direction = (camFwd * v + camRt * h).normalized;
            currentMovementVector = direction * moveSpeed;
            currentTurnAmount = 0; // Handled by auto-rotate
            shouldAutoRotate = (direction.magnitude >= 0.1f);
        }
    }

    void ApplyMovementPhysics()
    {
        // 1. Rotation (Tank Mode)
        if (useTankControls && Mathf.Abs(currentTurnAmount) > 0)
        {
            // For physics rotation, MoveRotation is smoother
            Quaternion turnOffSet = Quaternion.Euler(0, currentTurnAmount * 50f, 0); // Multiplier for FixedDeltaTime feel
            rb.MoveRotation(rb.rotation * turnOffSet);
        }

        // 2. Movement (Both Modes)
        if (currentMovementVector.magnitude >= 0.1f || useTankControls)
        {
            // Apply velocity directly
            rb.linearVelocity = new Vector3(currentMovementVector.x, rb.linearVelocity.y, currentMovementVector.z);

            // 3. Rotation (Modern Mode Auto-Turn)
            if (shouldAutoRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentMovementVector);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Stop horizontal movement, keep gravity
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    #endregion

    #region INTERACTION LOGIC

    // === HANDLE DISPOSAL INPUT === \\
    void HandleDisposalInput()
    {
        if (playerDetection && Input.GetKeyDown(KeyCode.F))
        {
            if (currentCapacity > 0)
            {
                StartCoroutine(BagEmptying()); // Simulate bag emptying process
            }
            else
            {
                Debug.Log("Dust bag is already empty.");
            }
        }
    }

    public void AddDirtToCapacity()
    {
        if (isBroken || currentCapacity >= maxCapacity) return;

        currentCapacity++;
        GameManager.Instance.UpdateUI(currentCapacity, maxCapacity);

        // Visuals
        StartCoroutine(SlowDown());
        PlaySound(pickupSound);
    }

    // Collision for furniture bumping
    private void OnCollisionEnter(Collision other) 
    {
        if (other.gameObject.CompareTag("Furniture") && !isBroken)
        {
            if (healthScript != null)
            {
                healthScript.TakeDamage(1);

                // Check if still alive
                if (healthScript.currentHealth > 0)
                {
                    StartCoroutine(FlashRedSequence());
                }
            }

            PlayRandomSound();
        }
    }

    public void RepairStation()
    {
        if (healthScript != null) healthScript.RepairFull();
        PlaySound(pickupSound);
    }

    // Trigger for Dust & Zone Entry
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dust"))
        {
            // --- If Broken or Full, do not collect dust --- \\
            if (isBroken || currentCapacity >= maxCapacity)
            {
                Debug.Log(isBroken ? "Cannot collect dust while broken!" : "Dust bag is full! Please empty.");
                return; // Exit the method early
            }

            // --- Capacity Tracking --- \\
            GameManager.Instance.CollectDust(); // Notify GameManager of dust collection
            AddDirtToCapacity(); // Capacity update & Sound

            // --- VFX LOGIC --- Instantiate and destroy in one flow
            GameObject vfx = Instantiate(suctionVFXPrefab, other.transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Destroy the VFX after 2 seconds

            Destroy(other.gameObject); // Remove dust object
        }

        // === COLLIDE WITH DISPOSAL AREA === \\
        if (other.gameObject == disposalArea)
        {
            promptCanvas.SetActive(true); // Show prompt UI
            playerDetection = true; // Player is in disposal area
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == disposalArea)
        {
            promptCanvas.SetActive(false); // Hide prompt UI
            playerDetection = false; // Player has left disposal area
            Debug.Log("Left disposal area.");
        }
    }

    // === COLLECT EVIDENCE METHOD === \\
    public bool CollectEvidence(Evidence_Collectable evidence)
    {
        if (currentCapacity + evidence.capacityValue > maxCapacity)
        {
            return false; // Not enough capacity to collect evidence
        }

        currentCapacity += evidence.capacityValue; // Increase current capacity

        GameManager.Instance.AddScore(evidence.scoreValue); // Add score for collecting evidence
        GameManager.Instance.UpdateUI(currentCapacity, maxCapacity); // Update UI with new capacity

        if (evidence.isKnife)
        {
            GameManager.Instance.FoundKnife(); // Notify GameManager of knife collection
            Debug.Log("CRITICAL EVIDENCE FOUND: " + evidence.evidenceName);
        }

        GameObject vfx = Instantiate(suctionVFXPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, 2f); // Destroy the VFX after 2 seconds

        StartCoroutine(SlowDown()); // Start slowdown coroutine
        PlaySound(pickupSound); // Play pickup sound

        return true; // Evidence collected successfully
    }
    // === COLLECT EVIDENCE END === \\

    #endregion

    #region AUDIO HELPER

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) // Safety check
        {
            audioSource.PlayOneShot(clip); // Play the sound once
        }
    }

    void PlayRandomSound() // Play a random furniture hit sound
    {
        // --- SAFETY CHECK --- \\ 
        // If the array is null or empty, exit the method immediately
        if (furnitureHitSounds == null || furnitureHitSounds.Length == 0) // Safety check
        {
            return; // Exit the method if there are no sounds to play
        }

        PlaySound(furnitureHitSounds[Random.Range(0, furnitureHitSounds.Length)]);
    }

    #endregion

    #region COROUTINES

    IEnumerator SlowDown()
    {
        isSlowed = true; // Set the slowed flag to true

        yield return new WaitForSeconds(1); // Wait for 1 second

        isSlowed = false; // Reset the slowed flag
    }

    IEnumerator FlashRedSequence()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.red; // Change roomba to red on collision

        yield return new WaitForSeconds(0.5f); // Wait for half a second

        GetComponentInChildren<MeshRenderer>().material.color = Color.aquamarine; // Change roomba back to normal colour
    }

    // --- Bin Bag Disposal --- \\
    IEnumerator BagEmptying()
    {
        audioSource.Stop(); // Stop the current vacuum sound instantly

        isEmptying = true; // Set emptying flag to prevent multiple triggers

        moveSpeed = 0f; // Stop movement during emptying

        // Play the vacuum OFF sound
        PlaySound(vacuumOff);

        PlaySound(disposal);

        GetComponentInChildren<MeshRenderer>().material.color = Color.yellow; // Change roomba to yellow to indicate emptying

        StartCoroutine(ShakeRoomba());

        yield return new WaitForSeconds(5); // Simulate time taken to empty bag and audio to end

        // --- Core Logic --- \\
        EmptyBag(); // Call the empty bag method
        currentCapacity = 0; // Reset capacity after emptying
        GameManager.Instance.UpdateUI(currentCapacity, maxCapacity); // Update UI with new capacity
        // --- End of Core Logic --- \\

        // Play the vacuum on sound
        PlaySound(vacuumOn);

        GetComponentInChildren<MeshRenderer>().material.color = Color.green; // Change roomba to green to indicate successful emptying

        // Wait for the vacuum light to change
        yield return new WaitForSeconds(0.5f);

        // Return to the normal game state

        GetComponentInChildren<MeshRenderer>().material.color = Color.aquamarine; // Change roomba back to normal colour

        isEmptying = false; // Reset emptying flag

        // Restart the looping vacuum sound
        if (audioSource != null && vacuumLoop != null) // Safety check
        {
            audioSource.clip = vacuumLoop;
            audioSource.loop = true;
            audioSource.Play();
        }

        GameManager.Instance.CheckWinCondition(currentCapacity); // Check for win condition after emptying
    }

    // --- Dash Coroutine --- \\
    IEnumerator DashCoroutine()
    {
        isDashing = true; // Set dashing flag
        canDash = false; // Disable further dashes

        PlaySound(dash); // Play dash sound

        // Trigger dash UI cooldown in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerDashCooldownUI(dashDuration, dashCooldown);
        }

        // Trail effect \\
        dashTrail.emitting = true; // Enable trail effect
        yield return new WaitForSeconds(dashDuration); // Wait for dash duration
        dashTrail.emitting = false; // Disable trail effect

        isDashing = false; // Reset dashing flag
        yield return new WaitForSeconds(dashCooldown); // Wait for cooldown
        canDash = true; // Re-enable dashing
    }

    IEnumerator ShakeRoomba()
    {
        Vector3 originalPosition = transform.position; // Store the original position

        float elapsed = 0f; // Time elapsed
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            float zOffset = Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = originalPosition + new Vector3(xOffset, 0, zOffset); // Apply shake offset
            elapsed += Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }
        transform.position = originalPosition; // Reset to original position
    }

    #endregion

    // === EMPTY DUST BAG METHOD === \\
    void EmptyBag()
    {
        Debug.Log("Emptying dust bag...");

        SpawnBinBag(); // Spawn the bin bag
    }

    // === SPAWN BIN BAG METHOD === \\
    void SpawnBinBag()
    {
        if (binBagPrefab != null && disposalArea != null)
        {
            Vector3 spawnPosition = disposalArea.transform.position + new Vector3(0f, 1.5f, 0f); // Slightly above the disposal area
            Instantiate(binBagPrefab, spawnPosition, Quaternion.identity); // Spawn the bin bag prefab
        }
    }
}