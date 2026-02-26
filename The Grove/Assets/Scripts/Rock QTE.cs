using UnityEngine;

public class RockQTE : MonoBehaviour
{
    [Header("Rock GameObject")]
    public GameObject rock;

    [Header("Camera Tilt Script")]
    public QTECameraTilt cameraTilt;

    [Header("Timer Script")]
    public QTETimerSystem timer;

    [Header("Stamina Depletion Settings")]
    public float depletedStamina = 1f;

    private PlayerRockQTEInput playerInput;
    private staminaSystem playerStamina;
    private float qTECooldownTimer = 0f;
    public float qTECooldownDuration = 2f;
    public bool isRockQTEActive = false;
    public bool isCooldownActive = false;

    private void Awake()
    {
        if (rock == null)
        {
            throw new System.Exception("Rock gameobject not assigned in inspector");
        }
        if (cameraTilt == null)
        {
            throw new System.Exception("Camera tilt script not assigned in inspector");
        }
        if (timer == null)
        {
            throw new System.Exception("Timer script not assigned in inspector");
        }

        playerInput = GetComponent<PlayerRockQTEInput>();
        playerStamina = GetComponent<staminaSystem>();
    }

    private void Update()
    {
        if (playerInput.enabled)
        {
            playerInput.TiltEnabled = true;

            if (playerInput.currentZ < -0.01f)
            {
                playerInput.TiltDirection = -1f;
            }
            if (playerInput.currentZ > 0.01f)
            {
                playerInput.TiltDirection = 1f;
            }
        }

        if ((timer.remainingTime <= 0.01f) && isRockQTEActive)
        {
            isRockQTEActive = false;
            cameraTilt.TiltCamera();
        }

        if ((Mathf.Abs(playerInput.currentZ) >= playerInput.maxAngle - .01f) && isRockQTEActive)
        {
            isRockQTEActive = false;
            cameraTilt.TiltCamera();
            cameraTilt.StartTripAnim();
            playerStamina.currentStamina -= depletedStamina;
            playerInput.InitiateTiltDirection();
        }

        if (isCooldownActive)
        {
            qTECooldownTimer += Time.deltaTime;
            if (qTECooldownTimer > qTECooldownDuration) isCooldownActive = false;
        }
        else
        {
            qTECooldownTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == rock.tag && !isCooldownActive)
        {
            Debug.Log("Rock QTE Started");
            isCooldownActive = true;
            isRockQTEActive = true;
            cameraTilt.TiltCamera();

            //Disable the collider to prevent multiple triggers
            other.enabled = false;
        }
    }

}
