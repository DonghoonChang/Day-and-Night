using UnityEngine;


[System.Serializable]
public class Sound{

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;

    [Range(0f, 1f)]
    public float spatialBlend;

    [Range(-3f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public void Play()
    {
        source.Play();
    }

    public void Pause()
    {
        source.Play();
    }

    public void Stop()
    {
        source.Play();
    }

    public static void SoundtoSource(AudioSource source, Sound sound)
    {
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.spatialBlend = sound.spatialBlend;
        source.loop = sound.loop;
        sound.source = source;
        source.playOnAwake = false;
    }
}
