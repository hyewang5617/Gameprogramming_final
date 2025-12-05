using UnityEngine;
using System;

public class VehiclePlatform : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    
    [Header("Movement")]
    public float speed = 10f;
    public float waypointReachDistance = 5f;
    public float rotationSpeed = 5f;
    public float moveForce = 1000f;
    
    [Header("Despawn")]
    public bool destroyAtEnd = true;
    
    public Action onDestroyed;
    
    int currentWaypointIndex = 0;
    Rigidbody rigid;

    void Start()
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
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (currentWaypointIndex >= waypoints.Length) return;
        if (rigid == null) return;
        
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;
        
        Vector3 direction = (target.position - transform.position).normalized;
        
        if (direction.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        
        rigid.AddForce(direction * moveForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        
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
                {
                    currentWaypointIndex = 0;
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawWireSphere(waypoints[i].position, waypointReachDistance);
            }
        }
        
        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[waypoints.Length - 1].position, waypointReachDistance);
        }
    }
}

