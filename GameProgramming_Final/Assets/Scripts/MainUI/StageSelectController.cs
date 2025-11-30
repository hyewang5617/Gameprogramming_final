using UnityEngine;

public class StageSelectController : BaseStateController
{
    [SerializeField] private Transform stageListRoot;
    [SerializeField] private GameObject[] stageButtons;

    // 필요한 초기화나 추가 동작이 있으면 Awake/Start에 작성
    protected override void Reset()
    {
        base.Reset();
        // default fadeDuration 등 초기값 설정 가능
    }

    // 필요시 진입 시 추가 작업을 수행하고 싶다면 아래처럼 오버라이드
    public override void Enter(object param = null)
    {
        // 예: param으로 특정 스테이지 선택 정보가 들어올 수 있음
        // param 처리 코드는 여기에

        // 공통 동작(페이드 + moving UI 적용)
        base.Enter(param);

        // 추가로 StageSelect 전용 초기화가 필요하면 여기에 추가
        // 예: stage buttons refresh
        RefreshStageButtons();
    }

    public override void Exit()
    {
        // StageSelect 전용 종료 처리(있다면)
        // ...

        // 공통 종료 동작 수행 (페이드아웃 + 이동)
        base.Exit();
    }

    private void RefreshStageButtons()
    {
        if (stageListRoot == null || stageButtons == null) return;
        // 버튼 생성/정렬/활성화 로직 등
        // 간단 예: stageButtons를 stageListRoot의 자식으로 배치
        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (stageButtons[i] == null) continue;
            stageButtons[i].transform.SetParent(stageListRoot, false);
            // 추가 초기화...
        }
    }
}
