using UnityEngine;
using System;

public class VehiclePlatform : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f; // 트럭 이동 속도
    
    [Header("Despawn")]
    public float despawnDistance = 50f; // 사라지는 거리
    
    public Action onDestroyed;
    
    Vector3 startPos;
    float traveled = 0f;

    void Start()
    {
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        float move = speed * Time.fixedDeltaTime;
        
        // 계속 앞으로만 이동 (플레이어는 자식이 되어 자동으로 따라감)
        transform.position += transform.forward * move;
        
        traveled += move;
        
        // 일정 거리 이동하면 사라짐
        if (traveled >= despawnDistance)
        {
            onDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}

