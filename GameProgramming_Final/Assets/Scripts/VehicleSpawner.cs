using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] vehiclePrefabs;
    public float spawnInterval = 3f;
    public int maxVehicles = 10;
    public int initialSpawnCount = 5;
    public float initialSpawnInterval = 1.5f;
    public float initialSpawnSpeedMultiplier = 5f;
    
    [Header("Vehicle Variation")]
    public float speedVariation = 2f;
    
    [Header("Waypoint Path")]
    public Transform waypointPath;
    
    [Header("Player Reference")]
    public Transform player;
    
    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(20f, 0f, 5f);
    public float spawnCheckRadius = 5f;
    public int maxSpawnAttempts = 20;
    public bool autoCalculateVehicleSize = true;
    
    float timer = 0f; // 일반 스폰 타이머
    int currentVehicles = 0; // 현재 존재하는 차량 수
    int initialSpawned = 0; // 초기 스폰된 차량 수
    Transform[] waypoints; // 웨이포인트 배열
    bool initialSpawnComplete = false; // 초기 스폰 완료 여부
    bool playerVehicleSpawned = false; // 플레이어 위치 차량 스폰 완료 여부
    
    public bool IsReady => initialSpawnComplete && playerVehicleSpawned; // 모든 초기 스폰 완료
    public float InitialSpawnProgress => initialSpawnCount > 0 ? (float)initialSpawned / initialSpawnCount : 0f; // 초기 스폰 진행도

    void Awake()
    {
        if (waypointPath == null)
        {
            Debug.LogWarning("[VehicleSpawner] waypointPath 미설정");
            return;
        }
        
        int count = waypointPath.childCount;
        waypoints = new Transform[count];
        for (int i = 0; i < count; i++)
            waypoints[i] = waypointPath.GetChild(i);
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        if (initialSpawnCount > 0)
            StartCoroutine(InitialSpawnCoroutine());
        else
            initialSpawnComplete = true;
    }

    System.Collections.IEnumerator InitialSpawnCoroutine()
    {
        while (initialSpawned < initialSpawnCount)
        {
            bool isLastVehicle = (initialSpawned + 1 == initialSpawnCount);
            
            if (SpawnVehicle(isLastVehicle))
            {
                initialSpawned++;
                
                if (initialSpawned < initialSpawnCount)
                {
                    yield return new WaitForSeconds(initialSpawnInterval);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        initialSpawnComplete = true;
        
        yield return null;
        
        SpawnVehicleAtPlayerPosition();
        playerVehicleSpawned = true;
    }

    void Update()
    {
        if (!initialSpawnComplete)
        {
            return;
        }

        timer += Time.deltaTime;
        
        if (timer >= spawnInterval && currentVehicles < maxVehicles)
        {
            if (SpawnVehicle())
            {
                timer = 0f;
            }
        }
    }

    // 차량 스폰 (성공 여부 반환)
    bool SpawnVehicle(bool avoidPlayerPosition = false)
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return false;

        GameObject selectedPrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        if (selectedPrefab == null) return false;

        Vector3 spawnPos = FindValidSpawnPosition(avoidPlayerPosition);
        if (spawnPos == Vector3.zero) return false;
        
        GameObject vehicle = Instantiate(selectedPrefab, spawnPos, transform.rotation);
        currentVehicles++;
        
        Rigidbody vehicleRigid = vehicle.GetComponent<Rigidbody>();
        if (vehicleRigid != null)
        {
            vehicleRigid.velocity = Vector3.zero;
            vehicleRigid.angularVelocity = Vector3.zero;
        }
        
        VehiclePlatform platform = vehicle.GetComponent<VehiclePlatform>();
        if (platform != null)
        {
            platform.waypoints = waypoints;
            platform.onDestroyed += () => currentVehicles--;
            
            if (!initialSpawnComplete)
                platform.SetMoveForce(platform.moveForce * initialSpawnSpeedMultiplier);
            
            if (speedVariation > 0f)
            {
                float speedOffset = Random.Range(-speedVariation, speedVariation);
                platform.speed = Mathf.Max(platform.speed + speedOffset, 1f);
            }
            
            platform.InitializeMovement();
        }
        
        return true;
    }

    // 플레이어 위치에 차량 스폰

    void SpawnVehicleAtPlayerPosition()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0 || player == null) return;

        GameObject selectedPrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        if (selectedPrefab == null) return;

        Vector3 spawnPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        GameObject vehicle = Instantiate(selectedPrefab, spawnPos, transform.rotation);
        currentVehicles++;
        initialSpawned++;
        
        Rigidbody vehicleRigid = vehicle.GetComponent<Rigidbody>();
        if (vehicleRigid != null)
        {
            vehicleRigid.velocity = Vector3.zero;
            vehicleRigid.angularVelocity = Vector3.zero;
        }
        
        VehiclePlatform platform = vehicle.GetComponent<VehiclePlatform>();
        if (platform != null)
        {
            platform.waypoints = waypoints;
            platform.onDestroyed += () => currentVehicles--;
            
            if (speedVariation > 0f)
            {
                float speedOffset = Random.Range(-speedVariation, speedVariation);
                platform.speed = Mathf.Max(platform.speed + speedOffset, 1f);
            }
            
            platform.InitializeMovement();
        }
    }

    // 모든 차량의 속도를 원래대로 복원
    public void ResetAllVehicleSpeeds()
    {
        VehiclePlatform[] vehicles = FindObjectsOfType<VehiclePlatform>();
        foreach (VehiclePlatform vehicle in vehicles)
            vehicle.ResetMoveForce();
    }

    // 겹치지 않는 스폰 위치 찾기

    Vector3 FindValidSpawnPosition(bool avoidPlayerPosition = false) // 유효한 스폰 위치 반환, 없으면 Vector3.zero
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Length == 0) return Vector3.zero;
        
        GameObject samplePrefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        if (samplePrefab == null) return Vector3.zero;
        
        float checkRadius = spawnCheckRadius;
        if (autoCalculateVehicleSize)
        {
            float vehicleSize = GetVehicleSize(samplePrefab);
            if (vehicleSize > 0f)
            {
                checkRadius = vehicleSize * 1.2f;
            }
        }
        
        Vector3 playerAvoidPos = Vector3.zero;
        float playerAvoidDistance = spawnAreaSize.x;
        
        if (avoidPlayerPosition && player != null)
        {
            playerAvoidPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            
            bool goLeft = Random.Range(0f, 1f) < 0.5f;
            float sideOffset = playerAvoidDistance;
            
            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector3 offset = goLeft ? Vector3.left : Vector3.right;
                offset *= sideOffset;
                
                Vector3 candidatePos = playerAvoidPos + offset;
                
                Vector3 clampedPos = new Vector3(
                    Mathf.Clamp(candidatePos.x, transform.position.x - spawnAreaSize.x / 2, transform.position.x + spawnAreaSize.x / 2),
                    candidatePos.y,
                    Mathf.Clamp(candidatePos.z, transform.position.z - spawnAreaSize.z / 2, transform.position.z + spawnAreaSize.z / 2)
                );
                
                if (IsPositionClear(clampedPos, checkRadius))
                {
                    return clampedPos;
                }
                
                goLeft = !goLeft;
                sideOffset = playerAvoidDistance * 0.5f * (attempt % 2 + 1);
            }
        }
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            
            Vector3 candidatePos = transform.position + randomOffset;
            
            if (avoidPlayerPosition && player != null)
            {
                float distanceToPlayer = Vector3.Distance(new Vector3(candidatePos.x, 0, candidatePos.z), new Vector3(player.position.x, 0, player.position.z));
                if (distanceToPlayer < playerAvoidDistance)
                {
                    continue;
                }
            }
            
            if (IsPositionClear(candidatePos, checkRadius))
            {
                return candidatePos;
            }
        }
        
        for (int attempt = 0; attempt < maxSpawnAttempts * 2; attempt++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
            
            Vector3 candidatePos = transform.position + randomOffset;
            
            if (avoidPlayerPosition && player != null)
            {
                float distanceToPlayer = Vector3.Distance(new Vector3(candidatePos.x, 0, candidatePos.z), new Vector3(player.position.x, 0, player.position.z));
                if (distanceToPlayer < playerAvoidDistance * 0.7f)
                {
                    continue;
                }
            }
            
            if (IsPositionClear(candidatePos, checkRadius * 0.7f))
            {
                return candidatePos;
            }
        }
        
        return Vector3.zero;
    }

    // 차량 프리팹의 크기 계산 (Collider 또는 Renderer bounds)
    float GetVehicleSize(GameObject prefab)
    {
        Bounds bounds = new Bounds();
        bool boundsSet = false;
        
        Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (!boundsSet)
            {
                bounds = col.bounds;
                boundsSet = true;
            }
            else
                bounds.Encapsulate(col.bounds);
        }
        
        if (!boundsSet)
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                if (!boundsSet)
                {
                    bounds = rend.bounds;
                    boundsSet = true;
                }
                else
                    bounds.Encapsulate(rend.bounds);
            }
        }
        
        if (boundsSet)
            return Mathf.Max(bounds.size.x, bounds.size.z);
        
        return 0f;
    }

    // 해당 위치에 차량이 있는지 확인
    bool IsPositionClear(Vector3 position, float checkRadius)
    {
        Collider[] overlaps = Physics.OverlapSphere(position, checkRadius);
        
        foreach (Collider col in overlaps)
        {
            if (IsVehicleCollider(col))
            {
                return false;
            }
        }
        
        return true;
    }

    // Collider가 차량에 속하는지 확인
    bool IsVehicleCollider(Collider col)
    {
        if (col == null) return false;
        
        if (col.gameObject.CompareTag("Vehicle"))
            return true;
        
        if (col.GetComponentInParent<VehiclePlatform>() != null)
            return true;
        
        return false;
    }

    // 플레이어를 제외하고 위치 확인 (사용하지 않음)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}

