using UnityEngine;
using System.Collections;
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
    private bool isLeftHeld = false;
    private bool isRightHeld = false;

    private Coroutine holdCoroutine;
    public float holdThreshold = 0.5f; // Time in seconds to consider an input as a hold

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
    public float leftRightMovement = 0f;
    public int lastDirectionInput = 0; // -1 for left, 1 for right, 0 for neutral
    public float horizontalSpeed = 2f; // How much the left/right bias influences horizontal movement
    public float maxHorizontalRange = 5f; // Maximum horizontal distance from the center line
    public Vector3 centerLine;
    public PlayerSide playerSide = PlayerSide.Left;
    public enum PlayerSide { Left, Right }
    public Vector3 horizontalOffset;
    public float leftRightTimer = 0f;
    public float leftRightMultiplier = 5f;
    public float forwardDuringLeftRightMultiplier = 3f; // How much forward force is applied when moving left/right

    [Header("--Handle View Bobbing--")]
    public float viewBobbingDisplacementIntensity = 0.1f;
    public float viewBobbingDisplacementSpeed = 5f;
    public float viewBobbingRollIntensity = 1f;
    public float viewBobbingRollSpeed = 5f;
    private Vector3 originalCameraPosition;
    private float originalCameraXRotation;
    private Quaternion originalCameraRotation;
    private Vector3 targetCameraPosition;
    public float leftRightViewBobbingIntensity = 0.5f;

    //[Header("--Camera Rotation QTE--")]
    //[SerializeField] public CustomCameraRotateQTE rotateTriggerBlockScript;
    

    //References
    private Rigidbody rb;
    private Camera cam;
    
    staminaSystem stamina;
    uiManager ui;
    monsterManager monsterManager;

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
        monsterManager = FindFirstObjectByType<monsterManager>();
    }

    void Start()
    {                
        centerLine = transform.position;
        originalCameraPosition = cam.transform.localPosition;
        originalCameraRotation = cam.transform.localRotation;
        targetCameraPosition = originalCameraPosition;
        originalCameraXRotation = cam.transform.localEulerAngles.x;
    }

    void Update()
    {
        //Movement
        //Currently polling input in Update for movement state determination
        ManageMovement();
        ManageCadenceTimer();
        

        //Looking
        HandleLookingCamera();
        HandleViewBobbing();
        HandleLeftRightHold();

        //Agitation UI
        float targetValue = cadence / 5f; // Assuming 5 inputs per second is the max for full agitation meter
        float currentValue = ui.agitationSlider.value;
        ui.SetSlider(ui.agitationSlider, Mathf.Lerp(currentValue, targetValue, Time.deltaTime * 5f));
    }

    void FixedUpdate()
    {
        ManageRigidBodyForce();
    }


    void OnEnable()
    {
        leftAction.Enable(); 
        leftAction.started += OnLeftStarted;
        leftAction.canceled += OnLeftCanceled;

        rightAction.Enable();
        rightAction.started += OnRightStarted;
        rightAction.canceled += OnRightCanceled;

        lookAction.Enable();
        lookAction.performed += LookStart;
        lookAction.canceled += ctx => LookStop();

        ClearInputTimestamps();
    }

    void OnDisable()
    {
        leftAction.Disable(); 
        leftAction.started -= OnLeftStarted;
        leftAction.canceled -= OnLeftCanceled;

        rightAction.Disable();
        rightAction.started -= OnRightStarted;
        rightAction.canceled -= OnRightCanceled;

        lookAction.Disable();
        lookAction.performed -= LookStart;
        lookAction.canceled -= ctx => LookStop();
    }

    void OnLeftStarted(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.aEnabledText, true);
        HandleLeftRightMovement(-1);
        InvokeFootstepAudio();

        isLeftHeld = true;
        holdCoroutine = StartCoroutine(HoldWatcher());
    }

    void OnRightStarted(InputAction.CallbackContext ctx)
    {
        RecordInputTimestamps();
        ui.EnableText(ui.dEnabledText, true);
        HandleLeftRightMovement(1);
        InvokeFootstepAudio();

        
        isRightHeld = true;
        holdCoroutine = StartCoroutine(HoldWatcher());
    }

    void OnLeftCanceled(InputAction.CallbackContext ctx)
    {
        ui.EnableText(ui.aEnabledText, false);
        StopCoroutine(holdCoroutine);
        leftRightMovement = 0f;
        isLeftHeld = false;
        leftRightTimer = 0f;
    }

    void OnRightCanceled(InputAction.CallbackContext ctx)
    {
        ui.EnableText(ui.dEnabledText, false);
        StopCoroutine(holdCoroutine);
        leftRightMovement = 0f;
        isRightHeld = false;
        leftRightTimer = 0f;
    }

    private IEnumerator HoldWatcher()
    {
        yield return new WaitForSeconds(holdThreshold);
        if (leftAction.ReadValue<float>() > 0.5f)
        {
            OnLeftHeld();
        }
        else if (rightAction.ReadValue<float>() > 0.5f)
        {
            OnRightHeld();
        }
    }

    void OnLeftHeld()
    {
        //Debug.Log("Left Held");
    }

    void OnRightHeld()
    {
        //Debug.Log("Right Held");
    }

    void HandleLeftRightHold()
    {
        if (isLeftHeld)
        {
            HandleLeftRightMovement(-1);
        }
        else if (isRightHeld)
        {
            HandleLeftRightMovement(1);
        }
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

    void HandleLeftRightMovement(int direction)
    {
        leftRightTimer += Time.deltaTime;
        if (leftRightTimer > holdThreshold)
        {
            leftRightMovement += direction * Time.deltaTime * leftRightMultiplier;
            //Keep leftRightMovement between -10 and 10
            leftRightMovement = Mathf.Clamp(leftRightMovement, -10f, 10f);
        }
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
        //Pause the timer if player is holding for left right
        if(isLeftHeld || isRightHeld)
        {
            return;
        }

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

        //Set playerside, using playerForward from monster manager to determine if player is on left or right side of the track
        if(monsterManager != null)
        {
            Vector3 playerForward = monsterManager.playerForward;
            if (playerForward.x >= 0.5f) // Track is oriented in the positive X direction
            {
                playerSide = (transform.position.z < centerLine.z) ? PlayerSide.Right : PlayerSide.Left;
            }
            else if (playerForward.x <= -0.5f) // Track is oriented in the negative X direction
            {
                playerSide = (transform.position.z < centerLine.z) ? PlayerSide.Left : PlayerSide.Right;
            }
            else if (playerForward.z >= 0.5f) // Track is oriented in the positive Z direction
            {
                playerSide = (transform.position.x < centerLine.x) ? PlayerSide.Left : PlayerSide.Right;
            }
            else if (playerForward.z <= -0.5f) // Track is oriented in the negative Z direction
            {
                playerSide = (transform.position.x < centerLine.x) ? PlayerSide.Right : PlayerSide.Left;
            }

            ui.SetText(ui.playerSideValue, playerSide.ToString());
        }
        
    }

    void ManageRigidBodyForce()
    {
        //Apply forward force based on movement state and left/right bias
        Vector3 forwardForceVector = transform.forward * minimumForce + transform.forward * forwardForce * movementMultiplier;
        Vector3 rightForceVector = Vector3.zero;
        
        //Left Right Movement
        if(leftRightMovement != 0)
        {
            forwardForceVector = transform.forward * minimumForce * forwardDuringLeftRightMultiplier;
            rightForceVector = transform.right * leftRightMovement * horizontalSpeed;
        }

        //Add the actual force
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
        ui.SetText(ui.movementText, currentState.ToString() + ": " + horizontalVelocity.magnitude.ToString("F0") ); 
    }

    // Tyvin: This rotates the player when they enter a camera rotate trigger zone.
    // TODO: Make the camera turns smoother and not instant.
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rotate Trigger"))
        {
            CustomCameraRotateQTE rotateScript = other.GetComponentInParent<CustomCameraRotateQTE>();
            Transform myRotatePoint = other.transform.parent.GetChild(1).transform;
            if(myRotatePoint.name == "Rotation Point")
            {
                float cameraTurnAmount = rotateScript.cameraRotation;
                transform.rotation = myRotatePoint.rotation * Quaternion.Euler(0f, cameraTurnAmount, 0f); // Tyvin: Temp fix for now to have the turn be 30 degrees.
                centerLine = myRotatePoint.position; // Update center line to the new position after rotation
                Debug.Log("Player entered rotation trigger, rotating player to match new camera angle. Rotation point: " + myRotatePoint.name);
            }
            else
            {
                Debug.LogError("Rotation Trigger's second child is not named 'Rotation Point'. Please check the setup of the rotation trigger.");
            }
        }
        monsterManager.StorePlayerForward();
        //Rotate the original CameraRotation to match the new angle
        originalCameraRotation = Quaternion.Euler(originalCameraXRotation, transform.eulerAngles.y, 0f);

    }
    
    void LookStart(InputAction.CallbackContext ctx)
    {
        isLooking = true;
    }

    void LookStop()
    {
        isLooking = false;
    }

    void HandleViewBobbing()
    {
        float bobbingAmount = 0f;
        float rollAmount = 0f;

        if (isLooking) return; // Don't bob when looking
        if (currentState == MovementState.Idle) return; // Don't bob when idle
        if (currentState == MovementState.Walking)
        {
            bobbingAmount = Mathf.Sin(Time.time * viewBobbingDisplacementSpeed) * viewBobbingDisplacementIntensity;
            rollAmount = Mathf.Sin(Time.time * viewBobbingRollSpeed) * viewBobbingRollIntensity;
        }
        else if (currentState == MovementState.Jogging)
        {
            bobbingAmount = Mathf.Sin(Time.time * viewBobbingDisplacementSpeed * 1.5f) * viewBobbingDisplacementIntensity * 1.5f;
            rollAmount = Mathf.Sin(Time.time * viewBobbingRollSpeed * 1.5f) * viewBobbingRollIntensity * 1.5f;
        }
        else if (currentState == MovementState.Sprinting)
        {
            bobbingAmount = Mathf.Sin(Time.time * viewBobbingDisplacementSpeed * 3f) * viewBobbingDisplacementIntensity * 3f;
            rollAmount = Mathf.Sin(Time.time * viewBobbingRollSpeed * 3f) * viewBobbingRollIntensity * 3f;
        }

        //Apply roll when player is moving left or right
        if(leftRightMovement != 0)
        {
            rollAmount -= leftRightMovement * viewBobbingRollIntensity * leftRightViewBobbingIntensity;
        }

        targetCameraPosition = new Vector3(originalCameraPosition.x, originalCameraPosition.y + bobbingAmount, originalCameraPosition.z);
        cam.transform.localPosition = targetCameraPosition;
        cam.transform.localRotation = cam.transform.localRotation * Quaternion.Euler(0f, 0f, rollAmount);
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
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, originalCameraRotation, Time.deltaTime * lookSpeed);
        }
    }


    public int GetPlayerSide()
    {
        return (playerSide == PlayerSide.Left) ? -1 : 1;
    }

    public void ResetMovement()
    {
        cadence = 0f;
        currentState = MovementState.Idle;
        movementMultiplier = 0f;
        leftRightBias = 0;
        leftRightMovement = 0f;
        lastDirectionInput = 0;
        leftRightTimer = 0f;

        //UI
        ui.SetText(ui.movementText, currentState.ToString() + ": " + "0");
        ui.SetSlider(ui.movementSlider, 0f);
    }

    public void OnDrawGizmos()
    {
        //Draw Three lines to represent the max horizontal range from the center line and the center line
        Gizmos.color = Color.blue;
        if(monsterManager == null) return;

        int myX = (int)Mathf.Round(monsterManager.playerForward.x);
        int myZ = (int)Mathf.Round(monsterManager.playerForward.z);
        
        if(myX == 1 || myX == -1)
        {
            //Center line
            Gizmos.DrawLine(centerLine, centerLine + new Vector3(myX * 50f, 0f, 0f));
            //Side lines
            Gizmos.DrawLine(centerLine + new Vector3(0f, 0f, -maxHorizontalRange), centerLine + new Vector3(myX * 50f, 0f, -maxHorizontalRange));
            Gizmos.DrawLine(centerLine + new Vector3(0f, 0f, maxHorizontalRange), centerLine + new Vector3(myX * 50f, 0f, maxHorizontalRange));
        }
        else if(myZ == 1 || myZ == -1)
        {
            //Center line
            Gizmos.DrawLine(centerLine, centerLine + new Vector3(0f, 0f, myZ * 50f));
            //Side lines
            Gizmos.DrawLine(centerLine + new Vector3(-maxHorizontalRange, 0f, 0f), centerLine + new Vector3(-maxHorizontalRange, 0f, myZ * 50f));
            Gizmos.DrawLine(centerLine + new Vector3(maxHorizontalRange, 0f, 0f), centerLine + new Vector3(maxHorizontalRange, 0f, myZ * 50f));
            
        }
        
    }
    public void ResetPlayerMovement()
    {
        // Stops all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        leftRightMovement = 0f;
        leftRightBias = 0;
        lastDirectionInput = 0;
        leftRightTimer = 0f;
        isLeftHeld = false;
        isRightHeld = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        cadence = 0f;
        cadenceTimer = 0f;
        currentState = MovementState.Idle;
        movementMultiplier = 0f;
    }
}

    
