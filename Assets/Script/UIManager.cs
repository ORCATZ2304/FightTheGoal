using UnityEngine;
using TMPro; // Use UnityEngine.UI if you are using standard Text instead of TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Game Over Panel (Optional)")]
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            // Update Score Display
            int homeScore = GameManager.Instance.GetScoreHome();
            int awayScore = GameManager.Instance.GetScoreAway();
            scoreText.text = $"{homeScore} - {awayScore}";

            // Update Timer Display (Convert seconds to MM:SS format)
            float currentTime = GameManager.Instance.GetCurrentTimer();
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // Check if Match is Over
            if (GameManager.Instance.IsMatchOver())
            {
                if (gameOverPanel != null && !gameOverPanel.activeSelf)
                {
                    gameOverPanel.SetActive(true);
                }
            }
        }
    }
}