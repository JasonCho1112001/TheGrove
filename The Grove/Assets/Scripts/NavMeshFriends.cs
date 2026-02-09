using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;

//Source: https://discussions.unity.com/t/how-to-stop-navmeshagent-immediately/174293
public class NavMeshFriends : MonoBehaviour
{
    // This is where the friends will attempt to follow the end of the path
    [Header("Exit Target")]
    [SerializeField] Transform exitPath;

    // To check if the player is in the safe room. If so, pause movement.
    [Header("Safe Room Trigger")]
    [SerializeField] List<SafeRoomTrigger> safeRoomTriggerScripts;

    NavMeshAgent agent;
    Transform target;
    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.updateUpAxis = true;
        ExitTarget();
    }

    // For player input and movement
    public void Update()
    {
        agent.isStopped = false;
        
        if (exitPath != null)
            agent.SetDestination(exitPath.position);

        foreach (SafeRoomTrigger trigger in safeRoomTriggerScripts)
        {
            if (trigger.isPlayerInSafeRoom)
                agent.isStopped = true;
        }

    }
    void ExitTarget()
    {
        target = exitPath;
    }

}
