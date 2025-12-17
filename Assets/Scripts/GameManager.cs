using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Progress")]
    public int score;
    public int dustCollected;
    public int totalDustRequired = 20;
    public bool hasMurderWeapon;

    [Header("UI References")]
    public TextMeshProUGUI scoreCounter;
    public TextMeshProUGUI dustCounter;
    public TextMeshProUGUI capacityCounter;
    public GameObject gameWonCanvas;
    // Move other UI references here !!!

    private void Awake()
    {
        // Singleton pattern: ensures only one GameManager exists
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
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
    }

    // === EVIDENCE END === \\

    // === UPDATE UI METHODS === \\
    void UpdateUI()
    {
        if (scoreCounter) scoreCounter.text = $"Score: {score}";
        if (dustCounter) dustCounter.text = $"Dust Collected: {dustCollected}/{totalDustRequired}";
    }
    // === UPDATE UI END === \\

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
