using UnityEngine;
using UnityEngine.SceneManagement;  // For loading and quitting scenes

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Reference to the pause menu canvas
    private bool isPaused = false;  // Track whether the game is paused

    void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();  // If already paused, resume the game
            else
                Pause();  // If not paused, pause the game
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);  // Hide the pause menu
        Time.timeScale = 1f;  // Resume the game (normal time scale)
        isPaused = false;  // Update paused state
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);  // Show the pause menu
        Time.timeScale = 0f;  // Freeze the game (pause)
        isPaused = true;  // Update paused state
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");  // Log in the editor for debugging
        Application.Quit();  // Quit the game (works only in a built game)
    }
}
