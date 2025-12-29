using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//________________________________\\
//  BUTTON MANAGER SCRIPT  \\
// This script manages button interactions, tutorial panel display, and scene transitions. \\
//________________________________\\

public class ButtonManager : MonoBehaviour
{
    public GameObject tutorialPanel; // Reference to the tutorial panel UI
    public GameObject settingsPanel;
    public GameObject audioSettingsPanel;
    public GameObject knifeCollectedPanel;

    public UnityEngine.UI.Toggle controlsToggle;

    private Roomba_Player roombaPlayer;

    private void Start()
    {
        roombaPlayer = FindAnyObjectByType<Roomba_Player>();

        // Sync the visual checkbox to the actual setting
        if (controlsToggle != null && roombaPlayer != null)
        {
            controlsToggle.isOn = roombaPlayer.useTankControls;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Check if the Escape key is pressed
        {
            PauseGame(); // Call the method to pause the game and show the tutorial panel
        }
    }

    // === TUTORIAL PANEL MANAGEMENT === \\
    void PauseGame() 
    {
        tutorialPanel.SetActive(true); // Show the tutorial panel
        Time.timeScale = 0f; // Pause the game

        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible
    }

    // === SETTINGS === \\
    public void SettingsButton()
    {
        settingsPanel.SetActive(true); // Show the settings panel
    }

    public void SetTankControls(bool isTank)
    {
        if (roombaPlayer != null)
        {
            roombaPlayer.useTankControls = isTank;
            Debug.Log("Controls set to " + (isTank ? "Tank" : "Standard"));
        }
    }

    // --- Audio --- \\
    public void OpenAudioPanel()
    {
        audioSettingsPanel.SetActive(true);
    }

    public void ExitAudioPanel()
    {
        audioSettingsPanel.SetActive(false);
    }

    public void ExitSettingsPanel()
    {
        settingsPanel.SetActive(false); // Hide the settings panel
    }
    // === END OF SETTINGS === \\

    // === TUTORIAL BUTTON HANDLER === \\
    public void TutorialButton()
    {
        tutorialPanel.SetActive(false); // Hide the tutorial panel
        Time.timeScale = 1f; // Resume the game

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor
    }

    // === Evidence Panel Button Handler === \\
    public void EvidenceButton()
    {
        knifeCollectedPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game

        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
        Cursor.visible = false; // Hide the cursor
    }

    // === SCENE MANAGEMENT === \\

    public void LoadGame()
    {
        SceneManager.LoadScene(1); // Load the main game scene
    }

    // === QUIT APPLICATION === \\
    public void QuitGame()
    {
        Debug.Log("Exit Application."); // Log message for quitting the game
        Application.Quit(); // Quit the application
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0); // Load the main menu scene
    }
}
