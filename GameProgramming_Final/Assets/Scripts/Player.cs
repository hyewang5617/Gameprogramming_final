using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    
    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 10f;
    
    [Header("Jump")]
    public float jumpPower = 5f;
    public float maxFallSpeed = 30f;
    
    [Header("Start Settings")]
    public float startHeight = 8f;
    public float startUpwardForce = 60f;

    bool isGrounded = true;
    Rigidbody rigid;
    bool onVehicle = false;
    bool canMove = true;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = startHeight;
        transform.position = pos;
        isGrounded = false;
        rigid.AddForce(Vector3.up * startUpwardForce, ForceMode.VelocityChange);
    }

    void Update()
    {
        if (canMove && Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        LimitFallSpeed();
        
        if (!canMove || onVehicle) return;

        Vector3 move = GetMoveVector();
        rigid.velocity = new Vector3(move.x, rigid.velocity.y, move.z);
    }

    void LimitFallSpeed()
    {
        if (rigid.velocity.y < -maxFallSpeed)
        {
            Vector3 vel = rigid.velocity;
            vel.y = -maxFallSpeed;
            rigid.velocity = vel;
        }
    }

    Vector3 GetMoveVector()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f) return Vector3.zero;

        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        return (right * h + forward * v).normalized * moveSpeed;
    }

    public Vector3 GetPlayerInput()
    {
        return canMove ? GetMoveVector() : Vector3.zero;
    }

    public void SetOnVehicle(bool onVehicle)
    {
        this.onVehicle = onVehicle;
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGrounded(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckGrounded(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Vehicle"))
            isGrounded = false;
    }

    void CheckGrounded(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle")) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.3f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    public void SetGrounded(bool grounded)
    {
        if (grounded) isGrounded = true;
    }
}
