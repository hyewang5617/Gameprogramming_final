using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel; // 반투명 검은색 배경 패널
    public Text tutorialText; // 튜토리얼 텍스트

    [Header("Tutorial Settings")]
    public float displayDuration = 2f; // 각 튜토리얼 표시 시간
    public string tutorialSceneName = "Stage1"; // 튜토리얼이 발동될 씬 이름

    string[] tutorialMessages = new string[]
    {
        "WASD TO MOVE",
        "SPACE TO JUMP",
        "MOUSE TO LOOK AROUND",
        "SHIFT TO SPRINT"
    };

    int currentIndex = 0;
    bool isTutorialActive = false;

    void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (tutorialText != null)
            tutorialText.text = "";
    }

    // 게임 시작 시 튜토리얼 시작 (stage1 씬에서만)
    public void StartTutorial()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        string currentSceneLower = currentSceneName.ToLower().Trim();
        string tutorialSceneLower = tutorialSceneName.ToLower().Trim();
        
        Debug.Log($"[TutorialManager] Current Scene: '{currentSceneName}', Required Scene: '{tutorialSceneName}'");
        
        // 씬 이름이 비어있거나 stage1이 아니면 튜토리얼 실행 안 함
        if (string.IsNullOrEmpty(tutorialSceneLower) || !currentSceneLower.Contains(tutorialSceneLower))
        {
            Debug.Log($"[TutorialManager] 튜토리얼 실행 안 함 - 씬 이름 불일치");
            return;
        }
        
        if (isTutorialActive) return;
        
        Debug.Log("[TutorialManager] 튜토리얼 시작");
        isTutorialActive = true;
        StartCoroutine(ShowTutorialSequence());
    }

    // 튜토리얼 순차 표시 코루틴
    IEnumerator ShowTutorialSequence()
    {
        if (tutorialPanel == null || tutorialText == null)
        {
            Debug.LogWarning("[TutorialManager] tutorialPanel 또는 tutorialText가 할당되지 않았습니다!");
            isTutorialActive = false;
            yield break;
        }

        tutorialPanel.SetActive(true);
        tutorialText.gameObject.SetActive(true); // Text GameObject 활성화

        for (int i = 0; i < tutorialMessages.Length; i++)
        {
            currentIndex = i;
            tutorialText.text = tutorialMessages[i];
            tutorialText.enabled = true; // Text 컴포넌트 활성화
            
            Debug.Log($"[TutorialManager] 튜토리얼 메시지 표시: {tutorialMessages[i]}");
            
            yield return new WaitForSeconds(displayDuration);
            
            tutorialText.text = "";
            
            if (i < tutorialMessages.Length - 1)
                yield return new WaitForSeconds(0.3f);
        }

        tutorialText.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);
        isTutorialActive = false;
    }
}

