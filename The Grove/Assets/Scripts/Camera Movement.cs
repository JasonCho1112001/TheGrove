using UnityEngine;

// Handles mouse camera movement
// This script was used from my CMPM 125: Game Technologies course with modifications to fit the game's needs
public class MouseCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEDrag quickTimeEventScript;
    public Camera playerCamera;
    public static MouseCamera instance;
    [SerializeField] private PlayerControlWithDraggable playerController;
    [SerializeField] private float sens = 10.0f;
    [SerializeField] private float x;
    [SerializeField] private float y;

    void Start()
    {
        playerCamera = GetComponent<Camera>();

        Vector3 euler = transform.rotation.eulerAngles;
        x = euler.x;
        y = euler.y;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerControlWithDraggable>();
    }

    // Update is called once per frame
    void Update()
    {

        if (playerController != null && playerController.isFrozen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            const float yMin = -89.9f;
            const float yMax = 89.9f;

            x += Input.GetAxis("Mouse X") * (sens * Time.deltaTime);
            y -= Input.GetAxis("Mouse Y") * (sens * Time.deltaTime);
            y = Mathf.Clamp(y, yMin, yMax);

            transform.rotation = Quaternion.Euler(y, x, 0.0f);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}