using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RockQTE : MonoBehaviour
{
    public enum QTEKey
    {
        Left,
        Right
    }

    [Header("Inputs")]
    public InputActionReference leftInput;
    public InputActionReference rightInput;
    public InputActionReference startInput;

    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction startAction;

    [Header("Tuning")]
    public int sequenceLength = 4;
    public float resetTime = .5f;

    [Header("UI")]
    public Transform lettersContainer;
    public GameObject letterPrefab;

    [Header("Colors")]
    public Color pendingColor = Color.gray;
    public Color activeColor = Color.yellow;
    public Color successColor = Color.green;
    public Color failColor = Color.black;

    private List<GameObject> letterPool = new();
    private List<QTEKey> sequence = new();
    private List<Image> letterImages = new();
    private List<TextMeshProUGUI> letterTexts = new();

    private int currentIndex = 0;
    private bool isQTEActive = false;

    void Awake()
    {
        leftAction = leftInput.action;
        rightAction = rightInput.action;
        startAction = startInput.action;

        CreatePool();
    }

    void OnEnable()
    {
        leftAction.Enable();
        rightAction.Enable();
        startAction.Enable();

        startAction.performed += OnStartPressed;
        leftAction.performed += _ => OnInput(QTEKey.Left);
        rightAction.performed += _ => OnInput(QTEKey.Right);
    }

    void OnDisable()
    {
        leftAction.performed -= _ => OnInput(QTEKey.Left);
        rightAction.performed -= _ => OnInput(QTEKey.Right);
        startAction.performed -= OnStartPressed;

        leftAction.Disable();
        rightAction.Disable();
        startAction.Disable();
    }

    void OnStartPressed(InputAction.CallbackContext context)
    {
        if (isQTEActive) return;
        StartQTE();
    }

    public void StartQTE()
    {
        
        currentIndex = 0;
        isQTEActive = true;

        ClearLetters();
        sequence = GenerateSequence();

        for (int i = 0; i < sequence.Count; i++)
        {
            var qTELetter = letterPool[i];
            qTELetter.SetActive(true);

            var letterImage = qTELetter.GetComponent<Image>();
            var letter = qTELetter.GetComponentInChildren<TextMeshProUGUI>();

            letterImage.color = pendingColor;
            letter.color = Color.black;

            if (sequence[i] == QTEKey.Left) letter.text = "L";
            else
            {
                letter.text = "R";
            }

            letterImages.Add(letterImage);
            letterTexts.Add(letter);
        }

        SetActive(0);
    }

    List<QTEKey> GenerateSequence()
    {
        var list = new List<QTEKey>();
        for (int i = 0; i < sequenceLength; i++)
        {
            list.Add(Random.value > 0.5f ? QTEKey.Left : QTEKey.Right);
        }
        return list;
    }

    void OnInput(QTEKey input)
    {
        if (!isQTEActive) return;

        if (input == sequence[currentIndex])
        {
            letterImages[currentIndex].color = successColor;
            currentIndex++;

            if (currentIndex >= sequence.Count)
            {
                EndQTE();
            }
            else
            {
                SetActive(currentIndex);
            }
        }
        else
        {
            letterImages[currentIndex].color = failColor;
            EndQTE();
        }
    }

    void SetActive(int index)
    {
        for (int i = 0; i < letterImages.Count; i++)
        {
            letterImages[i].color =
                i < index ? successColor :
                i == index ? activeColor :
                pendingColor;
        }
    }

    void EndQTE()
    {
        isQTEActive = false;
        StartCoroutine(ResetAfterDelay());
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetTime);
        ClearLetters();
    }

    void ClearLetters()
    {
        for (int i = 0; i < letterPool.Count; i++)
        {
            letterPool[i].SetActive(false);
        }

        letterImages.Clear();
        letterTexts.Clear();
    }

    void CreatePool()
    {
        for (int i = 0; i < sequenceLength; i++)
        {
            var qTELetter = Instantiate(letterPrefab, lettersContainer);
            qTELetter.SetActive(false);
            letterPool.Add(qTELetter);
        }
    }
}