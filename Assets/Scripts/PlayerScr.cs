using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerScr : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] float maxLinearVelocity = 1.0f;
    [SerializeField] float cooldownJumpCount;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float maxLookAngle = 90f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;


    bool isGrounded = false, touchingWall = false;

    Rigidbody rb;
    GameController gc;
    ControlsController controls;

    private float cameraRotationX;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        gc = GameController.Instance;
        controls = gc.GetControlsController();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.maxLinearVelocity = maxLinearVelocity;
    }

    private void Update()
    {
        Look();
        Jump();
    }

    private void FixedUpdate()
    {
        Move();

        isGrounded = IsGrounded();

        if (!isGrounded && touchingWall)
        {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }
    }

    private void Move()
    {
        Vector3 direction = transform.forward * controls.Move.y + transform.right * controls.Move.x;

        rb.AddForce(direction * moveSpeed, ForceMode.Impulse);
        //Debug.Log(rb.linearVelocity.magnitude);
    }

    private void Look()
    {
        float mouseX = controls.MouseDelta.x * mouseSensitivity;
        float mouseY = controls.MouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraRotationX -= mouseY;
        cameraRotationX = Mathf.Clamp(
            cameraRotationX,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    private void Jump()
    {
        if (controls.Jump && (isGrounded || touchingWall) && cooldownJumpCount <= 0)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            cooldownJumpCount = .1f;

            rb.linearVelocity = transform.forward.normalized * rb.linearVelocity.magnitude;
        }

        if (cooldownJumpCount > 0) cooldownJumpCount -= Time.deltaTime;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < 0.3f)
            {
                touchingWall = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        touchingWall = false;
    }

}
