using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Progress")]
    public int score;
    public int furnitureHits;
    public int dustCollected;
    public int totalDustRequired = 20;
    public bool hasMurderWeapon;

    [Header("UI References")]
    public TextMeshProUGUI scoreCounter;
    public TextMeshProUGUI dustCounter;
    public TextMeshProUGUI capacityCounter;
    public TextMeshProUGUI hitsCounter;
    public GameObject gameWonCanvas;
    [Header("Hotbar UI References")]
    public Image dashUI;

    [Header("Script References")]
    private ButtonManager buttonManager;
    private Roomba_Player roombaPlayer;

    private void Awake()
    {
        // Singleton pattern: ensures only one GameManager exists
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        buttonManager = FindAnyObjectByType<ButtonManager>();
        roombaPlayer = FindAnyObjectByType<Roomba_Player>();
    }

    private void Start()
    {
        UpdateUI(0, 10);
    }

    // === SCORE METHODS === \\
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void CollectDust()
    {
        dustCollected++;
        AddScore(20);
        UpdateUI();
    }
    // === SCORE METHODS END === \\

    // === EVIDENCE === \\
    public void FoundKnife()
    {
        hasMurderWeapon = true;
        UpdateUI();

        buttonManager.knifeCollectedPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // === EVIDENCE END === \\

    // === UPDATE UI METHODS === \\
    public void UpdateUI(int currentCapacity = 0, int maxCapacity = 10)
    {
        if (scoreCounter) scoreCounter.text = $"Score: {score}";
        if (dustCounter) dustCounter.text = $"Dust Collected: {dustCollected}/{totalDustRequired}";
        if (capacityCounter) capacityCounter.text = $"Capacity: {currentCapacity}/{maxCapacity}";
        if (hitsCounter) hitsCounter.text = $"Furniture Hits: {furnitureHits}";
    }

    // --- Dash UI --- \\
    // Call this form Roomba_Player when dash starts
    public void TriggerDashCooldownUI(float dashTime, float cooldownTime)
    {
        if (dashUI != null)
        {
            StartCoroutine(AnimateDashUI(dashTime, cooldownTime));
        }
    }

    private IEnumerator AnimateDashUI(float dashTime, float cooldownTime)
    {
        // ACTIVE DASHING
        // Make icon dark
        dashUI.color = Color.gray;
        dashUI.fillAmount = 1;

        // Wait for dash to finish
        yield return new WaitForSeconds(dashTime);

        // COOLDOWN
        dashUI.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Semi-transparent gray
        dashUI.fillAmount = 0;

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            // Calulcate percentage
            float progress = timer / cooldownTime;

            // Update the UI fill
            dashUI.fillAmount = progress;

            yield return null;
        }

        // READY TO DASH
        dashUI.fillAmount = 1;
        dashUI.color = Color.white; // Back to normal color
    }
    // === UPDATE UI END === \\

    // === REGISTER FURNITURE HITS === \\
    public void RegisterFurnitureHit(int currentRoombaCapacity, int maxRoombaCapacity)
    {
        furnitureHits++;
        AddScore(-50);
        UpdateUI(currentRoombaCapacity, maxRoombaCapacity);
    }
    // === REGISTER FURNITURE HITS END === \\

    // === WIN CONDITION CHECK === \\
    public void CheckWinCondition(int currentRoombaCapacity)
    {
        if (dustCollected >= totalDustRequired && hasMurderWeapon && currentRoombaCapacity == 0)
        {
            WinGame();
        }
    }
    // === WIN CONDTION CHECK END === \\

    // === WIN GAME === \\
    public void WinGame()
    {
        gameWonCanvas.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Evidence Disposed. Case Closed!");
    }
    // === WIN GAME END === \\
}
