using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class staminaSystem : MonoBehaviour
{
    //Stamina Stats
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    public float staminaDepletionRate = 10f;
    public float staminaRegenDelay = 2f;
    public float restingStaminaRegenRate = 20f;
    private bool isBreathing = false;
    private bool isHeartBeating = false;    

    // NEW: exhaustion controls
    public float exhaustionDuration = 5f;
    public float fadeDuration = 1f;
    private bool isExhausted = false;

    public enum StaminaState { Stage1, Stage2, Stage3, Exhausted}
    public StaminaState currentStaminaState = StaminaState.Stage1;
    //References
    private rowBoatInput input;
    private uiManager ui;

    //Temp: Manually Assigned
    public RockQTE rockQTE;

    void Awake()
    {
        ui = FindFirstObjectByType<uiManager>();
        input = GetComponent<rowBoatInput>();
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        PassiveStaminaRegen();
        RestingRegen();
        HandleStaminaStages();
        ManageAudio();
    }

    void HandleStaminaStages()
    {
        if (currentStamina > maxStamina * 0.66f)
        {
            //Stage 1 - Full Stamina
            currentStaminaState = StaminaState.Stage1;
        }
        else if (currentStamina > maxStamina * 0.33f)
        {
            //Stage 2 - Moderate Stamina
            currentStaminaState = StaminaState.Stage2;
        }
        else if (currentStamina > 1f)
        {
            //Stage 3 - Low Stamina
            currentStaminaState = StaminaState.Stage3;
        }
        else 
        {
            //Exhausted - Very Low Stamina
            currentStaminaState = StaminaState.Exhausted;
            InitiateExhaustion();
        }

        // Play breathing if in Stage 2
        if (currentStaminaState == StaminaState.Stage2 || currentStaminaState == StaminaState.Stage3 || currentStaminaState == StaminaState.Exhausted)
        {
            // Only call Play if we aren't already breathing
            if (!isBreathing)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Play("HeavyBreathing", gameObject);
                    isBreathing = true;
                }
            }
        }
        // Stop breathing if we recovered to Stage 1
        else
        {
            if (isBreathing)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Stop("HeavyBreathing");
                    isBreathing = false;
                }
            }
        }   
        //UI
        ui.SetText(ui.staminaText, currentStaminaState.ToString());
    }

    void InitiateExhaustion()
    {
        // Prevent multiple concurrent exhaustion routines
        if (isExhausted) return;
        StartCoroutine(ExhaustionCoroutine());
    }

    private IEnumerator ExhaustionCoroutine()
    {
        isExhausted = true;
        Debug.Log("Player is exhausted!");

        // Disable player input/component to prevent movement
        if (input != null)
        {
            input.enabled = false;
        }

        // Attempt to fade screen to black if a UI Image named "FadePanel" exists
        Image fadeImage = null;
        GameObject fadeObj = GameObject.Find("FadePanel");
        if (fadeObj != null) fadeImage = fadeObj.GetComponent<Image>();

        // Fade to black
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            for (float t = 0f; t <= 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, fadeDuration))
            {
                c.a = Mathf.Lerp(0f, 1f, t);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeImage.color = c;
        }

        // Wait out exhaustion
        float elapsed = 0f;
        while (elapsed < exhaustionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Restore stamina and UI
        currentStamina = maxStamina * 0.5f; // Restore to 50% stamina after exhaustion
        float targetValue = currentStamina / maxStamina;
        if (ui != null)
        {
            ui.SetSlider(ui.staminaSlider, targetValue);
        }

        // Fade back in
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            for (float t = 0f; t <= 1f; t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, fadeDuration))
            {
                c.a = Mathf.Lerp(1f, 0f, t);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 0f;
            fadeImage.color = c;
        }

        // Re-enable input/component
        if (input != null)
        {
            input.enabled = true;
            input.ResetMovement();
        }

        // Ensure stamina state updates next frame
        isExhausted = false;
    }

    void ManageAudio()
    {   
        // Breathing: Starts at Stamina Stage 2 continues to Stage 3
        bool shouldBreathe = currentStaminaState == StaminaState.Stage2 || currentStaminaState == StaminaState.Stage3 || currentStaminaState == StaminaState.Exhausted;

        // Heartbeat: Starts at Stamina Stage 3
        bool shouldHeartbeat = currentStaminaState == StaminaState.Stage3 || currentStaminaState == StaminaState.Exhausted;

        // Breathing Execution
        if (shouldBreathe)
        {
            if (!isBreathing)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Play("HeavyBreathing", gameObject);
                    isBreathing = true;
                }
            }
        }
        else
        {
            if (isBreathing)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Stop("HeavyBreathing");
                    isBreathing = false;
                }
            }
        }

        // Heartbeat Execution
        if (shouldHeartbeat)
        {
            if (!isHeartBeating)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Play("Heartbeat", gameObject);
                    isHeartBeating = true;
                }
            }
        }
        else
        {
            if (isHeartBeating)
            {
                if (audioManager.instance != null)
                {
                    audioManager.instance.Stop("Heartbeat");
                    isHeartBeating = false;
                }
            }
        }
    }

    void RestingRegen()
    {
        //When to not do resting regen
        if (currentStaminaState == StaminaState.Exhausted || rockQTE.isRockQTEActive)
        {
            return;
        }

        if (input.currentState == rowBoatInput.MovementState.Idle)
        {
            currentStamina += restingStaminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }

            //UI
            float targetValue = currentStamina / maxStamina;
            float currentValue = ui.staminaSlider.value;
            ui.SetSlider(ui.staminaSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
        }
    }

    public void PassiveStaminaRegen()
    {
        if (input.currentState == rowBoatInput.MovementState.Idle || StaminaState.Exhausted == currentStaminaState || rockQTE.isRockQTEActive)
        {
            return; // No passive regen if idle, handled by resting regen
        }
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }

            //UI
            float targetValue = currentStamina / maxStamina;
            float currentValue = ui.staminaSlider.value;
            ui.SetSlider(ui.staminaSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
        }
    }

    public void DepleteStamina(float multiplier)
    {
        currentStamina -= multiplier * staminaDepletionRate * Time.deltaTime;
        if (currentStamina < 0f)
        {
            currentStamina = 0f;
        }

        //UI
        float targetValue = currentStamina / maxStamina;
        float currentValue = ui.staminaSlider.value;
        ui.SetSlider(ui.staminaSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
        
    }

    public StaminaState GetCurrentStaminaState()
    {
        return currentStaminaState;
    }

}
