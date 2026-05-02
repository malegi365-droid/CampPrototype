using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Audio Source Pool")]
    [SerializeField] private int poolSize = 8;
    [SerializeField] private float defaultVolume = 1f;

    [Header("Anti-Spam Settings")]
    [SerializeField] private float defaultMinInterval = 0.04f;

    [Header("Debug")]
    [SerializeField] private AudioClip debugTestClip;
    [SerializeField] private KeyCode debugPlayKey = KeyCode.T;
    [SerializeField] private KeyCode emergencyResetKey = KeyCode.Y;

    private AudioSource[] sources;
    private int sourceIndex = 0;

    private readonly Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AudioListener.volume = 1f;

        BuildAudioSourcePool();
    }

    private void Update()
    {
        if (Input.GetKeyDown(debugPlayKey))
        {
            PlayUISound(debugTestClip, 1f, 0f);
        }

        if (Input.GetKeyDown(emergencyResetKey))
        {
            ResetAudioPool();
        }
    }

    private void BuildAudioSourcePool()
    {
        sources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = defaultVolume;
            source.mute = false;
            source.priority = 64;

            sources[i] = source;
        }

        Debug.Log($"[GameAudioManager] Built audio pool with {poolSize} sources.");
    }

    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        PlayUISound(clip, volume, defaultMinInterval);
    }

    public void PlayUISound(AudioClip clip, float volume, float minInterval)
    {
        if (clip == null)
            return;

        if (WasPlayedTooRecently(clip, minInterval))
            return;

        AudioSource source = GetNextSource();

        source.Stop();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.volume = Mathf.Clamp01(volume);
        source.mute = false;
        source.pitch = 1f;
        source.Play();
    }

    public void PlayWorldSound(AudioClip clip, Vector3 position, float volume = 1f)
    {
        PlayUISound(clip, volume, defaultMinInterval);
    }

    private bool WasPlayedTooRecently(AudioClip clip, float minInterval)
    {
        if (minInterval <= 0f)
            return false;

        if (lastPlayTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
                return true;
        }

        lastPlayTimes[clip] = Time.time;
        return false;
    }

    private AudioSource GetNextSource()
    {
        AudioSource source = sources[sourceIndex];

        sourceIndex++;
        if (sourceIndex >= sources.Length)
            sourceIndex = 0;

        return source;
    }

    public void ResetAudioPool()
    {
        AudioListener.volume = 1f;

        if (sources != null)
        {
            foreach (AudioSource source in sources)
            {
                if (source == null)
                    continue;

                source.Stop();
                source.volume = defaultVolume;
                source.mute = false;
                source.pitch = 1f;
            }
        }

        lastPlayTimes.Clear();

        Debug.Log("[GameAudioManager] Audio pool reset.");
    }
}