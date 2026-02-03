using UnityEngine;
using System;
using UnityEngine.AI;

public class NavMeshPlayerControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEButton quickTimeEventScript;

    // This is where the player will attempt to follow the end of the path
    [Header("Exit Target")]
    [SerializeField] Transform exitPath;

    [Header("Player Settings")]
    //[SerializeField] private float playerSpeed = 2f;
    [SerializeField] private float horizontalSpeed = 3f;

    NavMeshAgent agent;
    Transform target;
    private bool isFrozen = false;
    public event Action OnResetPosition;

    private void Awake()
    {
        Debug.Log("DEBUG LOG: ON");

        quickTimeEventScript.enabled = false;
        quickTimeEventScript.OnQteComplete += UnfreezePlayer;
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.updateUpAxis = true;
        ExitTarget();
    }

    // For player input and movement
    private void Update()
    {
        if (isFrozen) {
            agent.isStopped = true;
            return;
        }
        agent.isStopped = false;
        
        if (exitPath != null)
            agent.SetDestination(exitPath.position);

        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x = 1f;

        Vector3 move = Vector3.forward * 2 + Vector3.right * (x * horizontalSpeed);
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
    }

    void ExitTarget()
    {
        target = exitPath;
    }

    private void UnfreezePlayer()
    {
        isFrozen = false;
    }
    public void ResetPosition()
    {
        OnResetPosition?.Invoke();
    }
}
