using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HitSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool logWhenPlayed = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for testing
        audioSource.volume = 1f;
        audioSource.mute = false;
    }

    public void PlayHit()
    {
        if (hitClip == null)
        {
            Debug.LogWarning($"{gameObject.name} has no hit clip assigned.");
            return;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.PlayOneShot(hitClip, volume);

        if (logWhenPlayed)
            Debug.Log($"{gameObject.name} played hit sound: {hitClip.name}");
    }
}