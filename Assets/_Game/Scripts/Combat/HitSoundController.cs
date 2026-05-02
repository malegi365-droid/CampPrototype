using UnityEngine;

public class HitSoundController : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float volume = 0.65f;
    [SerializeField] private float minIntervalBetweenHits = 0.08f;
    [SerializeField] private bool logWhenPlayed = false;

    public void PlayHit()
    {
        if (hitClip == null)
            return;

        if (GameAudioManager.Instance == null)
        {
            Debug.LogWarning($"{gameObject.name} could not find GameAudioManager.");
            return;
        }

        GameAudioManager.Instance.PlayUISound(hitClip, volume, minIntervalBetweenHits);

        if (logWhenPlayed)
            Debug.Log($"{gameObject.name} requested hit sound: {hitClip.name}");
    }
}