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

    //References
    private Rigidbody rb;

    staminaSystem stamina;

    void Awake()
    {   //Assign input actions
        if(leftInput != null) { leftAction = leftInput.action; 
            } else { Debug.Log("No left input assigned");}
        if(rightInput != null) { rightAction = rightInput.action; 
            } else { Debug.Log("No right input assigned");} 

        //Get references
        rb = GetComponent<Rigidbody>();
        stamina = GetComponent<staminaSystem>();
    }

    void Update()
    {
        //Currently polling input in Update for movement state determination
        DetermineMovementState();
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
    }

    void RightStep(InputAction.CallbackContext ctx)
    {
        rb.AddForce(transform.forward * forwardForce, ForceMode.Impulse);
        balanceState += 1f;
    }

    void DetermineMovementState()
    {
        //Determine movement state based on how frequent inputs are received


    }
}
