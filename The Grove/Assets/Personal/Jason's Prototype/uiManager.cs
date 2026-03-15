using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class uiManager : MonoBehaviour
{
    //Default Location
    public GameObject defaultBackground;
    
    //Group Helpers
    public GameObject[] groups;

    //Agitation Helpers
    public Slider agitationSlider;
    public TextMeshProUGUI agitationText;

    public Slider movementSlider;
    public TextMeshProUGUI movementText;

    public Slider staminaSlider;
    public TextMeshProUGUI staminaText;

    public Slider playerStaminaSlider;

    public Slider distanceSlider;
    public TextMeshProUGUI distanceText;

    public Slider proximitySlider;
    public TextMeshProUGUI proximityText;

    public Slider timerSlider;
    public TextMeshProUGUI timerText;

    public Slider angleSlider;
    public TextMeshProUGUI angleText;

    public GameObject aDisabledText;
    public GameObject aEnabledText;
    public GameObject dEnabledText;
    public GameObject dDisabledText;

    public TextMeshProUGUI monsterSideValue;
    public TextMeshProUGUI monsterTimerValue;
    public TextMeshProUGUI playerSideValue;
    public Slider sameTrackSlider;
    public TextMeshProUGUI sameTrackText;
    public TextMeshProUGUI jumpscareText;

    public GameObject recDot;
    private float dotTimer;
    private float dotTimeDuration = 0.5f;
    private bool dotToggle = false;

    public Volume postProcessVolume;
    public DepthOfField depthOfField;
    public float myfocusDistance;
    public float myAperture;
    public float myFocalLength;
    public Vignette vignette;
    public float myVignetteIntensity;

    // Vignette bobbing settings
    public float vignetteMin = 0.6f;
    public float vignetteMax = 0.8f;
    public float vignetteBobbingSpeed = 2f;
    private Coroutine vignetteCoroutine;

    void Awake()
    {
        if (agitationSlider == null)
        {
            throw new System.Exception("Agitation Slider not assigned in uiManager");
        }
        if (agitationText == null)
        {
            throw new System.Exception("Agitation Text not assigned in uiManager");
        }

        if (staminaSlider == null)
        {
            throw new System.Exception("Stamina Slider not assigned in uiManager");
        }
        if (staminaText == null)
        {
            throw new System.Exception("Stamina Text not assigned in uiManager");
        }
        if (recDot == null)
        {
            throw new System.Exception("recDot not assigned in inspector");
        }
        if (playerStaminaSlider == null)
        {
            throw new System.Exception("Player Stamina Slider not assigned in inspector");
        }

        //Disable defaultBackground
        defaultBackground.SetActive(false);

        SaveDepthOfFieldSettings();
        SaveVignetteSettings();
    }

    
    void Update()
    {
        //Inputs to control panel visibility for testing
        if (Keyboard.current.leftShiftKey.isPressed) 
        {
            //Handle inputs for shift + 1-9
            for (int i = 1; i <= 9; i++)
            {
                Key digitKey = (Key)((int)Key.Digit1 + i - 1);
                var kc = Keyboard.current[digitKey];
                if (kc.wasPressedThisFrame)
                {
                    ToggleGroup(groups[i - 1]);
                }
            }

            if(Keyboard.current.bKey.wasPressedThisFrame)
            {
                InitiateBlur();
            }
        }

        dotTimer += Time.deltaTime;
        if (dotTimer >= dotTimeDuration)
        {
            dotTimer = 0;
            recDot.SetActive(dotToggle);
            dotToggle = !dotToggle;
        }
    }

    public void SetSlider(Slider mySlider, float value)
    {
        mySlider.value = value;
    }

    public void SetText(TextMeshProUGUI myText, string value)
    {
        myText.text = value;
    }

    public void EnableText(GameObject myText, bool enabled)
    {
        myText.SetActive(enabled);
    }

    public void EnableImage(Image myImage, bool enabled)
    {
        myImage.enabled = enabled;
    }

    public void ToggleGroup(GameObject group)
    {
        // No need to enable / disable, simply move the locations

        //Check if toggled, this is done by comparing positions
        bool toggled = group.transform.position == defaultBackground.transform.position;

        if(!toggled)
        {
            //Move to default location
            group.transform.position = defaultBackground.transform.position;
        }
        else
        {
            //Move to the right of the screen, this is arbitrary and can be changed later
            group.transform.position = new Vector3(-1000f, defaultBackground.transform.position.y, defaultBackground.transform.position.z);
        }

        Debug.Log("Toggled group: " + group.name + " to " + (enabled ? "enabled" : "disabled"));

        //UnToggle all other groups
        foreach (GameObject g in groups)
        {
            if (g != group)
            {
                g.transform.position = new Vector3(-1000f, defaultBackground.transform.position.y, defaultBackground.transform.position.z);
            }
        }
    }

    void SaveDepthOfFieldSettings()
    {
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                // Save current settings
                myfocusDistance = depthOfField.focusDistance.value;
                myAperture = depthOfField.aperture.value;
                myFocalLength = depthOfField.focalLength.value;
            }
        }
    }

    void SaveVignetteSettings()
    {
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
            {
                // Save current settings
                myVignetteIntensity = vignette.intensity.value;
            }
        }
    }

    public void InitiateBlur()
    {
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                depthOfField.focusDistance.value = myfocusDistance / 2;
                depthOfField.aperture.value = myAperture / 2;
                depthOfField.focalLength.value = myFocalLength * 2;
            }

        }
    }

    public void DisableBlur()
    {
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet<DepthOfField>(out depthOfField))
            {
                depthOfField.focusDistance.value = myfocusDistance;
                depthOfField.aperture.value = myAperture;
                depthOfField.focalLength.value = myFocalLength;
            }
        }
    }

    public void InitiateVignette(int stage)
    {
        float min = vignetteMin;
        float max = vignetteMax;
        if(stage == 2) // Stage 2 - Vignette is not as intense
        {
            min = min - 0.15f;
            max = max - 0.15f;
        }
        
        // start vignette bobbing
        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
            vignetteCoroutine = StartCoroutine(VignetteBobbing(min, max));
        }
    } 
    public void DisableVignette()
    {
        // stop vignette bobbing and restore saved intensity
        if (vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
            vignetteCoroutine = null;
        }

        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.value = myVignetteIntensity;
        }
    }

    private System.Collections.IEnumerator VignetteBobbing(float min, float max)
    {
        // Ensure vignette reference
        if (postProcessVolume == null || !postProcessVolume.profile.TryGet<Vignette>(out vignette))
            yield break;

        while (true)
        {
            Debug.Log("Bobbing vignette with settings: min=" + min + ", max=" + max + ", speed=" + vignetteBobbingSpeed);
            float s = Mathf.Sin(Time.time * vignetteBobbingSpeed); // -1..1
            float t = (s + 1f) * 0.5f; // 0..1
            vignette.intensity.value = Mathf.Lerp(min, max, t);
            yield return null;
        }
    }
}




