using UnityEngine;
using System;

public class PlayerControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuickTimeEvent quickTimeEventScript;

    [Header("Player Settings")]
    [SerializeField] private float playerSpeed = 2f;
    [SerializeField] private float horizontalSpeed = 3f;

    private Rigidbody rb;
    private bool isFrozen = false;
    public event Action OnResetPosition;

    private void Awake()
    {
        Debug.Log("DEBUG LOG: ON");
        rb = GetComponent<Rigidbody>();

        quickTimeEventScript.enabled = false;
        quickTimeEventScript.OnQteComplete += UnfreezePlayer;
    }

    // For player input and movement
    private void FixedUpdate()
    {
        if (isFrozen) return;

        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x = 1f;

        Vector3 move = Vector3.forward * playerSpeed + Vector3.right * (x * horizontalSpeed);
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    // Collision detection for obstacles and respawns
    private void OnCollisionEnter(Collision other)
    {
        // Freeze the player for now, and start the QTE
        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("TRIPPED ON:" + other.gameObject.name);
            FreezePlayer();
            quickTimeEventScript.enabled = true;
        }

        // Reset position when the player reaches the respawn point
        if (other.gameObject.CompareTag("Respawn"))
        {
            Debug.Log("Respawning Player");
            ResetPosition();
        }
    }

    private void FreezePlayer()
    {
        isFrozen = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void UnfreezePlayer()
    {
        isFrozen = false;
    }
    public void ResetPosition()
    {
        OnResetPosition?.Invoke();
        rb.position = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
