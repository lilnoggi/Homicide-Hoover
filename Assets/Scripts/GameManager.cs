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
    public TextMeshProUGUI hitsCounter;
    public GameObject gameWonCanvas;
    public GameObject gameOverPanel;

    [Header("Hotbar UI")]
    public Image dashUI;
    public Image senseUI;

    [Header("HUD References")]
    public Image capacityBarFill;
    public Image damageBarFill;

    [Header("Script References")]
    private ButtonManager buttonManager;
    private Roomba_Player roombaPlayer;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        buttonManager = FindAnyObjectByType<ButtonManager>();
        roombaPlayer = FindAnyObjectByType<Roomba_Player>();
    }

    private void Start()
    {
        InitIcons();

        // Safe Update
        if (roombaPlayer != null)
        {
            UpdateUI(roombaPlayer.currentCapacity, roombaPlayer.maxCapacity);
            UpdateDamageBar(10);
        }
        else
        {
            UpdateUI(0, 10);
        }
    }

    void InitIcons()
    {
        if (dashUI != null) { dashUI.fillAmount = 1; dashUI.color = Color.white; }
        if (senseUI != null) { senseUI.fillAmount = 1; senseUI.color = Color.white; }
        
        // Reset bars to empty/full as per design
        if (capacityBarFill != null) { capacityBarFill.fillAmount = 0; }
        if (damageBarFill != null) { damageBarFill.fillAmount = 1; }
    }

    // === GAME LOGIC === \\

    public void AddScore(int amount)
    {
        score += amount;
        // Auto-fetch capacity so we don't reset the UI to 0/10 by accident
        if (roombaPlayer != null) 
            UpdateUI(roombaPlayer.currentCapacity, roombaPlayer.maxCapacity);
        else 
            UpdateUI(); // Fallback
    }

    public void CollectDust()
    {
        dustCollected++;
        AddScore(20);
    }

    public void FoundKnife()
    {
        hasMurderWeapon = true;
        if (buttonManager != null)
        {
            buttonManager.knifeCollectedPanel.SetActive(true);
            TogglePause(true);
        }
        UpdateUI(roombaPlayer.currentCapacity, roombaPlayer.maxCapacity);
    }

    // === UI UPDATES === \\

    public void UpdateUI(int currentCapacity = 0, int maxCapacity = 10)
    {
        if (scoreCounter) scoreCounter.text = $"Score: {score}";
        if (dustCounter) dustCounter.text = $"Dust Collected: {dustCollected}/{totalDustRequired}";

        if (capacityBarFill != null)
        {
            // Convert to 0.0 - 1.0 percentage
            float fillAmount = (float)currentCapacity / maxCapacity;
            capacityBarFill.fillAmount = fillAmount;
        }
    }

    public void UpdateDamageBar(int currentHits)
    {
        if (damageBarFill != null)
        {
            float maxHits = 3;
            damageBarFill.fillAmount = (float)currentHits / maxHits;
        }
    }

    public void RegisterFurnitureHit(int cap, int max)
    {
        furnitureHits++;
        AddScore(-500); // This calls UpdateUI automatically now
    }
     
    // === COOLDOWN ANIMATIONS === \\

    public void TriggerDashCooldownUI(float duration, float cooldown)
    {
        if (dashUI != null) StartCoroutine(AnimateIcon(dashUI, duration, cooldown));
    }

    public void TriggerSenseCooldownUI(float duration, float cooldown)
    {
        if (senseUI != null) StartCoroutine(AnimateIcon(senseUI, duration, cooldown));
    }

    // Combined Animation Coroutine (Reused for both Dash and Sense)
    private IEnumerator AnimateIcon(Image icon, float activeTime, float cooldownTime)
    {
        // Active State
        icon.color = Color.gray;
        icon.fillAmount = 1;
        yield return new WaitForSeconds(activeTime);

        // Cooldown State
        icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        icon.fillAmount = 0;

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            icon.fillAmount = timer / cooldownTime;
            yield return null;
        }

        // Ready State
        icon.fillAmount = 1;
        icon.color = Color.white;
    }

    // === WIN STATE === \\

    public void CheckWinCondition(int currentRoombaCapacity)
    {
        if (dustCollected >= totalDustRequired && hasMurderWeapon && currentRoombaCapacity == 0)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        gameWonCanvas.SetActive(true);
        TogglePause(true);
        Debug.Log("Evidence Disposed. Case Closed!");
    }

    // Helper to pause/unpause without repeating code
    void TogglePause(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pause;
    }

    public void TriggerGameOver()
    {
        Debug.Log("Game Over!");
        if (gameOverPanel != null)
        {
            //gameOverPanel.SetActive(true);
            TogglePause(true);
        }
    }
}