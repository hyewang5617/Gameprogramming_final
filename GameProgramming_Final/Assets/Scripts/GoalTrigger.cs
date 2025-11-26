using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[GoalTrigger] 충돌 감지: {other.name} (Tag: {other.tag})");
        
        // 플레이어가 Goal에 닿았을 때
        if (other.CompareTag("Player"))
        {
            Debug.Log("[GoalTrigger] 플레이어가 Goal에 도착!");
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                Debug.Log("[GoalTrigger] GameManager.LevelComplete() 호출");
                gm.LevelComplete();
            }
            else
            {
                Debug.LogWarning("[GoalTrigger] GameManager를 찾을 수 없습니다!");
            }
        }
    }
}

