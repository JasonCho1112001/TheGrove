using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEditor;
using JetBrains.Annotations;

public class rowBoatInput : MonoBehaviour
{
    //Inputs
    [Header("--Inputs--")]
    public InputActionReference leftInput;
    public InputActionReference rightInput;

    public InputActionReference lookInput;

    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction lookAction;

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

    //Look Input
    [Header("--Look Input--")]
    public bool isLooking = false;
    public float lookSpeed = 5f;

    //Moving Left or Right
    [Header("--Moving Left or Right--")]
    public int leftRightBias = 0; // -1 for left, 1 for right, 0 for neutral
    public int lastDirectionInput = 0; // -1 for left, 1 for right, 0 for neutral
    public float horizontalSpeed = 2f; // How much the left/right bias influences horizontal movement
    public float maxHorizontalRange = 5f; // Maximum horizontal distance from the center line
    public Vector3 centerLine;
    public PlayerSide playerSide = PlayerSide.Left;
    public enum PlayerSide { Left, Right }

    //Monster Manager List
    //Tyvin: There is a bug if the player turns left or right, the monster manager won't follow the player properly with its rotation
    //For a temporary fix, I just have it where we have multiple monster managers. 
    //When the player turns left or right, it disables the current monster manager and enables the next one.
    //Like a linked list.
    [Header("--Monster Manager List--")]
    public List<GameObject> monsterManagers; 
    public int currentIndex = 0;
    public GameObject currentMonsterManager;
    public bool isTurned = false;

    //References
    private Rigidbody rb;
    private Camera cam;
    
    staminaSystem stamina;
    uiManager ui;

    void Awake()
    {   //Assign input actions
        if(leftInput != null) { leftAction = leftInput.action; 
            } else { Debug.Log("No left input assigned");}
        if(rightInput != null) { rightAction = rightInput.action; 
            } else { Debug.Log("No right input assigned");} 
        if(lookInput != null) { lookAction = lookInput.action; 
            } else { Debug.Log("No look input assigned");} 

        //Get references
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<staminaSystem>();
        ui = FindFirstObjectByType<uiManager>();
        cam = FindFirstObjectByType<Camera>();
    }

    void Start()
    {
        if (monsterManagers == null || monsterManagers.Count == 0)
        {Debug.LogError("monsterManagers list is empty in player cube");}
        for (int i = 0; i < monsterManagers.Count; i++)
        { monsterManagers[i].SetActive(false); }

        currentIndex = 0;
        currentMonsterManager = monsterManagers[currentIndex];
        currentMonsterManager.SetActive(true);

        centerLine = transform.position;
    }

    void Update()
    {
        //Movement
        //Currently polling input in Update for movement state determination
        ManageMovement();
        ManageCadenceTimer();
        ManageRigidBodyForce();

        //Looking
        HandleLookingCamera();

        //Agitation UI
        float targetValue = cadence / 5f; // Assuming 5 inputs per second is the max for full agitation meter
        float currentValue = ui.agitationSlider.value;
        ui.SetSlider(ui.agitationSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));

        //Monster Manager Handling
        if (isTurned)
        {MonsterManagerHelper(); isTurned = false;}
    }

    public void MonsterManagerHelper(){
    cadence = 0f;
    if (currentMonsterManager != null){currentMonsterManager.SetActive(false);}

    currentIndex = (currentIndex + 1) % monsterManagers.Count;

    currentMonsterManager = monsterManagers[currentIndex];
    if (currentMonsterManager != null){currentMonsterManager.SetActive(true);}
    }

    void OnEnable()
    {
        leftAction.Enable(); 
        leftAction.performed += LeftStep;
        leftAction.canceled += ctx => ui.EnableText(ui.aEnabledText, false); // Disable A enabled text on button release

        rightAction.Enable();
        rightAction.performed += RightStep;
        rightAction.canceled += ctx => ui.EnableText(ui.dEnabledText, false); // Disable D enabled text on button release

        lookAction.Enable();
        lookAction.performed += LookStart;
        lookAction.canceled += ctx => LookStop();

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

        lookAction.Disable();
        lookAction.performed -= LookStart;
        lookAction.canceled -= ctx => LookStop();
    }

    // Tyvin: This rotates the player when they enter a camera rotate trigger zone.
    // TODO: Make the camera turns smoother and not instant.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Left Camera Rotate"))
        {
            Debug.Log("Player Rotate Left");
            transform.Rotate(0f, -90f, 0f);
            isTurned = true;
            
        }
        if (other.CompareTag("Right Camera Rotate"))
        {
            Debug.Log("Player Rotate Right");
            transform.Rotate(0f, 90f, 0f);
            isTurned = true;
        }
    }
    
    void LookStart(InputAction.CallbackContext ctx)
    {
        isLooking = true;
    }

    void LookStop()
    {
        isLooking = false;
    }

    void HandleLookingCamera()
    {
        if (isLooking)
        {
            //Rotate the camera toward the -forward direction of the character
            Vector3 targetDirection = -transform.forward ;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, Time.deltaTime * lookSpeed);

            //Stop movement while looking
            cadence = 0f;
        }
        else
        {
            //Rotate it back to forward
            Vector3 targetDirection = transform.forward;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
        }
    }

    void LeftStep(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.aEnabledText, true);
        InvokeFootstepAudio();

        HandleLeftRightBias(-1);
    }

    void RightStep(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.dEnabledText, true);
        InvokeFootstepAudio();

        HandleLeftRightBias(1);
    }

    void HandleLeftRightBias(int leftRight)
    {
        //Keep track of the last direction input 
        //If the second input is the same direction, begin adding bias

        if (leftRight == lastDirectionInput)
        {
            leftRightBias += leftRight * 5; //Takes two steps in the same direction to reach full bias
        }
        else
        {
            leftRightBias = 0; // Reset bias if direction changes
        }
        //Clamp bias to prevent excessive biasing
        leftRightBias = (int)Mathf.Clamp(leftRightBias, -10f, 10f);

        lastDirectionInput = leftRight;
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

        //How moving left or right works:
        //Left input adds to leftRightBias, right input adds to it. This bias then influences the direction of the forward force applied to the rigidbody in ManageRigidBodyForce, creating a curved movement path when bias is not neutral. Bias slowly returns to neutral over opposite steps.
        
    }

    void ManageRigidBodyForce()
    {
        //Apply forward force based on movement state and left/right bias
        Vector3 forwardForceVector = transform.forward * minimumForce + transform.forward * forwardForce * movementMultiplier;
        Vector3 rightForceVector = Vector3.zero;

        //Apply leftRightmovement Only if we are within horizontal range
        Vector3 horizontalOffset = transform.position - centerLine;
        if (Mathf.Abs(horizontalOffset.x) < maxHorizontalRange || Mathf.Sign(horizontalOffset.x) != Mathf.Sign(leftRightBias))
        {
            rightForceVector = transform.right * leftRightBias * horizontalSpeed;
        }

        //Set playerside
        if (horizontalOffset.x < -0.25f)
        {
            playerSide = PlayerSide.Left;
            ui.SetText(ui.playerSideValue, playerSide.ToString());
        }
        else if (horizontalOffset.x > 0.25f)
        {
            playerSide = PlayerSide.Right;
            ui.SetText(ui.playerSideValue, playerSide.ToString());
        }
        else
        {
            //Leave it as last side when at the center
        }

        rb.AddForce(forwardForceVector + rightForceVector, ForceMode.Acceleration);

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

    public int GetPlayerSide()
    {
        return (playerSide == PlayerSide.Left) ? -1 : 1;
    }
}

    
