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
    Vector3 moveInput; // 현재 이동 입력 값
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
            rigid.angularDrag = 5f; // 원통 회전 저항
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

    // 낙하 속도 제한
    void LimitFallSpeed()
    {
        if (rigid.velocity.y < -maxFallSpeed)
            rigid.velocity = new Vector3(rigid.velocity.x, -maxFallSpeed, rigid.velocity.z);
    }

    // 카메라 기준 이동 벡터 계산
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

    // 외부 스크립트에서 플레이어 입력 가져오기
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

    // 게임 시작 시 물리 활성화
    public void ReleaseFromStart()
    {
        rigid.isKinematic = false;
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

    // 충돌면이 지면인지 확인
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

    // 외부에서 지면 상태 설정 (VehicleRider에서 사용)
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
        if (grounded)
        {
            extraJumpsRemaining = maxExtraJumps;
        }
    }

    public void SetMaxExtraJumps(int count)
    {
        maxExtraJumps = Mathf.Max(0, count);
        extraJumpsRemaining = maxExtraJumps;
    }

    public Rigidbody GetRigidbody()
    {
        return rigid;
    }
}
