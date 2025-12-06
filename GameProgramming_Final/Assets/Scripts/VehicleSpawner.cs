using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] vehiclePrefabs;
    public float spawnInterval = 3f;
    public int maxVehicles = 10;
    
    [Header("Waypoint Path")]
    public Transform waypointPath;
    
    [Header("Spawn Area")]
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
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return;

        GameObject selectedPrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        if (selectedPrefab == null) return;

        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        Vector3 spawnPos = transform.position + randomOffset;
        GameObject vehicle = Instantiate(selectedPrefab, spawnPos, transform.rotation);
        currentVehicles++;
        
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

