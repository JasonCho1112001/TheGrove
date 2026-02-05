using UnityEngine;
using UnityEngine.InputSystem;


// This version is when the user use A or D, it will move the player left or right
public class rowBoatInputModified : MonoBehaviour
{
    //Inputs
    public InputActionReference leftInput;
    public InputActionReference rightInput;

    private InputAction leftAction;
    private InputAction rightAction;

    //Stats
    public float forwardForce = 2.5f;
    public float sidewaysForce = 100f;

    //Camera Movement Sensitivity and Rotations
    [Header("Camera Settings")]
    public Camera playerCamera;
    //public static MouseCamera instance;
    [SerializeField] private float sens = 10.0f;
    [SerializeField] private float x;
    [SerializeField] private float y;

    //Balance State
    [Header("Character States")]
    [SerializeField]
    private float balanceState = 0f;

    //Movement State
    public enum MovementState { Idle, Walking, Sprinting}
    [SerializeField]
    private MovementState currentState = MovementState.Idle;

    //Agitation (Help determine player intent through input frequency)
    [SerializeField]
    private float agitationMeter = 0f;
    private float maxAgitation = 100f;
    private float agitationTimerMax = 1f;
    private float agitationTimer = 0f;
    public float agitationDecayRate = 150f;
    public float passiveAgitationDecayRate = 25f;

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

    void Start()
    {
        playerCamera = GetComponent<Camera>();

        Vector3 euler = transform.rotation.eulerAngles;
        x = euler.x;
        y = euler.y;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Clamps camera rotation to prevent flipping
        const float yMin = -89.9f;
        const float yMax = 89.9f;

        x += Input.GetAxis("Mouse X") * (sens * Time.deltaTime);
        y -= Input.GetAxis("Mouse Y") * (sens * Time.deltaTime);
        y = Mathf.Clamp(y, yMin, yMax);

        transform.rotation = Quaternion.Euler(y, x, 0.0f);

        //Currently polling input in Update for movement state determination
        ManageMovementState();
    }

    void OnEnable()
    {
        leftAction.Enable(); 
        leftAction.performed += LeftStep;

        rightAction.Enable();
        rightAction.performed += RightStep;
    }

    void OnDisable()
    {
        leftAction.Disable(); 
        leftAction.performed -= LeftStep;

        rightAction.Disable();
        rightAction.performed -= RightStep;
    }
    
    void LeftStep(InputAction.CallbackContext ctx)
    {
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
        transform.Translate(Vector3.left * sidewaysForce * Time.deltaTime);
        balanceState -= 1f;
        agitationMeter += 20f;

        //Reset agitation timer
        agitationTimer = agitationTimerMax;
    }

    void RightStep(InputAction.CallbackContext ctx)
    {
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
        transform.Translate(Vector3.right * sidewaysForce * Time.deltaTime);
        balanceState += 1f;
        agitationMeter += 20f;

        //Reset agitation timer
        agitationTimer = agitationTimerMax;
    }

    void ManageMovementState()
    {
        //Determine movement state based on how frequent inputs are received
        if (agitationMeter <= 10f)
        {
            currentState = MovementState.Idle;
            ui.SetText(ui.agitationText, "Idle" );
        }
        else if (agitationMeter > 10f && agitationMeter <= 50f)
        {
            currentState = MovementState.Walking;
            ui.SetText(ui.agitationText, "Walking" );
        }
        else if (agitationMeter > 50f)
        {
            currentState = MovementState.Sprinting;
            ui.SetText(ui.agitationText, "Sprinting" );
        }

        //Deplete agitation meter over time
        agitationMeter -= passiveAgitationDecayRate * Time.deltaTime;
        //Agitation Timer
        if (agitationTimer > 0f)
        {
            agitationTimer -= Time.deltaTime;
        }
        else
        {
            agitationMeter -= agitationDecayRate * Time.deltaTime;
        }
        //TODO: Make agitation decay after timer wears out

        //Cap agitation meter
        if (agitationMeter < 0f)
        {
            agitationMeter = 0f;
        } 
        if (agitationMeter > maxAgitation)
        {
            agitationMeter = maxAgitation;
        }

        //Update agitation meter UI
        ui.SetSlider(ui.agitationSlider, agitationMeter / 100f);
    }
}
