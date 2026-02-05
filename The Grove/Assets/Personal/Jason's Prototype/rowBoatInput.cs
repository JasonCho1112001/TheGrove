using UnityEngine;
using UnityEngine.InputSystem;

public class rowBoatInput : MonoBehaviour
{
    //Inputs
    public InputActionReference leftInput;
    public InputActionReference rightInput;

    private InputAction leftAction;
    private InputAction rightAction;

    //Stats
    public float forwardForce = 2.5f;

    //Balance State
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

    void Update()
    {
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
        balanceState -= 1f;
        agitationMeter += 20f;

        //Reset agitation timer
        agitationTimer = agitationTimerMax;
    }

    void RightStep(InputAction.CallbackContext ctx)
    {
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
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
