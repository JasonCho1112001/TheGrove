using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;

// This prototype QTE script is for dragging. User must drag a UI element to a target area within a time limit.
// Built off from the QTEButton script and the UIDragHandle script.
public class QTEDrag : MonoBehaviour 
{
    [Header("Temporary QTE UI Settings")]
    [SerializeField] private TextMeshProUGUI tempQTEInfoText;
    [SerializeField] private TextMeshProUGUI tempQTETimerText;

    [Header("QTE (Draggable) Settings")]
    [SerializeField] private float startDuration = 10f;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private RectTransform draggableRect;
    [SerializeField] private RectTransform destinationRect;

    // Target distance to consider the QTE complete
    [Header("Completion")]
    [SerializeField] private float successDistance = 50f;

    private float timeLeft;
    private Vector2 pointerOffset;
    private bool running;

    // Starting positions 
    private float draggableCircleX = 200;
    private float draggableCircleY = -200;
    private float destinationCircleX = 1000;
    private float destinationCircleY = -400;

    public event Action OnQteComplete;
    public event Action OnQteFailed;

    private void OnEnable()
    {
        // Reset state on each start
        timeLeft = startDuration;
        running = true;

        SetRectanglePosition();

        tempQTEInfoText.enabled = true;
        tempQTETimerText.enabled = true;
        draggableRect.gameObject.SetActive(true);
        destinationRect.gameObject.SetActive(true);

        tempQTEInfoText.text = "Drag Yellow Circle to Green Circle";
        UpdateTimerText();
    }

    private void OnDisable()
    {
        if (tempQTEInfoText) tempQTEInfoText.enabled = false;
        if (tempQTETimerText) tempQTETimerText.enabled = false;
        if (draggableRect) draggableRect.gameObject.SetActive(false);
        if (destinationRect) destinationRect.gameObject.SetActive(false);
        running = false;
    }

    private void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        UpdateTimerText();

        if (timeLeft <= 0f)
        {
            Fail();
            return;
        }

        float dist = Vector2.Distance(draggableRect.anchoredPosition, destinationRect.anchoredPosition);
        if (dist <= successDistance)
        {
            Complete();
        }
    }

    private void UpdateTimerText()
    {
        tempQTETimerText.text = $"Time Left: {Mathf.CeilToInt(timeLeft)}s";
    }

    // Sets Random Position for the draggable and destination rectangles
    public void SetRectanglePosition()
    {
        draggableRect.anchoredPosition = new Vector2(
            draggableCircleX + UnityEngine.Random.Range(-100, 150),
            draggableCircleY
        );

        destinationRect.anchoredPosition = new Vector2(
            destinationCircleX + UnityEngine.Random.Range(-500, 500),
            destinationCircleY
        );
    }

    private void Complete()
    {
        running = false;
        OnQteComplete?.Invoke();
        enabled = false;
    }

    private void Fail()
    {
        running = false;
        OnQteFailed?.Invoke();
        enabled = false;
    }
}

