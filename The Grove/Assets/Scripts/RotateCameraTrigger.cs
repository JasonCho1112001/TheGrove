using UnityEngine;

public class RotateCameraTrigger : MonoBehaviour
{
    [Header("Trigger Player Camera and Player Rotation")]
    [SerializeField] public GameObject playerModel;
    [SerializeField] public GameObject playerCamera;

    // This deals with camera rotations 
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Collide with Rotate Camera Trigger");
            if (gameObject.CompareTag("Left Camera Rotate"))
            {
                Debug.Log("Entered Left Camera Trigger");
                playerCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                playerModel.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (gameObject.CompareTag("Right Camera Rotate"))
            {
                Debug.Log("Entered Right Camera Trigger");
                playerCamera.transform.rotation = Quaternion.Euler(0, 90, 0);
                playerModel.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
    }
}
