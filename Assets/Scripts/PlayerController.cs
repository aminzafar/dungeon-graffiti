using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.7f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    
    [Space]
    [Header("Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 4f;
    [SerializeField] private float maxLookAngle = 85f;

    [Space]
    [Header("Head Bobbing")]
    [SerializeField] private float bobSpeed = 14f;
    [SerializeField] private float bobAmount = 0.05f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private float cameraDefaultYPosition;
    private float bobTimer = 0f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraDefaultYPosition = playerCamera.transform.localPosition.y;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleHeadBob();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        Vector3 finalVelocity = moveDirection * moveSpeed;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // stick to slopes
        }
        if (Input.GetButton("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight); // -2f unrelated to slope precaution
            // this formula ensures the player jumps up exactly jumpHeight meters no matter how much gravity it was undergoing
            // vf2 = vi2 + 2ad
            // vi = sqrt(vf2 - (2 * a * d) )
            // since vf = 0, velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
        velocity.y += gravity * Time.deltaTime;

        finalVelocity.y = velocity.y;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        float inputMagnitude = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;

        if (inputMagnitude > 0.2f && controller.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float newY = cameraDefaultYPosition + Mathf.Sin(bobTimer) * bobAmount;

            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                newY,
                playerCamera.transform.localPosition.z
            );
        }
        else
        {
            bobTimer = 0f;

            float resetY = Mathf.Lerp(playerCamera.transform.localPosition.y, cameraDefaultYPosition, Time.deltaTime * bobSpeed);

            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                resetY,
                playerCamera.transform.localPosition.z
            );
        }
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic) return;

        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        
        body.linearVelocity = pushDirection * 2.5f; // to send the object faster than the player is moving 
    }
}