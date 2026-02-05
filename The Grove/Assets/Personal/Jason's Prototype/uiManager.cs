using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class uiManager : MonoBehaviour
{
    //Agitation Helpers
    public Slider agitationSlider;
    public TextMeshProUGUI agitationText;

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
}
