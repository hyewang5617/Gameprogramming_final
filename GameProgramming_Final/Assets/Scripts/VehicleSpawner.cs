using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("생성할 차량 프리팹")]
    public GameObject vehiclePrefab;
    [Tooltip("차량 생성 주기 (초)")]
    public float spawnInterval = 3f;
    [Tooltip("동시에 존재 가능한 최대 차량 수")]
    public int maxVehicles = 10;
    
    [Header("Waypoint Path")]
    [Tooltip("차량이 따라갈 웨이포인트 경로")]
    public Transform waypointPath;
    
    [Header("Spawn Area")]
    [Tooltip("차량 생성 영역 크기")]
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 5f);
    
    float timer = 0f;
    int currentVehicles = 0;
    Transform[] waypoints;

    void Awake()
    {
        if (waypointPath != null)
        {
            int count = waypointPath.childCount;
            waypoints = new Transform[count];
            
            for (int i = 0; i < count; i++)
                waypoints[i] = waypointPath.GetChild(i);
        }
        else
        {
            Debug.LogWarning("[VehicleSpawner] waypointPath 미설정");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= spawnInterval && currentVehicles < maxVehicles)
        {
            SpawnVehicle();
            timer = 0f;
        }
    }

    void SpawnVehicle()
    {
        if (vehiclePrefab == null) return;

        // 랜덤 위치 계산
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        Vector3 spawnPos = transform.position + randomOffset;
        
        // 차량 생성
        GameObject vehicle = Instantiate(vehiclePrefab, spawnPos, transform.rotation);
        currentVehicles++;
        
        // 웨이포인트 할당
        VehiclePlatform platform = vehicle.GetComponent<VehiclePlatform>();
        if (platform != null)
        {
            platform.waypoints = waypoints;
            platform.onDestroyed += () => currentVehicles--;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}

