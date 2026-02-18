using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

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

    public Slider distanceSlider;
    public TextMeshProUGUI distanceText;

    public Slider proximitySlider;
    public TextMeshProUGUI proximityText;

    public GameObject aDisabledText;
    public GameObject aEnabledText;
    public GameObject dEnabledText;
    public GameObject dDisabledText;

    public TextMeshProUGUI monsterSideValue;
    public TextMeshProUGUI monsterTimerValue;

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

        //Disable defaultBackground
        defaultBackground.SetActive(false);
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

    
}




