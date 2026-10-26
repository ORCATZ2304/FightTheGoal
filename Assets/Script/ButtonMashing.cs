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
    [SerializeField] private float shakeMagnitude = 0.08f; // Keep it subtle so it's a nice minor shake

    private bool isDuelActive = false;
    private GameObject playerWithBall;
    private GameObject playerTryingToTackle;

    private int ballHolderPresses = 0;
    private int tacklerPresses = 0;

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
        playerTryingToTackle = tackler;
        playerWithBall = ballHolder;
        ballHolderPresses = 0;
        tacklerPresses = 0;

        if (mashPanel != null) mashPanel.SetActive(true);
        if (dimOverlay != null) dimOverlay.SetActive(true);

        // START CONTINUOUS CAMERA SHAKE
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

        if (pressingPlayer == playerWithBall)
        {
            ballHolderPresses++;
        }
        else if (pressingPlayer == playerTryingToTackle)
        {
            tacklerPresses++;
        }
    }

    private void UpdateUI()
    {
        if (mashSlider == null) return;

        int totalPresses = tacklerPresses + ballHolderPresses;

        if (totalPresses == 0)
        {
            mashSlider.value = 0.5f;
        }
        else
        {
            float netDifference = tacklerPresses - ballHolderPresses;
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

        // STOP CAMERA SHAKE AND RESET CAMERA POSITION
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.StopShake();
        }

        Ball ball = FindObjectOfType<Ball>();

        Player tacklerScript = playerTryingToTackle != null ? playerTryingToTackle.GetComponent<Player>() : null;
        Player holderScript = playerWithBall != null ? playerWithBall.GetComponent<Player>() : null;

        if (tacklerScript != null) tacklerScript.ResetState();
        if (holderScript != null) holderScript.ResetState();

        bool tacklerWon = tacklerPresses >= ballHolderPresses;

        if (tacklerWon)
        {
            Debug.Log("Tackler Won the Duel!");
            if (ball != null) ball.ForceAttachTo(playerTryingToTackle);
            ApplyKnockback(playerWithBall, playerTryingToTackle);
        }
        else
        {
            Debug.Log("Ball Holder Won the Duel!");
            if (ball != null) ball.ForceAttachTo(playerWithBall);
            ApplyKnockback(playerTryingToTackle, playerWithBall);
        }
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