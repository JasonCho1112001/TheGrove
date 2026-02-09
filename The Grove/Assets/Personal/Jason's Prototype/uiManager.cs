using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class uiManager : MonoBehaviour
{
    //Agitation Helpers
    public Slider agitationSlider;
    public TextMeshProUGUI agitationText;

    public Slider movementSlider;
    public TextMeshProUGUI movementText;

    public Slider staminaSlider;
    public TextMeshProUGUI staminaText;

    public GameObject aDisabledText;
    public GameObject aEnabledText;
    public GameObject dEnabledText;
    public GameObject dDisabledText;

    public Image DDR1;
    public Image DDR2;

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
    }

    
    void Update()
    {
        
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

    
}




