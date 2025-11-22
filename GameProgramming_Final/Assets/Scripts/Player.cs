using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f; // 이동 속도
    
    [Header("Jump")]
    public float jumpPower = 5f; // 점프 파워
    public float jumpCooldown = 0.1f; // 점프 쿨타임 (연타 방지)

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
}
