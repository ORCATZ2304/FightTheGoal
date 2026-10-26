using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float shootForce = 15f;

    // Set this to match the same offset as your Player.cs (-90f, +90f, or 180f)
    [SerializeField] private float angleOffset = -90f;

    [Header("Attachment Settings")]
    [SerializeField] private Transform ballSocket;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioSource audioSource;

    private Rigidbody2D rb;
    private Transform bodyTransform;
    private GameObject currentHolder = null; // Tracks which specific player is holding the ball
    private bool isAttached = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (rb == null)
            Debug.LogError("Rigidbody2D missing on Ball!");
    }

    private void Update()
    {
        if (isAttached)
        {
            FollowSocket();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Play impact sound when the ball collides with enough speed
        if (audioSource != null && hitClip != null && rb.linearVelocity.magnitude > 1f)
        {
            audioSource.PlayOneShot(hitClip, Mathf.Clamp01(rb.linearVelocity.magnitude / 10f));
        }

        if (!isAttached && collision.gameObject.CompareTag("Player"))
        {
            AttachToPlayer(collision.gameObject);
        }
    }

    private void AttachToPlayer(GameObject player)
    {
        // If someone else held it previously, clear them first
        if (currentHolder != null && currentHolder != player)
        {
            Player prevPlayer = currentHolder.GetComponent<Player>();
            if (prevPlayer != null) prevPlayer.PlayerLoseBall();
        }

        isAttached = true;
        currentHolder = player; // Register current holder

        // Notify the player script that they successfully obtained the ball
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.PlayerGetBall(this);
        }

        // Find Body visual child
        Transform foundBody = player.transform.Find("Body");
        bodyTransform = foundBody != null ? foundBody : player.transform;

        // Look for BallHolding or BallSocket under Body
        if (ballSocket == null)
        {
            Transform foundSocket = bodyTransform.Find("BallHolding");
            if (foundSocket == null) foundSocket = bodyTransform.Find("BallSocket");
            ballSocket = foundSocket != null ? foundSocket : bodyTransform;
        }

        // Freeze physics on ball while held
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void FollowSocket()
    {
        if (ballSocket != null)
        {
            transform.position = ballSocket.position;
        }
    }

    // Public method for players to check if this ball belongs to them
    public bool IsHeldBy(GameObject player)
    {
        return isAttached && currentHolder == player;
    }

    // Public method triggered when a tackle successfully hits the ball holder
    public void ForceRelease()
    {
        if (currentHolder != null)
        {
            Player holderScript = currentHolder.GetComponent<Player>();
            if (holderScript != null) holderScript.PlayerLoseBall();
        }

        isAttached = false;
        currentHolder = null;

        // Unfreeze physics simulation so it bounces away
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Push the ball loose in a random direction upon being tackled
        Vector2 looseDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.AddForce(looseDirection * 6f, ForceMode2D.Impulse);

        ballSocket = null;
    }

    public void ShootBall()
    {
        if (currentHolder != null)
        {
            Player holderScript = currentHolder.GetComponent<Player>();
            if (holderScript != null) holderScript.PlayerLoseBall();
        }

        isAttached = false;
        currentHolder = null;

        // Unfreeze physics simulation
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Play kick sound effect when the ball is shot
        if (audioSource != null && hitClip != null)
        {
            audioSource.PlayOneShot(hitClip, 1f);
        }

        // Calculate actual facing direction by rotating local angle by angleOffset
        float currentAngle = bodyTransform.eulerAngles.z - angleOffset;
        float rad = currentAngle * Mathf.Deg2Rad; // Fixed multiplication syntax
        Vector2 facingDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // Apply shoot force in the corrected facing direction
        rb.AddForce(facingDirection.normalized * shootForce, ForceMode2D.Impulse);

        ballSocket = null;
    }

    // Force attaches the ball to a specific player (used after duel resolution)
    public void ForceAttachTo(GameObject player)
    {
        // Reset state completely before re-attaching to the winner
        if (currentHolder != null && currentHolder != player)
        {
            Player prevPlayer = currentHolder.GetComponent<Player>();
            if (prevPlayer != null) prevPlayer.PlayerLoseBall();
        }

        isAttached = false;
        currentHolder = null;
        ballSocket = null;

        // Now attach fresh to the winner
        AttachToPlayer(player);
    }

    // Resets the ball to the center of the field and strips player possession (used by GoalZone)
    public void ResetToCenter(Vector3 centerPosition)
    {
        if (currentHolder != null)
        {
            Player holderScript = currentHolder.GetComponent<Player>();
            if (holderScript != null)
            {
                holderScript.PlayerLoseBall();
            }
        }

        isAttached = false;
        currentHolder = null;
        ballSocket = null;

        // Unfreeze physics and move to center
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = centerPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}