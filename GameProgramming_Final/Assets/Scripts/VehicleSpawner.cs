using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("생성할 트럭 프리팹")]
    public GameObject vehiclePrefab;
    public float spawnInterval = 3f; // 생성 주기 (초)
    public int maxVehicles = 10; // 최대 트럭 수
    
    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 5f); // 생성 영역 크기
    
    float timer = 0f;
    int currentVehicles = 0;

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
        if (vehiclePrefab == null)
        {
            Debug.LogWarning("[VehicleSpawner] vehiclePrefab 없음");
            return;
        }

        // 랜덤 위치 계산
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        Vector3 spawnPos = transform.position + randomOffset;
        
        // 트럭 생성
        GameObject vehicle = Instantiate(vehiclePrefab, spawnPos, transform.rotation);
        currentVehicles++;
        
        // 트럭이 사라질 때 카운트 감소
        VehiclePlatform platform = vehicle.GetComponent<VehiclePlatform>();
        if (platform != null)
        {
            platform.onDestroyed += () => currentVehicles--;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 에디터에서 스폰 영역 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}

