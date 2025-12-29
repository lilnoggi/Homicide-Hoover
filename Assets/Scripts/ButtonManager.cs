using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject tutorialPanel;
    public GameObject settingsPanel;
    public GameObject audioSettingsPanel;
    public GameObject knifeCollectedPanel;

    [Header("UI Elements")]
    public UnityEngine.UI.Toggle controlsToggle;

    private Roomba_Player roombaPlayer;

    private void Start()
    {
        roombaPlayer = FindAnyObjectByType<Roomba_Player>();

        // Sync toggle if it exists
        if (controlsToggle != null && roombaPlayer != null)
        {
            controlsToggle.isOn = roombaPlayer.useTankControls;
        }
    }

    void Update()
    {
        // Simple Toggle for Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialPanel.activeSelf) TutorialButton(); // Resume
            else PauseGame(); // Pause
        }
    }

    // === PAUSE LOGIC === \\
    void PauseGame()
    {
        tutorialPanel.SetActive(true);
        SetGameState(false); // Pause
    }

    public void TutorialButton() // Resume Button
    {
        tutorialPanel.SetActive(false);
        SetGameState(true); // Resume
    }

    public void EvidenceButton() // Close Evidence Panel
    {
        knifeCollectedPanel.SetActive(false);
        SetGameState(true); // Resume
    }

    // Helper to handle Time and Cursor together
    void SetGameState(bool isPlaying)
    {
        Time.timeScale = isPlaying ? 1f : 0f;
        Cursor.lockState = isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPlaying;
    }

    // === SETTINGS MENU === \\
    public void SettingsButton()
    {
        settingsPanel.SetActive(true);
    }

    public void ExitSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    public void SetTankControls(bool isTank)
    {
        if (roombaPlayer != null)
        {
            roombaPlayer.useTankControls = isTank;
            Debug.Log("Controls set to " + (isTank ? "Tank" : "Modern"));
        }
    }

    // === AUDIO SETTINGS === \\
    public void OpenAudioPanel()
    {
        audioSettingsPanel.SetActive(true);
    }

    public void ExitAudioPanel()
    {
        audioSettingsPanel.SetActive(false);
    }

    // === SCENE MANAGEMENT === \\
    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("Exit Application.");
        Application.Quit();
    }
}