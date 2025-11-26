using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    
    [Header("Jump")]
    public float jumpPower = 5f;
    public float jumpCooldown = 0.1f;

    bool isJump;
    float lastJumpTime = -999f;
    Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && !isJump && Time.time - lastJumpTime > jumpCooldown)
        {
            isJump = true;
            lastJumpTime = Time.time;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        rigid.velocity = new Vector3(moveDir.x * speed, rigid.velocity.y, moveDir.z * speed);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckLanding();
    }

    void OnCollisionStay(Collision collision)
    {
        CheckLanding();
    }

    void CheckLanding()
    {
        if (Time.time - lastJumpTime > jumpCooldown)
        {
            isJump = false;
        }
    }

    public void SetGrounded(bool grounded)
    {
        if (grounded && Time.time - lastJumpTime > jumpCooldown)
        {
            isJump = false;
        }
    }
}
