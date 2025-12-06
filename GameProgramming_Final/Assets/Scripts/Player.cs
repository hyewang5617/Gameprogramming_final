using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float sprintSpeed = 10f;
    
    [Header("Jump")]
    public float jumpPower = 5f;

    bool isGrounded = true;
    Rigidbody rigid;
    bool onVehicle = false;
    Camera cam;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isGrounded = false;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (onVehicle) return;

        Vector3 move = GetMoveVector();
        rigid.velocity = new Vector3(move.x, rigid.velocity.y, move.z);
    }

    Vector3 GetMoveVector()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        if (Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f) return Vector3.zero;

        if (cam == null) cam = Camera.main;
        
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;
        
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        Vector3 moveDir = (right * h + forward * v).normalized;
        return moveDir * moveSpeed;
    }

    public Vector3 GetPlayerInput()
    {
        return GetMoveVector();
    }

    public void SetOnVehicle(bool onVehicle)
    {
        this.onVehicle = onVehicle;
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
        if (collision.gameObject.CompareTag("Vehicle")) return;
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
