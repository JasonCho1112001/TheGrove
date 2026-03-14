using UnityEngine;

public class AmbientDirector : MonoBehaviour
{
    [Header("Ambient Track")]
    public string ambientTrack = "forestWind";

    void Start()
    {
        Invoke(nameof(StartAmbience), 0.1f);
    }

    void StartAmbience()
    {
        if (audioManager.instance != null && !string.IsNullOrEmpty(ambientTrack))
        {
            audioManager.instance.Play(ambientTrack, null);
        }
    }
}