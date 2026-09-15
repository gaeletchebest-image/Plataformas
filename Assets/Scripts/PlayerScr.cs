using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UIElements.Experimental;

public class PlayerScr : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] float maxLinearVelocity = 1.0f;
    [SerializeField] float cooldownJumpCount;
    [SerializeField] float wallImpulse = 1;
    [SerializeField] float impulseForce = 5f, impulseCooldown = .5f, impulseTime = 2f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float maxLookAngle = 90f;
    private float cameraRotationX;

    public List<TouchedObjects> touchObjs = new List<TouchedObjects>();

    bool isGrounded = false, touchingWall = false, impulsed = false;

    float impulseTimeCount = 0, impulseCooldownCount = 0;

    Rigidbody rb;
    GameController gc;
    ControlsController controls;

    

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

        if (impulseCooldownCount > 0) 
        {
            impulseCooldownCount -= Time.deltaTime;
            if (impulseCooldownCount <= 0) impulsed = false;
        }

        if (impulseTimeCount > 0)
        {
            impulseTimeCount -= Time.deltaTime;
            if (impulseTimeCount <= 0)
                rb.maxLinearVelocity = maxLinearVelocity;
            else
                rb.maxLinearVelocity = maxLinearVelocity + (impulseForce - maxLinearVelocity) * impulseTimeCount / impulseTime;
        }

    }

    private void FixedUpdate()
    {
        Move();

        isGrounded = IsGrounded();

        if (!isGrounded && touchingWall)
        {
            rb.AddForce(Physics.gravity, ForceMode.Acceleration);
        }

        List<TouchedObjects> plataforms = touchObjs.Where(x => x.plataformComponent != null).ToList();
        
        if (plataforms.Count > 0)
        {
            Plataform plat = plataforms.FirstOrDefault().plataformComponent;
            rb.MovePosition(rb.position + plat.GetDelta());
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
        bool canJumpWall = touchingWall;
        if (canJumpWall)
        {
            if (touchObjs.Count(x => x.obj.tag == "CannotJump") == touchObjs.Count) canJumpWall = false;
        }

        if (controls.Jump && (isGrounded || canJumpWall) && cooldownJumpCount <= 0)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            cooldownJumpCount = .1f;

            Vector3 jumpDirection = transform.forward.normalized;

            if (touchingWall)
            {
                List<TouchedObjects> walls = touchObjs.Where(x => x.sumNormals().y < 0.3f).ToList();
                if (walls.Count > 0)
                {
                    Vector3 wallNormal = Vector3.zero;
                    walls.ForEach(x => wallNormal.x += x.sumNormals().x);
                    walls.ForEach(x => wallNormal.y += x.sumNormals().y);
                    walls.ForEach(x => wallNormal.z += x.sumNormals().z);
                    wallNormal = wallNormal.normalized;

                    float dot = Vector3.Dot(wallNormal, jumpDirection);
                    jumpDirection -= wallNormal * dot;

                    jumpDirection += wallNormal * wallImpulse;
                }
            }

            rb.linearVelocity = jumpDirection * rb.linearVelocity.magnitude;
        }

        if (cooldownJumpCount > 0) cooldownJumpCount -= Time.deltaTime;
    }

    private bool IsGrounded() => touchObjs.Any(x => x.normalContacts.Any(y => y.y > 0.3f));

    void Impulse(Vector3 dir)
    {
        if (impulsed) return;

        rb.maxLinearVelocity = impulseForce;
        rb.AddForce(dir * impulseForce, ForceMode.Impulse);

        impulsed = true;
        impulseCooldownCount = impulseCooldown;
        impulseTimeCount = impulseTime;
    }

    private void OnCollisionStay(Collision collision)
    {
        TouchedObjects nse1 = new TouchedObjects(collision.gameObject);
        foreach (ContactPoint contact in collision.contacts)
        {
            nse1.addNormalContact(contact.normal);
            if (contact.normal.y < 0.3f)
            {
                touchingWall = true;
                break;
            }
        }
        touchObjs.RemoveAll(x => x.obj == collision.gameObject);
        touchObjs.Add(nse1);

    }

    private void OnCollisionExit(Collision collision)
    {
        touchObjs.RemoveAll(x => x.obj == collision.gameObject);
        touchingWall = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Impulsor")
            Impulse(other.transform.forward);
    }

    public void ResetPlayer(Vector3 pos)
    {
        rb.linearVelocity = Vector3.zero;
        rb.maxLinearVelocity = maxLinearVelocity;

        cooldownJumpCount = 0;
        impulseTimeCount = 0; impulseCooldownCount = 0; impulsed = false;

        rb.MovePosition(pos);
    }

}

[Serializable]
public class TouchedObjects
{
    public GameObject obj;
    public List<Vector3> normalContacts;
    public Plataform plataformComponent;

    public TouchedObjects(GameObject obj1)
    {
        obj = obj1;
        normalContacts = new List<Vector3>();

        if (obj.tag == "Plataform") plataformComponent = obj.GetComponentInParent<Plataform>();
        else plataformComponent = null;
    }

    public void addNormalContact(Vector3 contact)
    {
        contact.x = MathF.Round(contact.x, 2);
        contact.y = MathF.Round(contact.y, 2);
        contact.z = MathF.Round(contact.z, 2);
        normalContacts.Add(contact);
    }

    public Vector3 sumNormals()
    {
        Vector3 vec = Vector3.zero;
        normalContacts.ForEach(x => vec += x);
        return vec.normalized;
    }

}
