using UnityEngine;

public class staminaSystem : MonoBehaviour
{
    //Stamina Stats
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 5f;
    public float staminaDepletionRate = 10f;
    public float staminaRegenDelay = 2f;

    //References
    private rowBoatInput input;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DepleteStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0f)
        {
            currentStamina = 0f;
        }
    }
}
