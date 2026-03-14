using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class SafeRoomTrigger : MonoBehaviour
{
    NavMeshAgent agent;
    public bool isPlayerInSafeRoom = false;

    // While in a safe room, pause the friends movement by disabling the friends movement
    [Header("References")]
    [SerializeField] private NavMeshFriends navMeshFriendsScript;

    public void Start()
    {
        Debug.Log("SafeRoomTrigger Script On");
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInSafeRoom = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInSafeRoom = false;
        }
    }
}
