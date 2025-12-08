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

    bool isGrounded = true;
    Rigidbody rigid;
    bool onVehicle = false;
    bool canMove = true;
    Vector3 moveInput;
    int maxExtraJumps = 0;
    int extraJumpsRemaining = 0;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        if (cam == null) cam = Camera.main;
        
        if (rigid != null)
        {
            rigid.freezeRotation = true;
            rigid.drag = 0.5f;
            rigid.angularDrag = 5f;
            rigid.interpolation = RigidbodyInterpolation.Interpolate;
            rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = startHeight;
        transform.position = pos;
        rigid.isKinematic = true;
        isGrounded = false;
    }

    void Update()
    {
        moveInput = GetMoveVector();
        
        if (canMove && Input.GetButtonDown("Jump") && (isGrounded || onVehicle || extraJumpsRemaining > 0))
        {
            bool wasGrounded = isGrounded || onVehicle;
            isGrounded = false;
            if (onVehicle) SetOnVehicle(false);
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            if (!wasGrounded && extraJumpsRemaining > 0)
            {
                extraJumpsRemaining--;
            }
        }
    }

    void FixedUpdate()
    {
        LimitFallSpeed();
        
        if (rigid != null)
        {
            rigid.rotation = Quaternion.identity;
        }
        
        if (!canMove || onVehicle) return;

        if (moveInput.magnitude > 0.01f)
        {
            Vector3 targetVelocity = new Vector3(moveInput.x, rigid.velocity.y, moveInput.z);
            rigid.velocity = targetVelocity;
        }
        else
        {
            Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
        }
    }

    void LimitFallSpeed()
    {
        if (rigid.velocity.y < -maxFallSpeed)
            rigid.velocity = new Vector3(rigid.velocity.x, -maxFallSpeed, rigid.velocity.z);
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

    public Vector3 GetPlayerInput() => canMove ? GetMoveVector() : Vector3.zero;
    public void SetOnVehicle(bool onVehicle) => this.onVehicle = onVehicle;
    public void SetCanMove(bool canMove) => this.canMove = canMove;
    public void ReleaseFromStart() => rigid.isKinematic = false;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.GameOver();
            }
            return;
        }
        
        CheckGrounded(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.GameOver();
            }
            return;
        }
        
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
                extraJumpsRemaining = maxExtraJumps;
                break;
            }
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (grounded) extraJumpsRemaining = maxExtraJumps;
    }

    public void SetMaxExtraJumps(int count)
    {
        maxExtraJumps = Mathf.Max(0, count);
        extraJumpsRemaining = maxExtraJumps;
    }

    public Rigidbody GetRigidbody() => rigid;
    public bool IsGrounded() => isGrounded;
    public bool IsOnVehicle() => onVehicle;
}
