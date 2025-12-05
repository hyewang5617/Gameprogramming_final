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

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;
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

    public Vector3 GetPlayerInput()
    {
        return GetMoveVector();
    }

    Vector3 GetMoveVector()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float moveSpeed = isGrounded && Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;
        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        return moveDir * moveSpeed;
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
        isGrounded = false;
    }

    void CheckGrounded(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
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
