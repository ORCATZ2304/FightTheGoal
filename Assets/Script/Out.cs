using UnityEngine;

public class Out : MonoBehaviour
{
    [Header("Teleport Target")]
    [Tooltip("Leave unassigned to automatically target (0,0) in world space.")]
    [SerializeField] private Transform customCenterPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndResetBall(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CheckAndResetBall(other.gameObject);
    }

    private void CheckAndResetBall(GameObject obj)
    {
        // Verify if the colliding object is the Ball
        if (obj.CompareTag("Ball") || obj.GetComponent<Ball>() != null)
        {
            Rigidbody2D ballRb = obj.GetComponent<Rigidbody2D>();

            // Determine target center position
            Vector3 targetPosition = customCenterPoint != null
                ? customCenterPoint.position
                : Vector3.zero; // (0,0,0) is world center

            // Teleport ball
            obj.transform.position = targetPosition;

            // Reset velocity and momentum completely
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector2.zero; // Unity 6 (use .velocity for older versions)
                ballRb.angularVelocity = 0f;
            }
        }
    }
}