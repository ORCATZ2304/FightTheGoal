using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Ensure crowd is playing when match starts
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // Call this method if you want to stop the crowd (e.g., half-time, game over)
    public void StopCrowd()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    // Call this method to resume the crowd noise
    public void StartCrowd()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}