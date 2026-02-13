using UnityEngine;
using UnityEngine.InputSystem;

public class rowBoatInput : MonoBehaviour
{
    //Inputs
    [Header("--Inputs--")]
    public InputActionReference leftInput;
    public InputActionReference rightInput;

    private InputAction leftAction;
    private InputAction rightAction;

    //Stats
    [Header("--Stats--")]
    public float forwardForce = 2.5f;
    public float maxSpeed = 5f;

    [Header("--Audio Settings--")]
    public float stepAudioCooldown = 0.4f; 
    private float lastStepTime = 0f;    
    public float cadenceDelayTime = 0f;

    //Movement State
    
    public enum MovementState { Idle, Walking, Jogging, Sprinting}

    [Header("--Movement State--")]
    [SerializeField]
    public MovementState currentState = MovementState.Idle;
    public float[] movementMultipliers = new float[] {0f, 1f, 2f, 4f};
    public float movementMultiplier = 0f;
    public float minimumForce = 5f;

    //Cadence Tracking
    [Header("--Cadence Tracking--")]
    public float[] inputTimestamps = new float[5];
    public float cadence = 0f;
    
    public float cadenceTimerMax = 2f;
    public float cadenceTimer = 2f;



    //References
    private Rigidbody rb;
    
    staminaSystem stamina;
    uiManager ui;

    void Awake()
    {   //Assign input actions
        if(leftInput != null) { leftAction = leftInput.action; 
            } else { Debug.Log("No left input assigned");}
        if(rightInput != null) { rightAction = rightInput.action; 
            } else { Debug.Log("No right input assigned");} 

        //Get references
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<staminaSystem>();
        ui = FindFirstObjectByType<uiManager>();
    }

    void Update()
    {
        //Currently polling input in Update for movement state determination
        ManageMovement();
        ManageCadenceTimer();
        ManageRigidBodyForce();

        //Agitation UI
        float targetValue = cadence / 5f; // Assuming 5 inputs per second is the max for full agitation meter
        float currentValue = ui.agitationSlider.value;
        ui.SetSlider(ui.agitationSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
    }

    void OnEnable()
    {
        leftAction.Enable(); 
        leftAction.performed += LeftStep;
        leftAction.canceled += ctx => ui.EnableText(ui.aEnabledText, false); // Disable A enabled text on button release

        rightAction.Enable();
        rightAction.performed += RightStep;
        rightAction.canceled += ctx => ui.EnableText(ui.dEnabledText, false); // Disable D enabled text on button release

        ClearInputTimestamps();
    }

    void OnDisable()
    {
        leftAction.Disable(); 
        leftAction.performed -= LeftStep;
        leftAction.canceled -= ctx => ui.EnableText(ui.aEnabledText, false);

        rightAction.Disable();
        rightAction.performed -= RightStep;
        rightAction.canceled -= ctx => ui.EnableText(ui.dEnabledText, false);
    }
    
    void LeftStep(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.aEnabledText, true);

        InvokeFootstepAudio();
    }

    void RightStep(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.dEnabledText, true);
        
        InvokeFootstepAudio();
    }


    void InvokeFootstepAudio()
    {
        // Audio
        if (Time.time >= lastStepTime + stepAudioCooldown)
        {
            //Vision: Play the footstep audio with a delay based on cadence 
            //Faster cadence = shorter delay , slower cadence = longer delay

            //Determine cadence delay time (e.g. 0.5 seconds at 1 input per second, 0.25 seconds at 2 inputs per second, etc.)
            cadenceDelayTime = Mathf.Clamp(1f / cadence, 0.1f, 0.5f); // Clamp delay between 0.1 and 0.5 seconds
    
            Invoke(nameof(PlayFootstepSound), cadenceDelayTime);
            lastStepTime = Time.time;
        }
    }

    void PlayFootstepSound()
    {
        if (audioManager.instance != null)
        {
            audioManager.instance.Play("PlayerFootstep", gameObject);
        }
    }

    void RecordInputTimestamps()
    {
        for (int i = 0; i < inputTimestamps.Length - 1; i++)
        {
            inputTimestamps[i] = inputTimestamps[i + 1];
        }
        inputTimestamps[inputTimestamps.Length - 1] = Time.time;

        CalculateCadence();
        cadenceTimer = cadenceTimerMax;
    }

    public void ClearInputTimestamps()
    {
        for (int i = 0; i < inputTimestamps.Length; i++)
        {
            inputTimestamps[i] = 0f;
        }
    }

    void ManageCadenceTimer()
    {
        if (cadenceTimer > 0f)
        {
            cadenceTimer -= Time.deltaTime;
        }

        if (cadenceTimer <= 0f)
        {
            cadence = 0f;
            ui.SetText(ui.agitationText, cadence.ToString("F2"));
            ui.SetSlider(ui.agitationSlider, 0f);
        }
    }

    void CalculateCadence()
    {
        //Calculate time difference between first and last input in the array but only do so for non-zero timestamps to avoid skewing cadence when there are few inputs
        int validTimestamps = 0;
        for (int i = 0; i < inputTimestamps.Length; i++)
        {
            if (inputTimestamps[i] > 0f)
            {
                validTimestamps++;
            }
        }
        if (validTimestamps < 2)
        {
            cadence = 0f;
            return;
        }
        float timeDifference = inputTimestamps[inputTimestamps.Length - 1] - inputTimestamps[0];

        //Calculate cadence (inputs per second)
        cadence = (inputTimestamps.Length - 1) / timeDifference;

        //UI 
        ui.SetText(ui.agitationText, cadence.ToString("F2"));
    }

    void ManageMovement()
    {
        //Determine movement state based off of cadence
        if (cadence < 0.25f)
        {
            currentState = MovementState.Idle;
            movementMultiplier = movementMultipliers[0]; // 0
        }
        else if (cadence < 2f)
        {
            currentState = MovementState.Walking;
            movementMultiplier = movementMultipliers[1]; // 1
        }
        else if (cadence < 3.5f)
        {
            currentState = MovementState.Jogging;
            movementMultiplier = movementMultipliers[2]; // 2
        }
        else
        {
            currentState = MovementState.Sprinting;
            movementMultiplier = movementMultipliers[3]; // 4
        }

        //Deplete stamina based on movement state
        stamina.DepleteStamina(movementMultiplier); // Deplete more stamina at higher movement states

        //UI
        ui.SetText(ui.movementText, currentState.ToString());

    }

    void ManageRigidBodyForce()
    {
        //Apply forward force based on movement state
        Vector3 forwardForceVector = transform.forward * minimumForce + transform.forward * forwardForce * movementMultiplier;
        rb.AddForce(forwardForceVector, ForceMode.Acceleration);

        //Cap speed
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 cappedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
        }

        //UI
        float targetValue = horizontalVelocity.magnitude / maxSpeed;
        float currentValue = ui.movementSlider.value;
        ui.SetSlider(ui.movementSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
    }
}

    
