using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause menu starts hidden and game time is running
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Check for Keyboard Escape OR Controller Start button (using New Input System's Gamepad)
        bool escapePressed = Input.GetKeyDown(KeyCode.Escape);
        bool gamepadStartPressed = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (escapePressed || gamepadStartPressed)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ResumeGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Restore normal time
        isPaused = false;
    }

    public void PauseGame()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Freeze game time
        isPaused = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Always reset time scale before reloading a scene!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu(string MainMenu)
    {
        SceneManager.LoadScene(MainMenu);
    }
}