using UnityEngine;


//Source: https://www.youtube.com/watch?v=5Rq8A4H6Nzw
public class FirstPersonCamera : MonoBehaviour
{
    //Camera Movement Sensitivity and Rotations
    [Header("Camera Settings")]
    public Transform player;
    public float mouseSensitivity = 2f;
    float cameraVerticalRotation = 0f;
    float cameraHorizontalRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float inputX = Input.GetAxis("Mouse X")* mouseSensitivity;
        float inputY = Input.GetAxis("Mouse Y")* mouseSensitivity;

        // Rotate the Cameras X axis
        cameraVerticalRotation -= inputY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -80f, 80f);
        transform.localEulerAngles = Vector3.right * cameraVerticalRotation;
    }
}
