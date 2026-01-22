using UnityEngine;
using TMPro;
using System;

public class QuickTimeEvent : MonoBehaviour
{
    [Header("Temporary QTE UI Settings")]
    [SerializeField] private TextMeshProUGUI tempQTEInfoText;
    [SerializeField] private TextMeshProUGUI tempQTETimerText;

    [Header("QTE Settings")]
    [SerializeField] private int minPresses = 3;
    [SerializeField] private int maxPresses = 7;
    [SerializeField] private float timerDuration = 10f;

    public int requiredKeyPresses { get; private set; }
    public int currentKeyPresses { get; private set; }

    public event Action OnQteComplete;

    private void OnEnable()
    {
        requiredKeyPresses = UnityEngine.Random.Range(minPresses, maxPresses + 1);
        currentKeyPresses = 0;
        tempQTEInfoText.enabled = true;
        tempQTETimerText.enabled = true;
        UpdateText();
    }

    private void OnDisable()
    {
        if (tempQTEInfoText != null) tempQTEInfoText.enabled = false;
        if (tempQTETimerText != null) tempQTETimerText.enabled = false;
    }

    private void Update()
    {
        // Starts the Countdown Timer
        TimerText();

        // QTE Prototype Interaction: Press the Spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentKeyPresses++;
            UpdateText();
            if (currentKeyPresses >= requiredKeyPresses) Complete();
        }
    }

    private void UpdateText()
    {
        tempQTEInfoText.text = $"Press SPACE {currentKeyPresses} / {requiredKeyPresses}";
    }

    private void TimerText()
    {
        timerDuration -= Time.deltaTime; 
        tempQTETimerText.enabled = true;
        tempQTETimerText.text = $"Time Left: {Mathf.CeilToInt(timerDuration)}s";
        if (timerDuration <= 0)
        {
            Debug.Log("QTE Failed");
            Complete();                                 //Placeholder for failure handling
        }
    }

    private void Complete()
    {
        timerDuration = UnityEngine.Random.Range(3f, 5f);
        tempQTEInfoText.enabled = false;
        tempQTETimerText.enabled = false;
        OnQteComplete?.Invoke();
        enabled = false;
    }
}
