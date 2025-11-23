using UnityEngine;
using System;

public class VehiclePlatform : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [Tooltip("차가 따라갈 웨이포인트들 (순서대로)")]
    public Transform[] waypoints;
    
    [Header("Movement")]
    [Tooltip("차의 이동 속도")]
    public float speed = 10f;
    [Tooltip("웨이포인트 도착 인식 거리")]
    public float waypointReachDistance = 0.5f;
    [Tooltip("회전 속도")]
    public float rotationSpeed = 5f;
    
    [Header("Despawn")]
    [Tooltip("마지막 웨이포인트 도착 후 사라짐")]
    public bool destroyAtEnd = true;
    
    public Action onDestroyed;
    
    int currentWaypointIndex = 0;

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
            Debug.LogWarning("[VehiclePlatform] 웨이포인트 미설정");
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypointIndex >= waypoints.Length) return;
        
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;
        
        // 목표 방향으로 회전
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        
        // 이동
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
        
        // 웨이포인트 도착 체크
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;
            
            // 마지막 웨이포인트 도착
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (destroyAtEnd)
                {
                    onDestroyed?.Invoke();
                    Destroy(gameObject);
                }
                else
                {
                    currentWaypointIndex = 0; // 반복
                }
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

