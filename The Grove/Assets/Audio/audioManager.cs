using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundEvent
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float maxDistance = 50f;
    public SoundCategory category;
    [HideInInspector] public AudioSource source;
}

public enum SoundCategory { Player, Monster, Friend, Ambience, UI }

public class audioManager : MonoBehaviour
{
    public static audioManager instance; 

    [Header("Ducking Settings")]
    [Range(0f, 1f)] public float duckingVolume = 0.3f;
    public float duckingDuration = 3.0f;

    [Header("Audio Library")]
    public List<SoundEvent> sounds;
    
    private Coroutine duckingCoroutine;

    void Awake()
    {
        // Singleton Pattern setup
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
        
        DontDestroyOnLoad(gameObject);
    }

    public void Play(string name, GameObject emitter = null)
    {
        SoundEvent s = sounds.Find(sound => sound.soundName == name);
        if (s == null) return;
        if (s.clip == null) return;

        GameObject soundObj = new GameObject("Sound_" + name);
        
        if (emitter != null)
        {
            if (s.loop) 
            {
                soundObj.transform.parent = emitter.transform;
                soundObj.transform.localPosition = Vector3.zero;
            }
            else 
            {
                soundObj.transform.position = emitter.transform.position;
            }
        }
        else
        {
            if (Camera.main != null) soundObj.transform.position = Camera.main.transform.position;
        }

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.loop = s.loop;
        source.spatialBlend = s.spatialBlend;
        source.maxDistance = s.maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;

        s.source = source; 
        source.Play();

        if (s.category == SoundCategory.Monster)
        {
            if (duckingCoroutine != null) StopCoroutine(duckingCoroutine);
            duckingCoroutine = StartCoroutine(DuckAudio());
        }

        if (!s.loop) Destroy(soundObj, s.clip.length + 0.1f);
    }

    public void Stop(string name)
    {
        SoundEvent s = sounds.Find(sound => sound.soundName == name);
        if (s == null || s.source == null) return;

        s.source.Stop();
        if(s.source.gameObject != null) Destroy(s.source.gameObject);
    }

    IEnumerator DuckAudio()
    {
        foreach (SoundEvent s in sounds)
        {
            if (s.category == SoundCategory.Friend || s.category == SoundCategory.Ambience)
            {
                if(s.source != null) s.source.volume = s.volume * duckingVolume; 
            }
        }

        yield return new WaitForSeconds(duckingDuration);

        foreach (SoundEvent s in sounds)
        {
            if (s.category == SoundCategory.Friend || s.category == SoundCategory.Ambience)
            {
                if(s.source != null) s.source.volume = s.volume;
            }
        }
    }
}