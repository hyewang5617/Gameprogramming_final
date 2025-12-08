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
    public float curveLookAhead = 0.3f; // 다음 웨이포인트를 미리 보는 비율 (0~1)
    public float sharpTurnMultiplier = 2f; // 급격한 커브에서 회전 속도 배율
    public float curveStartDistance = 2f; // 현재 웨이포인트에서 이 거리 이내일 때만 커브 시작
    
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
        
        // 부드러운 곡선을 위한 타겟 위치 계산 (다음 웨이포인트를 미리 고려)
        Vector3 targetPosition = GetCurvedTargetPosition();
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        if (direction.magnitude > 0.01f)
        {
            rigid.AddForce(direction * moveForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // 급격한 커브 감지 (90도 커브 등)
            float currentRotSpeed = rotationSpeed;
            if (currentWaypointIndex + 1 < waypoints.Length)
            {
                Transform currentWaypoint = waypoints[currentWaypointIndex];
                Transform nextWaypoint = waypoints[currentWaypointIndex + 1];
                if (currentWaypoint != null && nextWaypoint != null)
                {
                    Vector3 currentDir = (currentWaypoint.position - transform.position).normalized;
                    Vector3 nextDir = (nextWaypoint.position - currentWaypoint.position).normalized;
                    currentDir.y = 0f;
                    nextDir.y = 0f;
                    
                    float angle = Vector3.Angle(currentDir, nextDir);
                    if (angle > 45f) // 45도 이상이면 급격한 커브로 간주
                        currentRotSpeed = rotationSpeed * sharpTurnMultiplier;
                }
            }
            
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, Time.fixedDeltaTime * currentRotSpeed);
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
    
    // 부드러운 곡선을 위한 타겟 위치 계산
    Vector3 GetCurvedTargetPosition()
    {
        if (currentWaypointIndex >= waypoints.Length) 
            return transform.position;
        
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (currentWaypoint == null) return transform.position;
        
        float distanceToCurrent = Vector3.Distance(transform.position, currentWaypoint.position);
        
        // 현재 웨이포인트에 충분히 가까워질 때까지는 현재 웨이포인트만 바라봄
        if (distanceToCurrent > curveStartDistance)
            return currentWaypoint.position;
        
        // 다음 웨이포인트가 있고, 현재 웨이포인트에 가까워졌을 때만 커브 시작
        if (currentWaypointIndex + 1 < waypoints.Length)
        {
            Transform nextWaypoint = waypoints[currentWaypointIndex + 1];
            if (nextWaypoint != null)
            {
                // 웨이포인트를 거의 지나간 경우에만 커브 시작 (뒤로 가는 것 방지)
                Vector3 toWaypoint = (currentWaypoint.position - transform.position).normalized;
                Vector3 vehicleForward = transform.forward;
                float waypointBehind = Vector3.Dot(toWaypoint, vehicleForward);
                
                // 웨이포인트가 뒤에 있거나 거의 지나갔을 때만 커브 시작
                if (waypointBehind < 0.5f && distanceToCurrent < curveStartDistance)
                {
                    float totalDistance = Vector3.Distance(currentWaypoint.position, nextWaypoint.position);
                    
                    if (totalDistance > 0.01f)
                    {
                        // 현재 웨이포인트에 가까울수록 다음 웨이포인트를 더 많이 바라봄
                        float normalizedDistance = Mathf.Clamp01(distanceToCurrent / curveStartDistance);
                        float t = Mathf.Lerp(curveLookAhead, 0f, normalizedDistance);
                        Vector3 curvedTarget = Vector3.Lerp(currentWaypoint.position, nextWaypoint.position, t);
                        
                        // 보간된 타겟이 현재 위치보다 뒤에 있지 않도록 확인
                        Vector3 toCurvedTarget = (curvedTarget - transform.position).normalized;
                        float dot = Vector3.Dot(vehicleForward, toCurvedTarget);
                        
                        // 앞으로 가는 방향이 아니면 현재 웨이포인트만 바라봄
                        if (dot > 0.3f)
                            return curvedTarget;
                    }
                }
            }
        }
        
        return currentWaypoint.position;
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

