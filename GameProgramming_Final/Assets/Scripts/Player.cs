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

    void Awake()
    {
        isJump = false;
        rigid = GetComponent<Rigidbody>();
        
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
        if (collision.gameObject.name == "Floor")
        {
            isJump = false;
        }
    }
}
