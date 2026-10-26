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

    [Header("Audio Settings")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float footstepInterval = 0.35f; // Time between footsteps
    private float footstepTimer = 0f;

    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 movementInput = Vector2.zero;
    private bool isMoving = false;
    private bool isTackling = false;
    private bool isKnockedBack = false;
    private bool hasBall = false;
    private Ball currentBall = null;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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

    private void Update()
    {
        HandleFootsteps();
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
            if (hasBall && currentBall != null)
            {
                currentBall.ShootBall();
                PlayerLoseBall();
            }
        }
    }

    // Automatically called when the tackle button is pressed
    public void OnTackle(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            if (ButtonMashing.Instance != null && ButtonMashing.Instance.IsDuelActive())
            {
                // Register a button mash during an active duel
                ButtonMashing.Instance.RegisterPress(gameObject);
            }
            else if (!isTackling && !isKnockedBack)
            {
                StartCoroutine(PerformTackle());
            }
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
            float rad = currentAngle * Mathf.Deg2Rad; // Fixed multiplication syntax here
            dashDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        float elapsedTime = 0f;
        while (elapsedTime < tackleDuration)
        {
            if (rb != null)
            {
                rb.linearVelocity = dashDirection.normalized * tackleForce;
            }
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        isTackling = false;
    }

    private void MovePlayer()
    {
        if (rb == null || isTackling || isKnockedBack) return;
        rb.linearVelocity = movementInput * moveSpeed;
    }

    private void HandleFootsteps()
    {
        if (isMoving && !isTackling && !isKnockedBack)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                if (audioSource != null && footstepClip != null)
                {
                    audioSource.PlayOneShot(footstepClip, 0.5f);
                }
                footstepTimer = 0f;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
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
                // Trigger the mash duel instead of instant release
                if (ButtonMashing.Instance != null)
                {
                    ButtonMashing.Instance.StartDuel(gameObject, collision.gameObject);
                }
            }
        }
    }

    public void ApplyKnockback(Vector2 force)
    {
        StartCoroutine(KnockbackRoutine(force));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        isTackling = false; // Cancel any active tackle state immediately

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Clear existing velocity for a clean push
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        // Stun duration where movement input is ignored
        yield return new WaitForSecondsRealtime(0.3f);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Stop drift after knockback ends
        }

        isKnockedBack = false;
    }

    // Forcefully unlocks the player so they never stay stuck
    public void ResetState()
    {
        isTackling = false;
        isKnockedBack = false;
        StopAllCoroutines(); // Stops any hanging dash or knockback routines
    }
}