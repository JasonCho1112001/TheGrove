using UnityEngine;

public class playerTargetScript : MonoBehaviour
{
    public GameObject player;

    void Update()
    {
        transform.position = player.transform.position;
        transform.rotation = player.transform.rotation;
    }

}
