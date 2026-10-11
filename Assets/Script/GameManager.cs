using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance so other scripts (like goals or UI) can access it easily
    public static GameManager Instance { get; private set; }

    [Header("Match Variables")]
    [SerializeField] private int scoreHome = 0;
    [SerializeField] private int scoreAway = 0;
    [SerializeField] private float matchDuration = 300f; // e.g., 5 minutes (in seconds)
    [SerializeField] private bool isMatchOver = false;
    [SerializeField] private Transform CenterPoint;

    private float currentTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startGame();
    }

    private void Update()
    {
        if (!isMatchOver)
        {
            updateTimer();
        }
    }

    // + startGame(): void
    public void startGame()
    {
        scoreHome = 0;
        scoreAway = 0;
        currentTimer = matchDuration;
        isMatchOver = false;
        Debug.Log("Match Started!");
    }

    // + updateTimer(): void
    public void updateTimer()
    {
        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                triggerGameOver();
            }
        }
    }

    // + addScore(team: String): void
    public void addScore(string team)
    {
        if (isMatchOver) return;

        if (team.ToLower() == "home")
        {
            scoreHome++;
            Debug.Log("Home Scored! Current Score - Home: " + scoreHome + " | Away: " + scoreAway);
        }
        else if (team.ToLower() == "away")
        {
            scoreAway++;
            Debug.Log("Away Scored! Current Score - Home: " + scoreHome + " | Away: " + scoreAway);
        }

        // Reset ball to middle after scoring
        ResetBallToCenter();
    }

    // + triggerGameOver(): void
    public void triggerGameOver()
    {
        isMatchOver = true;
        Debug.Log("Match Over!");
    }

    private void ResetBallToCenter()
    {
        // Find the ball in the scene by tag or component
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            // Teleport to center (0,0,0)
            ball.transform.position = CenterPoint.position;

            // Reset ball physics velocity
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    // Public getters to read private variables in UI or other scripts
    public int GetScoreHome() => scoreHome;
    public int GetScoreAway() => scoreAway;
    public float GetCurrentTimer() => currentTimer;
    public bool IsMatchOver() => isMatchOver;
}