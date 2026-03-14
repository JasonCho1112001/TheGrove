using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRockQTEInput : MonoBehaviour
{
    [Header("Player Input References")]
    public InputActionReference leftInput;
    public InputActionReference rightInput;

    public InputAction leftAction;
    public InputAction rightAction;

    [Header("Camera Object")]
    public GameObject cameraTransform;

    [Header("Rotation Settings")]
    public float playerStrength = 6f;
    public float maxAngle = 35f;

    [Header("UI Manager Reference")]
    public uiManager uI;

    [Header("Smooth Tilt Settings")]
    public float smoothTime = .15f;

    [Header("Auto Tilt Settings")]
    public bool TiltEnabled = false;
    public float TiltDirection;
    public float TiltStrength = 28f;

    private float targetZ = 0f;
    private float smoothVelocity = 0f;
    public float currentZ;

    private void Awake()
    {
        if (leftInput != null)
        {
            leftAction = leftInput.action;
        }
        else 
        {
            throw new System.Exception("Player Input reference not assigned in inspector");
        }

        if (rightInput != null)
        {
            rightAction = rightInput.action;
        }
        else
        {
            throw new System.Exception("Player Input reference not assigned in inspector");
        }

        InitiateTiltDirection();
    }

    private void Update()
    {
        if (!TiltEnabled) return;

        targetZ += TiltDirection * TiltStrength * Time.deltaTime;
        targetZ = Mathf.Clamp(targetZ, -maxAngle, maxAngle);
    }

    private void LateUpdate()
    {
        ApplyRotate();
    }

    private void OnEnable()
    {
        leftAction.Enable();
        rightAction.Enable();

        leftAction.performed += LeftRotation;
        rightAction.performed += RightRotation;
    }

    private void OnDisable()
    {
        leftAction.performed -= LeftRotation;
        rightAction.performed -= RightRotation;

        leftAction.Disable();
        rightAction.Disable();
    }

    private void LeftRotation(InputAction.CallbackContext ctx)
    {
        targetZ += playerStrength;
        targetZ = Mathf.Clamp(targetZ, -maxAngle, maxAngle);
    }

    private void RightRotation(InputAction.CallbackContext ctx)
    {
        targetZ -= playerStrength;
        targetZ = Mathf.Clamp(targetZ, -maxAngle, maxAngle);
    }

    public void ApplyRotate()
    {
        currentZ = Mathf.SmoothDamp(
            currentZ,
            targetZ,
            ref smoothVelocity,
            smoothTime
            );

        cameraTransform.transform.localEulerAngles = new Vector3(
            cameraTransform.transform.localEulerAngles.x,
            cameraTransform.transform.localEulerAngles.y,
            currentZ
        );

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (uI == null) return;

        float sliderValue = Mathf.InverseLerp(0, maxAngle, Mathf.Abs(currentZ));

        uI.SetSlider(uI.angleSlider, sliderValue);
        uI.SetText(uI.angleText, Mathf.CeilToInt(currentZ).ToString());
    }

    public void ResetCurrentZ()
    {
        currentZ = 0f;
        targetZ = 0f;
    }

    public void InitiateTiltDirection()
    {
        TiltDirection = Random.value < 0.5f ? -1 : 1;
    }
}
