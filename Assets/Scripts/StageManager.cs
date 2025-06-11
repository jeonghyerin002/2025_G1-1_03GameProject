using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("���� �������� ����")]
    public int currentStage = 1;
    public bool isGameCleared = false;

    [Header("�������� ������")]
    public StageDataSO[] stageDataArray; // ���������� ������

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

    // ���� �������� ������ ��������
    public StageDataSO GetCurrentStageData()
    {
        if (currentStage - 1 < stageDataArray.Length)
        {
            return stageDataArray[currentStage - 1];
        }
        return stageDataArray[0]; // �⺻��
    }

    // ���� ������ �̵�
    public void LoadGameScene()
    {
        isGameCleared = false; 
        SceneManager.LoadScene("Level_1");
    }

    // ��� ������ �̵�
    public void LoadDialogueScene()
    {
        SceneManager.LoadScene("Dialog_0");
    }

    // ���� ���������� �̵�
    public void NextStage()
    {
        currentStage++; // �������� ����
        Debug.Log($"���� ����������! ���� ��������: {currentStage}");
        isGameCleared = false; // �� �������� �����̹Ƿ� Ŭ���� ���� �ʱ�ȭ

        // ��ȭ���� �ٽ� �ε��ؼ� ���ο� �������� ��ȭ�� ��������
        SceneManager.LoadScene("Dialog_0");
    }
    // �������� Ŭ���� Ȯ��
    public void CheckStageClear(int score)
    {
        StageDataSO currentData = GetCurrentStageData();
        if (score >= currentData.targetScore)
        {
            isGameCleared = true;
            Debug.Log("�������� Ŭ����!");
        }
    }
}