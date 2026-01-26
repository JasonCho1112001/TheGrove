using UnityEngine;
using UnityEngine.UI;

public class agitationMeterUI : MonoBehaviour
{
    public Slider slider;
    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

    }

    public void SetSlider(float value)
    {
        slider.value = value;
    }
}
