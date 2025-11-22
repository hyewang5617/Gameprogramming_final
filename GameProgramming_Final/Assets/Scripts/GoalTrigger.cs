using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.LevelComplete();
        }
    }
}

