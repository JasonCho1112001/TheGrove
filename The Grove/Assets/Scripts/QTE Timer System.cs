using UnityEngine;

public class QTETimerSystem : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timerDuration = 5.0f;

    [Header("UI Manager Reference")]
    public uiManager uI;

    private bool isTimerRunning = false;
    private float remainingTime;

    void Start()
    {
        ResetTimer();
        UpdateUI();
        StartTimer();
    }

    void Update()
    {
        if (!isTimerRunning) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f) StopTimer();

        UpdateUI();
    }

    public void StartTimer()
    {
        ResetTimer();
        isTimerRunning = true;
    }

    public void StopTimer()
    {
        remainingTime = 0f;
        isTimerRunning = false;
    }

    public void ResetTimer() 
    {
        remainingTime = timerDuration;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uI == null) return;

        float sliderValue = remainingTime / timerDuration;
        uI.SetSlider(uI.timerSlider, sliderValue);

        uI.SetText(uI.timerText, Mathf.CeilToInt(remainingTime).ToString());
    }
}
