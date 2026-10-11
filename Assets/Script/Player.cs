using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float deadzone = 0.2f;

    [Header("Tackle Settings")]
    [SerializeField] private float tackleForce = 12f;
    [SerializeField] private float tackleDuration = 0.2f;

    [Header("Sprite Orientation")]
    [SerializeField] private float angleOffset = -90f;

    [Header("Visual Container")]
    [SerializeField] private Transform bodyTransform;

    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 movementInput = Vector2.zero;
    private bool isMoving = false;
    private bool isTackling = false;
    private bool hasBall = false;
    private Ball currentBall = null;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (bodyTransform == null)
        {
            Transform foundBody = transform.Find("Body");
            bodyTransform = foundBody != null ? foundBody : transform;
        }

        if (animator != null)
        {
            animator.SetBool(IsMoving, false);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    // Automatically called by Player Input component (Send Messages behavior)
    public void OnMove(InputAction.CallbackContext value)
    {
        Vector2 rawInput = value.ReadValue<Vector2>();

        if (rawInput.magnitude < deadzone)
        {
            movementInput = Vector2.zero;
            isMoving = false;
        }
        else
        {
            movementInput = rawInput.normalized;
            isMoving = true;
        }

        UpdateAnimator();
        HandleRotation();
    }

    public void PlayerGetBall(Ball ball)
    {
        hasBall = true;
        currentBall = ball;
    }

    public void PlayerLoseBall()
    {
        hasBall = false;
        currentBall = null;
    }

    public void OnShoot(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            if(hasBall && currentBall != null)
            {
                // Implement shooting logic here
                currentBall.ShootBall();    
                PlayerLoseBall();
            }
        }
    }

    // Automatically called when the tackle button is pressed
    public void OnTackle(InputAction.CallbackContext value)
    {
        if (value.performed && !isTackling)
        {
            StartCoroutine(PerformTackle());
        }
    }

    private IEnumerator PerformTackle()
    {
        isTackling = true;

        // Dash in current movement direction, or forward relative to body rotation if standing still
        Vector2 dashDirection = movementInput;
        if (dashDirection == Vector2.zero && bodyTransform != null)
        {
            float currentAngle = bodyTransform.eulerAngles.z - angleOffset;
            float rad = currentAngle * Mathf.Deg2Rad;
            dashDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        float elapsedTime = 0f;
        while (elapsedTime < tackleDuration)
        {
            if (rb != null)
            {
                rb.linearVelocity = dashDirection.normalized * tackleForce;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isTackling = false;
    }

    private void MovePlayer()
    {
        if (rb == null || isTackling) return;
        rb.linearVelocity = movementInput * moveSpeed;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        if (isMoving)
        {
            animator.SetFloat(MoveX, movementInput.x);
            animator.SetFloat(MoveY, movementInput.y);
        }

        animator.SetBool(IsMoving, isMoving);
    }

    private void HandleRotation()
    {
        if (!isMoving || bodyTransform == null) return;

        float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
        bodyTransform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isTackling && collision.gameObject.CompareTag("Player"))
        {
            Ball ballInScene = FindObjectOfType<Ball>();
            if (ballInScene != null && ballInScene.IsHeldBy(collision.gameObject))
            {
                ballInScene.ForceRelease();

                // Apply slight knockback to the tackled opponent
                Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
                    targetRb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
                }
            }
        }
    }
}