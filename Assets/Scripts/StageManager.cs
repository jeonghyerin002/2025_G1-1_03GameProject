using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("현재 스테이지 정보")]
    public int currentStage = 1;
    public bool isGameCleared = false;

    [Header("스테이지 데이터")]
    public StageDataSO[] stageDataArray; // 스테이지별 데이터

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 현재 스테이지 데이터 가져오기
    public StageDataSO GetCurrentStageData()
    {
        if (currentStage - 1 < stageDataArray.Length)
        {
            return stageDataArray[currentStage - 1];
        }
        return stageDataArray[0]; // 기본값
    }

    // 게임 씬으로 이동
    public void LoadGameScene()
    {
        isGameCleared = false; 
        SceneManager.LoadScene("Level_1");
    }

    // 대사 씬으로 이동
    public void LoadDialogueScene()
    {
        SceneManager.LoadScene("Dialog_0");
    }

    // 다음 스테이지로 이동
    public void NextStage()
    {
        currentStage++; // 스테이지 증가
        Debug.Log($"다음 스테이지로! 현재 스테이지: {currentStage}");
        isGameCleared = false; // 새 스테이지 시작이므로 클리어 상태 초기화

        // 대화씬을 다시 로드해서 새로운 스테이지 대화가 나오도록
        SceneManager.LoadScene("Dialog_0");
    }
    // 스테이지 클리어 확인
    public void CheckStageClear(int score)
    {
        StageDataSO currentData = GetCurrentStageData();
        if (score >= currentData.targetScore)
        {
            isGameCleared = true;
            Debug.Log("스테이지 클리어!");
        }
    }
}