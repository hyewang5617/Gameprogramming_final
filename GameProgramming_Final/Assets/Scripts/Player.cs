using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stat")]
    public float speed = 5f; // 이동 속도
    public float jumpPower; //점프 정도

    bool isJump;
    Rigidbody rigid;
    Transform originalParent;

    void Awake()
    {
        isJump = false;
        rigid = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        
        // 구체가 굴러가지 않도록 회전 고정 (X, Z축만)
        rigid.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && !isJump)
        {
            isJump = true;
            rigid.AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 플레이어가 보는 방향 기준으로 이동
        Vector3 moveDirection = (transform.right * h + transform.forward * v).normalized;
        
        Vector3 targetVelocity = moveDirection * speed;
        rigid.velocity = new Vector3(targetVelocity.x, rigid.velocity.y, targetVelocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 바닥이나 차에 착지
        isJump = false;
        
        // 차 위에 올라탔는지 확인 (위쪽 면에만 착지)
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            // 충돌 지점의 노멀 벡터 확인 (위쪽인지 판단)
            foreach (ContactPoint contact in collision.contacts)
            {
                // 노멀 벡터의 y값이 0.5 이상이면 위쪽 면
                if (contact.normal.y > 0.5f)
                {
                    transform.SetParent(collision.transform);
                    break;
                }
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // 바닥이나 차에 닿아있으면 점프 가능 (옆면도 포함)
        isJump = false;
        
        // 차 위에 계속 있는지 확인
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            bool onTop = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    onTop = true;
                    break;
                }
            }
            
            // 위쪽 면에 있지 않으면 부모 관계 해제 (옆으로 미끄러짐)
            if (!onTop && transform.parent == collision.transform)
            {
                transform.SetParent(originalParent);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // 차에서 떨어지면 부모 관계 해제
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            transform.SetParent(originalParent);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 골인 지점 체크
        if (other.CompareTag("Goal"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.LevelComplete();
        }
    }
}
