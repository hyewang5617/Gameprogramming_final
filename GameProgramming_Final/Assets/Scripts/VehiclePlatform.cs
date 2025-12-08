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
    public float curveLookAhead = 0.3f;
    public float sharpTurnMultiplier = 2f;
    public float curveStartDistance = 2f;
    
    [Header("Despawn")]
    public bool destroyAtEnd = true;
    
    [Header("Explosion")]
    public GameObject explosionEffectPrefab;
    public float stalledTimeThreshold = 5f;
    
    public Action onDestroyed;
    
    int currentWaypointIndex = 0;
    Rigidbody rigid;
    bool initialized = false;
    float originalMoveForce;
    float stalledTimer = 0f;

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

    public void SetMoveForce(float force) => moveForce = force;
    public void ResetMoveForce() => moveForce = originalMoveForce;

    void Start()
    {
        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody>();
            Debug.LogWarning("[VehiclePlatform] Rigidbody가 없습니다!");
        }
        
        if (waypoints != null && waypoints.Length > 0)
            InitializeMovement();
    }

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
        
        Vector3 targetPosition = GetCurvedTargetPosition();
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        if (direction.magnitude > 0.01f)
        {
            rigid.AddForce(direction * moveForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
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
                    if (angle > 45f)
                        currentRotSpeed = rotationSpeed * sharpTurnMultiplier;
                }
            }
            
            rigid.rotation = Quaternion.Slerp(rigid.rotation, targetRotation, Time.fixedDeltaTime * currentRotSpeed);
        }
        
        Vector3 horizontalVelocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        
        if (currentSpeed > speed)
        {
            horizontalVelocity = horizontalVelocity.normalized * speed;
            rigid.velocity = new Vector3(horizontalVelocity.x, rigid.velocity.y, horizontalVelocity.z);
            currentSpeed = speed;
        }
        
        CheckExplosionConditions(currentSpeed);
        
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
    
    Vector3 GetCurvedTargetPosition()
    {
        if (currentWaypointIndex >= waypoints.Length) 
            return transform.position;
        
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (currentWaypoint == null) return transform.position;
        
        float distanceToCurrent = Vector3.Distance(transform.position, currentWaypoint.position);
        
        if (distanceToCurrent > curveStartDistance)
            return currentWaypoint.position;
        
        if (currentWaypointIndex + 1 < waypoints.Length)
        {
            Transform nextWaypoint = waypoints[currentWaypointIndex + 1];
            if (nextWaypoint != null)
            {
                Vector3 toWaypoint = (currentWaypoint.position - transform.position).normalized;
                Vector3 vehicleForward = transform.forward;
                float waypointBehind = Vector3.Dot(toWaypoint, vehicleForward);
                
                if (waypointBehind < 0.5f && distanceToCurrent < curveStartDistance)
                {
                    float totalDistance = Vector3.Distance(currentWaypoint.position, nextWaypoint.position);
                    
                    if (totalDistance > 0.01f)
                    {
                        float normalizedDistance = Mathf.Clamp01(distanceToCurrent / curveStartDistance);
                        float t = Mathf.Lerp(curveLookAhead, 0f, normalizedDistance);
                        Vector3 curvedTarget = Vector3.Lerp(currentWaypoint.position, nextWaypoint.position, t);
                        
                        Vector3 toCurvedTarget = (curvedTarget - transform.position).normalized;
                        float dot = Vector3.Dot(vehicleForward, toCurvedTarget);
                        
                        if (dot > 0.3f)
                            return curvedTarget;
                    }
                }
            }
        }
        
        return currentWaypoint.position;
    }
    
    void CheckExplosionConditions(float currentSpeed)
    {
        if (currentSpeed < 0.5f)
        {
            stalledTimer += Time.fixedDeltaTime;
            if (stalledTimer >= stalledTimeThreshold)
                Explode();
        }
        else
        {
            stalledTimer = 0f;
        }
    }
    
    void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystem.MainModule main = ps.main;
                Destroy(explosion, main.duration + main.startLifetime.constantMax);
            }
            else
            {
                Destroy(explosion, 3f);
            }
        }
        
        onDestroyed?.Invoke();
        Destroy(gameObject);
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

