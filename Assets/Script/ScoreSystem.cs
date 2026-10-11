using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [Header("Which team scored when the ball hits this goal?")]
    [SerializeField] private string scoringTeam = "Home"; // Set to "Away" for the opposite goal

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball") || other.GetComponent<Ball>() != null)
        {
            // Add score, reset ball, and stop timer briefly or handle match logic
            if (GameManager.Instance != null)
            {
                GameManager.Instance.addScore(scoringTeam);
            }
        }
    }
}