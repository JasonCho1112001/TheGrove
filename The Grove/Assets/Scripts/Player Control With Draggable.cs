using UnityEngine;
using System;

public class PlayerControlWithDraggable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEDrag quickTimeEventScript;
    [SerializeField] private rowBoatInput rowBoatInput;


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

    private void FixedUpdate()
    {
        if (isFrozen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (rowBoatInput != null)
                rowBoatInput.enabled = false;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (rowBoatInput != null)
                rowBoatInput.enabled = true;
        }
    }

    private void FreezePlayer()
    {
        Debug.Log("Freezing Player");
        isFrozen = true;
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
