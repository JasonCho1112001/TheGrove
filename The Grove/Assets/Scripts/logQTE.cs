using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 

public class LogQTE : MonoBehaviour
{
    [Header("QTE Settings")]
    public float staminaDamage = 30f; 
    public Key duckKey = Key.Space;
    
    [Header("Movement Penalty")]
    public float crouchingSpeed = 0.6f; 

    [Header("Camera Pan & Warning Settings")]
    public float warningDistance = 5f; 
    public float panAngle = -15f; 
    public float panDuration = 0.5f;

    [Header("Impact Settings (Fail)")]
    public float stunDuration = 1f;
    public float shakeMagnitude = 0.3f;

    [Header("Timing")]
    public float reactionWindow = 0.5f;


    // State
    private bool playerInZone = false;
    private bool hasHitLog = false;
    private bool isWarningActive = false; 
    private float entryTime = 0f;
    
    // References
    private staminaSystem playerStamina;
    private rowBoatInput playerMovement; 
    private Transform playerTransform;

    // Camera Tracking
    private Transform mainCamTransform;
    private Quaternion originalCamRotation;
    private Coroutine panCoroutine;

    void Start()
    {
        if(GetComponent<MeshRenderer>() != null) 
            GetComponent<MeshRenderer>().enabled = false;
            
        // Cache the camera and its starting rotation
        if (Camera.main != null) 
        {
            mainCamTransform = Camera.main.transform;
            originalCamRotation = mainCamTransform.localRotation;
        }

        // Find the player at the start so we can measure distance to them
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // Only check if the player hasn't actually entered the physical log zone yet
        if (playerTransform != null && !playerInZone && !hasHitLog)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // If player gets close, trigger the warning pan
            if (distanceToPlayer <= warningDistance && !isWarningActive)
            {
                isWarningActive = true;
                TriggerPanUp();
            }
            // If they move away from it, reset the pan
            else if (distanceToPlayer > warningDistance && isWarningActive)
            {
                isWarningActive = false;
                TriggerPanDown();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            hasHitLog = false;
            playerStamina = other.GetComponent<staminaSystem>();
            playerMovement = other.GetComponent<rowBoatInput>(); 
            entryTime = Time.time;

            if (audioManager.instance != null) 
            {
                audioManager.instance.Play("duckSwoosh", gameObject);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (playerInZone && !hasHitLog && other.CompareTag("Player"))
        {
            if (Keyboard.current != null)
            {
                // Manage crouching Speed
                if (Keyboard.current[duckKey].isPressed)
                {
                    if(playerMovement != null) playerMovement.currentSpeedModifier = crouchingSpeed;
                }
                else
                {
                    if(playerMovement != null) playerMovement.currentSpeedModifier = 1f;
                }

                // Fail Check
                float timeInZone = Time.time - entryTime;
                if (timeInZone > reactionWindow)
                {
                    if (!Keyboard.current[duckKey].isPressed)
                    {
                         FailEvent();
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasHitLog) SuccessEvent();
            ResetObstacle();
        }
    }

    void ResetObstacle()
    {
        if (playerMovement != null) playerMovement.currentSpeedModifier = 1f;

        playerInZone = false;
        isWarningActive = false; 
        playerStamina = null;
        playerMovement = null;

        if (audioManager.instance != null) audioManager.instance.Stop("duckSwoosh");

        TriggerPanDown();
    }

    void FailEvent()
    {
        hasHitLog = true;
        Debug.Log("Hit the log!");

        if (playerStamina != null)
        {
            playerStamina.currentStamina -= staminaDamage;
            if(playerStamina.currentStamina < 0) playerStamina.currentStamina = 0;
        }

        if (audioManager.instance != null) 
        {
            audioManager.instance.Play("bonk", gameObject);
            audioManager.instance.Stop("duckSwoosh");
        }

        // Return camera to normal instantly if we fail, so the shake looks right
        if (mainCamTransform != null)
        {
            if (panCoroutine != null) StopCoroutine(panCoroutine);
            mainCamTransform.localRotation = originalCamRotation;
        }
        
        StartCoroutine(HandleFailImpact());
    }

    // Camera helper functions 

    void TriggerPanUp()
    {
        if (mainCamTransform != null)
        {
            if (panCoroutine != null) StopCoroutine(panCoroutine);
            Quaternion targetRot = originalCamRotation * Quaternion.Euler(panAngle, 0f, 0f);
            panCoroutine = StartCoroutine(SmoothPanCamera(targetRot));
        }
    }

    void TriggerPanDown()
    {
        if (mainCamTransform != null)
        {
            if (panCoroutine != null) StopCoroutine(panCoroutine);
            panCoroutine = StartCoroutine(SmoothPanCamera(originalCamRotation));
        }
    }

    IEnumerator SmoothPanCamera(Quaternion targetRotation)
    {
        float elapsed = 0f;
        Quaternion startRotation = mainCamTransform.localRotation;

        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            mainCamTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / panDuration);
            yield return null;
        }

        mainCamTransform.localRotation = targetRotation;
    }

    IEnumerator HandleFailImpact()
    {
        if (playerMovement != null) playerMovement.currentSpeedModifier = 0f;

        Vector3 originalCamPos = mainCamTransform != null ? mainCamTransform.localPosition : Vector3.zero;
        float elapsed = 0f;

        while (elapsed < stunDuration)
        {
            elapsed += Time.deltaTime;

            if (mainCamTransform != null)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                mainCamTransform.localPosition = new Vector3(originalCamPos.x + x, originalCamPos.y + y, originalCamPos.z);
            }

            yield return null; 
        }

        if (mainCamTransform != null)
        {
            mainCamTransform.localPosition = originalCamPos;
        }

        if (playerMovement != null)
        {
            if (Keyboard.current != null && Keyboard.current[duckKey].isPressed)
                playerMovement.currentSpeedModifier = crouchingSpeed;
            else
                playerMovement.currentSpeedModifier = 1f;
        }
    }

    void SuccessEvent()
    {
        Debug.Log("Nice dodge!");
    }
}