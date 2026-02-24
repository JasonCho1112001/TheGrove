using System.Collections;
using UnityEngine;

public class QTECameraTilt : MonoBehaviour
{
    [Header("Tilt and Rotation Settings")]
    public float tiltX = 60f;
    public float rotateDuration = .75f;

    [Header("Timer Script")]
    public QTETimerSystem timer;

    [Header("RockQTE Script")]
    public RockQTE rockQTE;

    [Header("Player Movement Script")]
    public rowBoatInput playerInput;

    [Header("Player Rock Input Script")]
    public PlayerRockQTEInput playerRockInput;

    [Header("Camera Trip Settings")]
    public bool isTripping;
    public float tripDuration = .5f;
    public float tripLurch = 17f;
    public float shakeFrequency = 10f;
    public float shakeStrength = 30f;
    public float smoothTime = 0.12f;

    private float initialX;
    private float initialZ;
    private float tripTimer = 0f;
    private float currentX;
    private float currentZ;
    private float velocityX;
    private float velocityZ;

    private void Awake()
    {
        initialX = transform.localEulerAngles.x;
        initialZ = transform.localEulerAngles.z;

        if (timer == null)
        {
            throw new System.Exception("Timer script not assigned in inspector");
        }
        if (rockQTE == null)
        {
            throw new System.Exception("Rock QTE script not assigned in inspector");
        }
        if (playerInput == null)
        {
            throw new System.Exception("Player Input script not assigned in inspector");
        }
        if (playerRockInput == null)
        {
            throw new System.Exception("Player Rock Input script not assigned in inspector");
        }
        else
        {
            playerRockInput.enabled = false;
        }
    }

    private void LateUpdate()
    {
        //Deleted misleading excess code :)
        //UI
        playerRockInput.uI.SetText(playerRockInput.uI.timerText, $"{timer.remainingTime:F2}");
    }

    public void TiltCamera()
    {
        StopAllCoroutines();
        StartCoroutine(RotateCamera());
    }

    private IEnumerator RotateCamera()
    {
        if (rockQTE.isRockQTEActive) yield return StartCoroutine(RotateToAngle(tiltX, rotateDuration));
        else 
        { 
            yield return StartCoroutine(RotateToAngle(initialX, rotateDuration)); 
        }
    }

    private IEnumerator RotateToAngle(float targetX, float duration)
    {
        float time = 0f;
        float startX = transform.localEulerAngles.x;

        playerInput.enabled = false;
        playerRockInput.enabled = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float angleX = Mathf.LerpAngle(startX, targetX, t);

            transform.localEulerAngles = new Vector3(
                angleX,
                transform.localEulerAngles.y,
                initialZ
                );

            yield return null;
        }

        transform.localEulerAngles = new Vector3(
            targetX,
            transform.localEulerAngles.y,
            initialX
            );

        if (rockQTE.isRockQTEActive)
        {
            timer.StartTimer();
            playerRockInput.enabled = true;
        }
        else
        {
            playerInput.enabled = true;
            playerRockInput.ResetCurrentZ();
            timer.ResetTimer();
        }
    }
    public void StartTripAnim()
    {
        isTripping = true;
    }

    public void EndTripAnim()
    {
        isTripping = false;
    }
}
