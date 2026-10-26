using UnityEngine;
using UnityEngine.UI;

public class ButtonMashingDial : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mashPanel;     // The parent UI panel
    [SerializeField] private Slider mashSlider;        // The volume-style bar (Value 0 to 1)
    [SerializeField] private RectTransform dialNeedle; // The needle image that shifts left/right

    [Header("Mash Settings")]
    [SerializeField] private float pointsPerPress = 0.1f; // How much progress a press adds
    [SerializeField] private float drainSpeed = 0.2f;     // How fast the meter falls back to center if inactive
    [SerializeField] private float winThreshold = 1f;     // Value required to win (-1 for Player 2, +1 for Player 1)

    private float currentProgress = 0f; // Ranges from -1 (Player 2 win) to +1 (Player 1 win)
    private bool isMinigameActive = false;

    private void Start()
    {
        if (mashPanel != null) mashPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isMinigameActive) return;

        // Automatically drain progress back toward 0 (center) over time if players slow down
        if (currentProgress > 0)
            currentProgress -= drainSpeed * Time.deltaTime;
        else if (currentProgress < 0)
            currentProgress += drainSpeed * Time.deltaTime;

        currentProgress = Mathf.Clamp(currentProgress, -1f, 1f);
        UpdateUI();

        // Check for win condition
        if (currentProgress >= winThreshold)
        {
            ResolveMash(1); // Player 1 wins
        }
        else if (currentProgress <= -winThreshold)
        {
            ResolveMash(2); // Player 2 wins
        }
    }

    // Call this to start the minigame (e.g., during a tackle duel)
    public void StartButtonMash()
    {
        isMinigameActive = true;
        currentProgress = 0f;
        if (mashPanel != null) mashPanel.SetActive(true);
    }

    // Call this whenever Player 1 mashes their assigned button
    public void Player1Mash()
    {
        if (!isMinigameActive) return;
        Debug.Log($"{name}: Player 1 mashed!");
        currentProgress += pointsPerPress;
    }

    // Call this whenever Player 2 mashes their assigned button
    public void Player2Mash()
    {
        if (!isMinigameActive) return;
        Debug.Log($"{name}: Player 2 mashed!");
        currentProgress -= pointsPerPress;
    }

    private void UpdateUI()
    {
        if (mashSlider != null)
        {
            // Map currentProgress from [-1, 1] to slider range [0, 1] (0.5 is middle)
            mashSlider.value = (currentProgress + 1f) / 2f;
        }

        if (dialNeedle != null && mashSlider != null)
        {
            // Move the dial visually along the width of the slider
            RectTransform sliderRect = mashSlider.GetComponent<RectTransform>();
            float sliderWidth = sliderRect.rect.width;

            // Calculate X position based on slider value
            float posX = (mashSlider.value - 0.5f) * sliderWidth;
            dialNeedle.anchoredPosition = new Vector2(posX, dialNeedle.anchoredPosition.y);
        }
    }

    private void ResolveMash(int winnerPlayer)
    {
        isMinigameActive = false;
        if (mashPanel != null) mashPanel.SetActive(false);

        Debug.Log("Player " + winnerPlayer + " won the button mash duel!");

        // TODO: Trigger your game logic here (e.g., Player 1 wins ball back, Player 2 keeps it, etc.)
    }
}