using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float shootForce = 15f;
    //[SerializeField] private KeyCode keyboardShootKey = KeyCode.Space;
    //[SerializeField] private KeyCode controllerShootKey = KeyCode.JoystickButton2;

    // Set this to match the same offset as your Player.cs (-90f, +90f, or 180f)
    [SerializeField] private float angleOffset = -90f;

    [Header("Attachment Settings")]
    [SerializeField] private Transform ballSocket;

    private Rigidbody2D rb;
    private Transform bodyTransform;
    private GameObject currentHolder = null; // Tracks which specific player is holding the ball
    private bool isAttached = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

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
        if (!isAttached && collision.gameObject.CompareTag("Player"))
        {
            AttachToPlayer(collision.gameObject);
            collision.gameObject.GetComponent<Player>().PlayerGetBall(this);
        }
    }

    private void AttachToPlayer(GameObject player)
    {
        isAttached = true;
        currentHolder = player; // Register current holder

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
        isAttached = false;
        currentHolder = null;

        // Unfreeze physics simulation
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Calculate actual facing direction by rotating local angle by angleOffset
        float currentAngle = bodyTransform.eulerAngles.z - angleOffset;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 facingDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // Apply shoot force in the corrected facing direction
        rb.AddForce(facingDirection.normalized * shootForce, ForceMode2D.Impulse);

        ballSocket = null;
    }
}