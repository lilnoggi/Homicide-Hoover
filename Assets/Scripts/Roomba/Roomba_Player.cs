using System.Collections;
using UnityEngine;

public class Roomba_Player : MonoBehaviour

//_____________________________________\\
// ROOMBA_PLAYER SCRIPT                        \\_______\\
// This script handles the Roomba movement logic. \\
// It also manages dust collection, furniture      \\
// collisions, and disposal mechanics.              \\
//___________________________________________________\\
{
    // === VARIABLES === \\
    [Header("Movement Settings")]
    private const float baseMoveSpeed = 10f; // Base movement speed
    [SerializeField] private float moveSpeed = baseMoveSpeed; // How fast the Roomba can move
    [SerializeField] private float rotationSpeed = 150f; // How fast the Roomba can rotate (for tank controls)
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 1f;
    private Rigidbody rb; // Reference to the Rigidbody component
    private float moveInput; // Forward/backward input
    private float rotateInput; // Left/right rotation input

    [Header("Status Variables")]
    public int currentCapacity;            // Current dust capacity
    public int maxCapacity = 10;            // Maximum dust capacity before Roomba needs to empty
    [SerializeField] int hits;                 // Amount of times the player has hit furniture                

    [Header("State Flags")]
    private bool isBroken = false; // Prevents speed from being reset while broken
    private bool isSlowed = false; // Prevents the next fix from being overridden
    public bool isFull = false;  // New flag for bag status
    private bool isEmptying = false; // Prevents multiple emptying actions
    public bool playerDetection = false; // Whether the player is in the disposal area
    public bool isDashing = false;
    public bool canDash = true;

    [Header("References")]
    public GameObject disposalArea;     // Reference to the disposal area object
    public GameObject binBagPrefab;    // Prefab for the bin bag to instantiate
    public GameObject suctionVFXPrefab;        // VFX prefab for dust suction effect
    public GameObject promptCanvas;          // UI canvas for prompts 
    [SerializeField] private Cinemachine_Shake Cinemachine_Shake; // Reference to the Cinemachine shake script
    [SerializeField] private TrailRenderer dashTrail; // Reference to the dash trail renderer

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip[] furnitureHitSounds;
    public AudioClip vacuumLoop, vacuumOff, vacuumOn, brokeDown, disposal, dash;

    [Header("Cinemachine Shake")]
    [SerializeField] private float shakeIntensity = 10f; // Intensity of the shake
    [SerializeField] private float shakeTime = 5f;    // Duration of the shake

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component

        audioSource = GetComponent<AudioSource>();

        // === LOGIC FOR AUDIO LOOPING === \\
        if (audioSource != null && vacuumLoop != null)
        {
            audioSource.clip = vacuumLoop; // Set the clip to looping
            audioSource.loop = true;      // Enable looping
            audioSource.Play();          // Start the loop
        }

        // REMOVED: Camera.main reference is no longer needed for tank controls
    }

    void Update()
    {
        HandleDisposalInput(); // Check for disposal input

        // === TANK CONTROLS INPUT === \\
        // W/S moves Forward/Backward
        moveInput = Input.GetAxis("Vertical"); // Get forward/backward input
        // A/D rotates Left/Right
        rotateInput = Input.GetAxis("Horizontal"); // Get left/right rotation input

        // Rotate the Roomba immeditaely based on input
        if (!isBroken && !isEmptying)
        {
            float turnAmount = rotateInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, turnAmount, 0f);
        }

        MoveRoomba(); // Handle Roomba movement
    }

    // === ROOMBA MOVEMENT === \\
    void MoveRoomba()
    {
        // --- Only allow movement if not broken or emptying --- \\
        if (isBroken || isEmptying)
        {
            rb.linearVelocity = Vector3.zero; // Stop movement
            return; // Exit the method early if broken
        }

        // 1. Calculate the "Normal" intended speed
        float targetSpeed = baseMoveSpeed;

        // --- Apply slowdown capacity penalty --- \\
        if (currentCapacity >= maxCapacity)
        {
            targetSpeed = 5f;
            isFull = true;
        }
        else
        {
            isFull = false;
        }

        // --- Apply slowdown from dust collection --- \\
        if (isSlowed)
        {
            targetSpeed = 2f;
        }

        // 2. Dash Override
        // If dashing, set speed to dash speed
        if (isDashing)
        {
            targetSpeed = dashSpeed;
        }

        moveSpeed = targetSpeed;

        // --- PHYSICS MOVEMENT --- \\
        // Move in the direction the Roomba is currently FACING
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // Calculate velocity based on forward vector
            Vector3 desiredVelocity = transform.forward * moveInput * moveSpeed;

            // Apply velocity
            rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);
        }
        else
        {
            // Stop ONLY horizontal movement, gravity acts
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // --- Dash Input --- \\
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBroken && !isEmptying && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

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

    // === COLLIDE WITH FURNITURE === \\
    private void OnCollisionEnter(Collision other) // CHANGED from private int to private void
    {
        if (other.gameObject.CompareTag("Furniture") && !isBroken)
        {
            hits++; // If the player collides with an object, the hit counter increases by 1.
            GameManager.Instance.RegisterFurnitureHit(currentCapacity, maxCapacity); // Notify GameManager of furniture hit

            PlayRandomSound(); // Play a random furniture hit sound


            Debug.Log($"Bad Roomba! You hit: {hits} pieces of furniture!"); // Console output.

            GetComponentInChildren<MeshRenderer>().material.color = Color.red; // The roomba changes red.

            StartCoroutine(ChangeColour()); // Start the colour change coroutine.

            // --- CHECK FOR BREAKAGE --- \\
            if (hits >= 3)
            {
                isBroken = true; // Set broken state to true
                // Stop the current vacuum sound instantly
                audioSource.Stop();

                // Start the entire break/fix sequence
                StartCoroutine(BreakVacuumSequence(4.5f)); // 5 is the duration of the "break"

                // Reset the hit limit
                hits = 0;
            }
        }
    }

    // === TRIGGER ENTRY || Dust & Disposal === \\
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
            currentCapacity++; // Increase current capacity
            GameManager.Instance.CollectDust(); // Notify GameManager of dust collection
            GameManager.Instance.UpdateUI(currentCapacity, maxCapacity); // Update UI with new capacity

            // --- VFX LOGIC --- Instantiate and destroy in one flow
            GameObject vfx = Instantiate(suctionVFXPrefab, other.transform.position, Quaternion.identity);
            Destroy(vfx, 2f); // Destroy the VFX after 2 seconds

            StartCoroutine(SlowDown()); // Start slowdown coroutine
            PlaySound(pickupSound); // Play pickup sound
        }

        // === COLLIDE WITH DISPOSAL AREA === \\
        if (other.gameObject == disposalArea)
        {
            promptCanvas.SetActive(true); // Show prompt UI
            playerDetection = true; // Player is in disposal area
            Debug.Log("In disposal area. Press 'E' to empty dust bag.");
        }
    }

    // === TRIGGER EXIT || Disposal Area === \\
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

    // === AUDIO === \\
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

        // 1. Get a random index from 0 up to the array length.
        int randomIndex = UnityEngine.Random.Range(0, furnitureHitSounds.Length);

        // 2. Get the specific clip using the random index.
        AudioClip randomClip = furnitureHitSounds[randomIndex];

        // 3. Play the clip
        PlaySound(randomClip);
    }

    // === COROUTINES === \\
    IEnumerator ChangeColour()
    {
        yield return new WaitForSeconds(1); // Delay before changing back

        // --- Return to original colour only if not currently in the BreakVaccuumSequence --- \\
        if (!isBroken)
        {
            GetComponentInChildren<MeshRenderer>().material.color = Color.aquamarine; // Change roomba back to orginal colour
        }
    }

    // --- Slow player down when dust is collected --- \\
    IEnumerator SlowDown()
    {
        isSlowed = true; // Set the slowed flag to true

        moveSpeed = 2f; // Reduce speed

        yield return new WaitForSeconds(1); // Wait for 1 second

        isSlowed = false; // Reset the slowed flag

        moveSpeed = 10f; // Restore speed
    }

    // --- The main sequence for stopping and restarting the vacuum --- \\
    IEnumerator BreakVacuumSequence(float delay)
    {
        PlaySound(brokeDown);
        // 1. Enter the "broken" state
        // Temporarily set speed to 0 to stop movement
        moveSpeed = 0;

        // Play the vacuum OFF sound
        PlaySound(vacuumOff);

        GetComponentInChildren<MeshRenderer>().material.color = Color.black; // Change roomba to red to indicate broken state

        // 2. Wait for the break period
        yield return new WaitForSeconds(delay);

        // 3. Exit the "broken" state and repair

        // Play the vacuum on sound
        PlaySound(vacuumOn);

        GetComponentInChildren<MeshRenderer>().material.color = Color.green; // Change roomba to green to indicate repair

        // Wait for the vacuum ON sound to finish
        yield return new WaitForSeconds(0.5f);

        // 4. Return to the normal game state

        GetComponentInChildren<MeshRenderer>().material.color = Color.aquamarine; // Change roomba back to normal colour

        isBroken = false; // Reset broken state

        // Resume movement speed
        moveSpeed = 10;

        // Restart the looping vacuum sound
        if (audioSource != null && vacuumLoop != null) // Safety check
        {
            audioSource.clip = vacuumLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
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

        Cinemachine_Shake.ShakeCamera(shakeIntensity, shakeTime); // Start camera shake effect

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

        moveSpeed = baseMoveSpeed; // Resume normal speed

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

    // === END OF COROUTINES === \\
}
