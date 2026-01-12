using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Roomba_Health : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Visual Damage")]
    public ParticleSystem smokeVFX;
    //public GameObject explosionVFXPrefab;

    [Header("UI References")]
    public Image healthBarFill; // Drag the Red Bar here
    public Image faceUI; // Drag the Face Image here
    public Sprite[] faceStates; // 0=Happy, 1=Hurt, 2=Critical

    // References
    private Animator anim;
    private Roomba_Player playerMovement;
    private Rigidbody rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        playerMovement = GetComponent<Roomba_Player>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        anim.SetBool("IsDead", false);
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
            StartCoroutine(Die());
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

    IEnumerator Die()
    {
        playerMovement.enabled = false; // Disable movement

        // Explosion VFX here...

        anim.SetBool("IsDead", true); // Trigger death animation

        yield return new WaitForSeconds(5f); // Wait for death animation

        // Tell GameManager to end game
        GameManager.Instance.TriggerGameOver();

        // Disable script
        this.enabled = false;
    }
}
