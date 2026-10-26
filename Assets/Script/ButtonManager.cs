using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    // Call this to load your gameplay scene (replace "GameScene" with your actual scene name)
    public void PlayGame(string Game)
    {
        SceneManager.LoadScene(Game);
    }

    // Call this to restart the current match/scene
    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Call this to quit the game application
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}