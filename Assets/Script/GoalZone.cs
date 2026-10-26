using UnityEngine;

public class GoalZone : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private Transform centerResetPoint; // Drag an empty GameObject placed at the center of the pitch here

    [Header("Audio Settings")]
    [SerializeField] private AudioClip goalSound;       // Drag your cheer/goal sound here
    [SerializeField] private AudioSource audioSource;   // AudioSource to play the sound

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Case 1: Check if a player entered the goal holding the ball
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();
            Ball ballInScene = FindObjectOfType<Ball>();

            if (ballInScene != null && ballInScene.IsHeldBy(collision.gameObject))
            {
                TriggerGoal(ballInScene);
            }
        }
        // Case 2: Check if the ball itself entered the goal trigger directly (e.g. shot in)
        else if (collision.CompareTag("Ball"))
        {
            Ball ballInScene = collision.GetComponent<Ball>();
            if (ballInScene != null)
            {
                TriggerGoal(ballInScene);
            }
        }
    }

    private void TriggerGoal(Ball ballInScene)
    {
        Debug.Log("Goal scored! Playing celebration sound and resetting...");

        // Play the goal celebration sound effect
        if (audioSource != null && goalSound != null)
        {
            audioSource.PlayOneShot(goalSound);
        }

        // Reset the ball back to the center of the pitch
        ballInScene.ResetToCenter(centerResetPoint != null ? centerResetPoint.position : Vector3.zero);
    }
}