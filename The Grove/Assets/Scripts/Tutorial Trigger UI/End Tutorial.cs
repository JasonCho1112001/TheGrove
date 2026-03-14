using UnityEngine;

public class EndTutorial : MonoBehaviour
{

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           Debug.Log("Add to leave scene here :D");
        }
    }
}