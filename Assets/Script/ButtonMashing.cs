using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMashing : MonoBehaviour
{
    public static ButtonMashing Instance;

    [Header("Duel Settings")]
    [SerializeField] private float duelDuration = 1.5f;
    [SerializeField] private float knockbackForce = 6f;

    [Header("UI & Visual References")]
    [SerializeField] private GameObject mashPanel;
    [SerializeField] private GameObject dimOverlay;
    [SerializeField] private Slider mashSlider;
    [SerializeField] private RectTransform dialNeedle;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeMagnitude = 0.08f;

    private bool isDuelActive = false;
    private GameObject player1;
    private GameObject player2;
    private GameObject playerTryingTackle;

    private int player1Presses = 0;
    private int player2Presses = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (mashPanel != null) mashPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);
    }

    private void Update()
    {
        if (!isDuelActive) return;
        UpdateUI();
    }

    public void StartDuel(GameObject tackler, GameObject ballHolder)
    {
        if (isDuelActive) return;

        isDuelActive = true;
        playerTryingTackle = tackler; // Save reference properly here

        // Find players using modern Unity 6 methods
        Player[] allPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player p in allPlayers)
        {
            if (p.gameObject.name.Contains("Player2"))
            {
                player2 = p.gameObject;
            }
            else
            {
                player1 = p.gameObject;
            }
        }

        player1Presses = 0;
        player2Presses = 0;

        if (mashPanel != null) mashPanel.SetActive(true);
        if (dimOverlay != null) dimOverlay.SetActive(true);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StartContinuousShake(shakeMagnitude);
        }

        Time.timeScale = 0f;

        Debug.Log("TACKLE DUEL STARTED! MASH YOUR BUTTONS!");
        StartCoroutine(DuelRoutine());
    }

    public void RegisterPress(GameObject pressingPlayer)
    {
        if (!isDuelActive) return;

        if (pressingPlayer == player1)
        {
            player1Presses++;
        }
        else if (pressingPlayer == player2)
        {
            player2Presses++;
        }
    }

    private void UpdateUI()
    {
        if (mashSlider == null) return;

        int totalPresses = player1Presses + player2Presses;

        if (totalPresses == 0)
        {
            mashSlider.value = 0.5f;
        }
        else
        {
            float netDifference = player1Presses - player2Presses;
            float normalizedVal = 0.5f + (netDifference / 20f);
            mashSlider.value = Mathf.Clamp01(normalizedVal);
        }

        if (dialNeedle != null)
        {
            RectTransform sliderRect = mashSlider.GetComponent<RectTransform>();
            float sliderWidth = sliderRect.rect.width;
            float posX = (mashSlider.value - 0.5f) * sliderWidth;
            dialNeedle.anchoredPosition = new Vector2(posX, dialNeedle.anchoredPosition.y);
        }
    }

    private IEnumerator DuelRoutine()
    {
        float timer = 0f;
        while (timer < duelDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        ResolveDuel();
    }

    private void ResolveDuel()
    {
        isDuelActive = false;
        Time.timeScale = 1f;

        if (mashPanel != null) mashPanel.SetActive(false);
        if (dimOverlay != null) dimOverlay.SetActive(false);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StopShake();
        }

        Ball ball = Object.FindFirstObjectByType<Ball>();

        bool player1Won = player1Presses >= player2Presses;
        GameObject winner = player1Won ? player1 : player2;
        GameObject loser = player1Won ? player2 : player1;

        // Fixed variable name from 'tackler' to 'playerTryingTackle'
        Player tacklerScript = playerTryingTackle != null ? playerTryingTackle.GetComponent<Player>() : null;

        Player[] allPlayers = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player p in allPlayers)
        {
            p.ResetState();
        }

        if (ball != null)
        {
            ball.ForceAttachTo(winner);
        }

        ApplyKnockback(loser, winner);
    }

    private void ApplyKnockback(GameObject loser, GameObject winner)
    {
        Player loserPlayer = loser.GetComponent<Player>();
        if (loserPlayer != null)
        {
            Vector2 knockbackDir = (loser.transform.position - winner.transform.position).normalized;
            loserPlayer.ApplyKnockback(knockbackDir * knockbackForce);
        }
    }

    public bool IsDuelActive() => isDuelActive;
}