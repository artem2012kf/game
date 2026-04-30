using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public ThirdPersonController playerController;

    [Header("Third Person")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);
    public float thirdPersonLookHeight = 1.5f;

    [Header("First Person Aim")]
    public Vector3 firstPersonOffset = new Vector3(0f, 1.65f, 0.25f);
    public bool useFirstPersonWhenAiming = true;

    [Header("Camera Settings")]
    public float mouseSensitivity = 3f;
    public float smoothSpeed = 12f;

    [Header("Clamp")]
    public float minY = -30f;
    public float maxY = 60f;

    private float yaw;
    private float pitch;

    void Start()
    {
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }

        if (playerController == null && target != null)
        {
            playerController = target.GetComponent<ThirdPersonController>();

            if (playerController == null)
            {
                playerController = target.GetComponentInParent<ThirdPersonController>();
            }

            if (playerController == null)
            {
                playerController = target.GetComponentInChildren<ThirdPersonController>();
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minY, maxY);

        bool aiming = false;

        if (playerController != null)
        {
            aiming = playerController.IsAiming();
        }

        if (useFirstPersonWhenAiming && aiming)
        {
            HandleFirstPersonAim();
        }
        else
        {
            HandleThirdPerson();
        }
    }

    void HandleThirdPerson()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = target.position + rotation * thirdPersonOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * thirdPersonLookHeight);
    }

    void HandleFirstPersonAim()
    {
        Quaternion fullRotation = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 desiredPosition = target.position + yawRotation * firstPersonOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            fullRotation,
            smoothSpeed * Time.deltaTime
        );
    }
}