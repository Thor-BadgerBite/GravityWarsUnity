using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip missileLaunchSFX;
    public AudioClip missileFlySFX;
    public AudioClip missileDestroyedSFX;
    public AudioClip shipExplosionSFX;

    [Header("Engine Loop")]
    public AudioClip engineLoopSFX;
    public float engineMinPitch = 0.8f;
    public float engineMaxPitch = 2f;
    public float engineMinVolume = 0.2f;
    public float engineMaxVolume = 1f;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("Ship Move SFX")]
    public AudioClip slingshotSFX; // Assign your warp .wav in Inspector

    [Header("Volume Controls")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Header("3D Sound Settings")]
    public float minDistance = 1f;
    public float maxDistance = 20f;
    [Range(0f, 1f)]
    public float spatialBlend = 1f; // 0 = 2D, 1 = 3D

    private AudioSource sfxSource;
    private AudioSource musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void SetupAudioSources()
    {
        // Setup SFX audio source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // Keep general SFX as 2D
        
        // Setup music audio source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f; // Keep music as 2D
        
        UpdateVolumes();
        musicSource.Play();
    }

    void Update()
    {
        UpdateVolumes();
    }

    void UpdateVolumes()
    {
        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
    }

    // Method to create a 3D audio source on a game object
    public AudioSource Setup3DAudioSource(GameObject gameObject)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // force 2D temporarily
        audioSource.minDistance = 0f;
        audioSource.maxDistance = 500f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.volume = sfxVolume;
        return audioSource;
    }

    // 2D sound effects
    public void PlayMissileLaunch()
    {
        PlaySoundEffect(missileLaunchSFX);
    }

    public void PlayMissileDestroyed()
    {
        PlaySoundEffect(missileDestroyedSFX);
    }

    public void PlayShipExplosion()
    {
        PlaySoundEffect(shipExplosionSFX);
    }

    // 3D sound effects
    public void PlayMissileLaunch3D(Vector3 position)
    {
        PlaySoundEffect3D(missileLaunchSFX, position);
    }

    public void PlayMissileDestroyed3D(Vector3 position)
    {
        PlaySoundEffect3D(missileDestroyedSFX, position);
    }

    public void PlayShipExplosion3D(Vector3 position)
    {
        PlaySoundEffect3D(shipExplosionSFX, position);
    }

    // Generic methods for playing sounds
    private void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    private void PlaySoundEffect3D(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
    }

    // Utility methods
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void MuteMusic(bool mute)
    {
        musicSource.mute = mute;
    }

    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
    }
    public AudioSource StartEngineLoop3D(Transform parentTransform)
    {
        if (engineLoopSFX == null)
        {
            Debug.LogWarning("No engineLoopSFX assigned in AudioManager!");
            return null;
        }

        // Create child object to hold the AudioSource
        GameObject engineAudioObj = new GameObject("EngineLoopAudio");
        engineAudioObj.transform.SetParent(parentTransform, false); 
        engineAudioObj.transform.localPosition = Vector3.zero;

        // Use our existing 3D setup method
        AudioSource engineSource = Setup3DAudioSource(engineAudioObj);
        engineSource.clip = engineLoopSFX;
        engineSource.loop = true;
        engineSource.pitch = engineMinPitch;    // start at min
        engineSource.volume = engineMinVolume;  // optional
        engineSource.Play();

        return engineSource;
    }

    // If you want to update pitch/volume from the AudioManager side:
    // (You can also do this logic in PlayerShip directly.)
    public void UpdateEngineLoop(AudioSource engineSource, float velocityPercent)
    {
        if (engineSource == null) return;

        // velocityPercent is 0..1
        float newPitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, velocityPercent);
        float newVolume = Mathf.Lerp(engineMinVolume, engineMaxVolume, velocityPercent);

        engineSource.pitch = newPitch;
        engineSource.volume = newVolume;
    }

    // Stop and destroy the AudioSource
    public void StopEngineLoop3D(AudioSource engineSource)
    {
        if (engineSource == null) return;

        engineSource.Stop();
        Destroy(engineSource.gameObject); // removes the child obj
    }
    public void PlaySlingshotSound()
    {
        PlaySoundEffect(slingshotSFX);
    }

}