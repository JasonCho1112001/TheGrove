using UnityEngine;
using System;
using UnityEngine.AI;

public class NavMeshFriends : MonoBehaviour
{
    // This is where the friends will attempt to follow the end of the path
    [Header("Exit Target")]
    [SerializeField] Transform exitPath;

    NavMeshAgent agent;
    Transform target;


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
        agent.isStopped = false;
        
        if (exitPath != null)
            agent.SetDestination(exitPath.position);

    }
    void ExitTarget()
    {
        target = exitPath;
    }
}
