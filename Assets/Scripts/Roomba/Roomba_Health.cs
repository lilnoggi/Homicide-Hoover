using UnityEngine;
using UnityEngine.UI;

public class Roomba_Health : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Visual Damage")]
    public ParticleSystem smokeVFX;
    public GameObject explosionVFXPrefab;

    [Header("UI References")]
    public Image healthBarFill; // Drag the Red Bar here
    public Image faceUI; // Drag the Face Image here
    public Sprite[] faceStates; // 0=Happy, 1=Hurt, 2=Critical

    private void Start()
    {
        currentHealth = maxHealth;
        if (smokeVFX != null) smokeVFX.Stop();
        UpdateVisuals();
    }

    // === PUBLIC METHODS === \\
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateVisuals();

        // Check for death
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void RepairFull()
    {
        currentHealth = maxHealth;
        UpdateVisuals();
    }

    // === INTERNAL METHODS === \\
    void UpdateVisuals()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        // Update Health Bar
        if (healthBarFill != null)
            healthBarFill.fillAmount = healthPercent;

        // Update Face Expression
        if (faceUI != null && faceStates.Length >= 3)
        {
            if (healthPercent > 0.66f) faceUI.sprite = faceStates[0]; // Happy
            else if (healthPercent > 0.33f) faceUI.sprite = faceStates[1]; // Hurt
            else faceUI.sprite = faceStates[2]; // Critical
        }

        // Update Smoke VFX (Start smoking at 33% health)
        if (smokeVFX != null)
        {
            if (healthPercent <= 0.33f && !smokeVFX.isPlaying) smokeVFX.Play();
            else if (healthPercent > 0.33f && smokeVFX.isPlaying) smokeVFX.Stop();
        }
    }

    void Die()
    {
        // Visual Explosion
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        // Hide Roomba
        GetComponentInChildren<MeshRenderer>().enabled = false;
        smokeVFX.Stop();

        // Tell GameManager to end game
        GameManager.Instance.TriggerGameOver();

        // Disable script
        this.enabled = false;
    }
}
