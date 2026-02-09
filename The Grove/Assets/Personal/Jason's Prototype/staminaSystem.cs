using System;
using UnityEngine;

public class staminaSystem : MonoBehaviour
{
    //Stamina Stats
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    public float staminaDepletionRate = 10f;
    public float staminaRegenDelay = 2f;
    public float restingStaminaRegenRate = 20f;

    public enum StaminaState { Stage1, Stage2, Stage3, Exhausted}
    public StaminaState currentStaminaState = StaminaState.Stage1;
    //References
    private rowBoatInput input;
    private uiManager ui;

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
        }

        //UI
        ui.SetText(ui.staminaText, currentStaminaState.ToString());
    }

    void RestingRegen()
    {
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
        if (input.currentState == rowBoatInput.MovementState.Idle)
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
