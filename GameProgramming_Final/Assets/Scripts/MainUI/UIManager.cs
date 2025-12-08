using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum UIState
    {
        Main,
        StageSelect,
        Shop
    }

    [Header("Panels (assign in Inspector)")]
    public GameObject mainPanel;
    public GameObject stageSelectPanel;
    public GameObject shopPanel;

    private BaseStateController currentState;
    private GameObject currentPanel;

    // 전환 중인지 체크 (동시 전환 불가)
    private Coroutine transitionCoroutine = null;

    private void Awake()
    {
        Instance = this;

        // 초기 비활성화
        if (mainPanel != null) mainPanel.SetActive(false);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        ChangeToMain(0f);
    }

    private BaseStateController GetController(GameObject panel)
    {
        if (panel == null) return null;
        return panel.GetComponent<BaseStateController>();
    }

    private GameObject GetPanelForState(UIState state)
    {
        switch (state)
        {
            case UIState.Main: return mainPanel;
            case UIState.StageSelect: return stageSelectPanel;
            case UIState.Shop: return shopPanel;
            default: return null;
        }
    }

    /// <summary>
    /// 상태 전환 요청 (delaySeconds 이후 enter). 
    /// 이미 전환이 진행중이면 요청을 무시합니다.
    /// </summary>
    public void ChangeState(UIState toState, float delaySeconds = 0f, object param = null)
    {
        if (transitionCoroutine != null)
        {
            Debug.LogWarning("UIManager: transition already in progress. Ignoring ChangeState request.");
            return;
        }

        GameObject toPanel = GetPanelForState(toState);
        if (toPanel == null)
        {
            Debug.LogWarning($"UIManager.ChangeState: target panel for {toState} is not assigned.");
            return;
        }

        // 동일 패널이면 아무 동작하지 않음
        if (currentPanel == toPanel)
        {
            return;
        }

        transitionCoroutine = StartCoroutine(TransitionRoutine(toPanel, delaySeconds, param));
    }

    private IEnumerator TransitionRoutine(GameObject toPanel, float delaySeconds, object param)
    {
        // 호출 순서: 현재 Exit -> 대기(delay) -> 대상 Enter
        // 1) Exit current
        if (currentState != null)
        {
            currentState.Exit();
            // currentState.Exit()가 패널 비활성화를 담당한다고 가정
        }
        else if (currentPanel != null)
        {
            // 안전장치: controller가 없을 경우에도 패널 비활성화
            currentPanel.SetActive(false);
        }

        currentState = null;
        currentPanel = null;

        // 2) delay (realtime)
        if (delaySeconds > 0f)
            yield return new WaitForSecondsRealtime(delaySeconds);

        // 3) Enter new
        if (toPanel != null)
        {
            var controller = GetController(toPanel);
            if (controller != null)
            {
                currentState = controller;
                print("controller 인식 성공");
                currentPanel = toPanel;
                currentState.Enter(param);
            }
            else
            {
                // controller가 없으면 단순 활성화
                print("controller 인식 실패");
                toPanel.SetActive(true);
                currentState = null;
                currentPanel = toPanel;
            }
        }

        transitionCoroutine = null;
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    // 버튼용 편의 메서드 (delay optional)
    public void ChangeToMain(float delaySeconds = 0f) => ChangeState(UIState.Main, delaySeconds);
    public void ChangeToStageSelect(float delaySeconds = 0f) => ChangeState(UIState.StageSelect, delaySeconds);
    public void ChangeToShop(float delaySeconds = 0f) => ChangeState(UIState.Shop, delaySeconds);
}
