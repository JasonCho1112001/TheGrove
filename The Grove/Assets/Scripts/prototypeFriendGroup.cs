using UnityEngine;

public class prototypeFriendGroup : MonoBehaviour
{
    //Variables
    public float speed = 5f;

    //References
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    
    void FixedUpdate()
    {
        //Simply move forward at a constant speed
        rb.linearVelocity = transform.forward * speed;
    }
}
