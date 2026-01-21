using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    // Player Control Settings
    [Header("Player Settings")]
    //[SerializeField] public Camera playerCamera;
    [SerializeField] public float playerSpeed = 2f;
    [SerializeField] public float horizontalSpeed = 3f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x =  1f;

        Vector3 move = Vector3.forward * playerSpeed + Vector3.right * (x * horizontalSpeed);
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    // Collision detection debugging
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("TRIPPED ON:" + other.gameObject.name);
    }
}
