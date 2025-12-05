using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    
    [Header("Jump")]
    public float jumpPower = 5f;

    bool isJump = false;
    Rigidbody rigid;
    bool onVehicle = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && !isJump)
        {
            isJump = true;
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (onVehicle) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * h + transform.forward * v).normalized;
        rigid.velocity = new Vector3(moveDir.x * speed, rigid.velocity.y, moveDir.z * speed);
    }

    public void SetOnVehicle(bool onVehicle)
    {
        this.onVehicle = onVehicle;
    }

    void OnCollisionEnter(Collision collision)
    {
        isJump = false;
    }

    void OnCollisionStay(Collision collision)
    {
        isJump = false;
    }

    public void SetGrounded(bool grounded)
    {
        if (grounded) isJump = false;
    }
}
