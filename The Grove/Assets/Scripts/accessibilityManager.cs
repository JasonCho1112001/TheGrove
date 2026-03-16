using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class accessibilityManager : MonoBehaviour
{
    public bool limitedVisionMode = false;
    public Volume grayscaleVolume;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // keep existing Awake logic minimal; actual volume lookup will happen in Start/scene load
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        UpdateMainCameraVolume();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMainCameraVolume();
    }

    private void UpdateMainCameraVolume()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found after scene load.");
            grayscaleVolume = null;
            return;
        }

        Volume volume = mainCamera.GetComponent<Volume>();
        if (volume != null)
        {
            grayscaleVolume = volume;
            grayscaleVolume.enabled = limitedVisionMode;
        }
        else
        {
            Debug.LogWarning("Main Camera does not have a Volume component.");
            grayscaleVolume = null;
        }
    }

    void Update()
    {
        if (grayscaleVolume != null)
        {
            grayscaleVolume.enabled = limitedVisionMode;
        }
    }

    public void SetLimitedVisionMode(bool enabled)
    {
        limitedVisionMode = enabled;
    }
}
