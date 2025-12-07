using UnityEngine;
using System;

public class VehiclePlatform : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    
    [Header("Movement")]
    public float speed = 10f;
    public float waypointReachDistance = 0.5f;
    public float rotationSpeed = 5f;
    public float moveForce = 2000f;
    
    [Header("Despawn")]
    public bool destroyAtEnd = true;
    
    public Action onDestroyed;
    
    int currentWaypointIndex = 0; // 현재 목표 웨이포인트 인덱스
    Rigidbody rigid;
    bool initialized = false; // 초기화 완료 여부
    float originalMoveForce; // 원래 moveForce 값

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        if (rigid != null)
        {
            rigid.isKinematic = false;
            rigid.useGravity = true;
            rigid.mass = 500f;
            rigid.drag = 0.5f;
            rigid.angularDrag = 5f;
        }
        
        originalMoveForce = moveForce;
    }

    // moveForce 설정 (초기 스폰 시 빠른 속도용)
    public void SetMoveForce(float force)
    {
        moveForce = force;
    }

    // moveForce를 원래 값으로 복원
    public void ResetMoveForce()
    {
        moveForce = originalMoveForce;
    }

    void Start()
    {
        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody>();
            Debug.LogWarning("[VehiclePlatform] Rigidbody가 없습니다!");
        }
        
        if (waypoints != null && waypoints.Length > 0)
        {
            InitializeMovement();
        }
    }

    // 웨이포인트 방향으로 초기 회전 설정
    public void InitializeMovement()
    {
        if (waypoints == null || waypoints.Length == 0 || rigid == null || initialized) return;
        
        initialized = true;
        
        if (currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
        {
            Transform target = waypoints[currentWaypointIndex];
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f;
            direction.Normalize();
            
            if (direction.magnitude > 0.01f)
                rigid.rotation = Quaternion.LookRotation(direction);
        }
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            if (initialized) InitializeMovement();
            return;
        }
        if (currentWaypointIndex >= waypoints.Length) return;
        if (rigid == null) return;
        
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;
        
        Vector3 direction = (target.position - transform.position).normalized;
        
        if (direction.magnitude > 0.01f)
        {
            rigid.AddForce(direction * moveForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
        if (horizontalVelocity.magnitude > speed)
        {
            horizontalVelocity = horizontalVelocity.normalized * speed;
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
        }
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (destroyAtEnd)
                {
                    onDestroyed?.Invoke();
                    Destroy(gameObject);
                }
                else
                    currentWaypointIndex = 0;
            }
        }
    }
    
    // 에디터에서 경로 시각화
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            }
        }
        
        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.5f);
        }
    }
}

