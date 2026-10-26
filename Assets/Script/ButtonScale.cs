using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scaling Settings")]
    [SerializeField] private float hoverScale = 1.15f; // How much bigger it gets (1.15 = 15% larger)
    [SerializeField] private float scaleSpeed = 10f;    // How fast it scales up/down

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // Smoothly animate the button's scale toward the target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Triggered when mouse enters the button
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Triggered when mouse leaves the button
        targetScale = originalScale;
    }
}