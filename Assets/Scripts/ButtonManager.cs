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
    public GameObject knifeCollectedPanel; 

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
