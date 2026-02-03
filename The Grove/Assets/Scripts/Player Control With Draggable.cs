using UnityEngine;
using System;

public class PlayerControlWithDraggable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEDrag quickTimeEventScript;

    [Header("Player Settings")]
    [SerializeField] private float playerSpeed = 2f;
    [SerializeField] private float horizontalSpeed = 3f;

    private Rigidbody rb;
    public bool isFrozen = false;
    public event Action OnResetPosition;

    private void Awake()
    {
        Debug.Log("DEBUG LOG: ON");
        Debug.Log("Player Control With Draggable Script Initialized");
        Debug.Log("Screen Width: " + Screen.width);
        Debug.Log("Screen Height: " + Screen.height);
        rb = GetComponent<Rigidbody>();

        quickTimeEventScript.enabled = false;
        quickTimeEventScript.OnQteComplete += UnfreezePlayer;
    }

    // For player input and movement
    private void FixedUpdate()
    {
        if (isFrozen) return;

        float x = 0f;

        // This is for left and right movement
        // This version DOES NOT use mouse input for movement, cause it was causing issues with the draggable QTE
        if  (Input.GetKey(KeyCode.Mouse0) || (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))) x = -1f;
        if  (Input.GetKey(KeyCode.Mouse1) ||(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))) x = 1f;

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
