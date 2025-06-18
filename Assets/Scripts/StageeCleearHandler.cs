using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageeCleearHandler : MonoBehaviour
{

    public int endingStage = 5;
    public string endingSceneName = "Ending";
    public float delayBeforeEnding = 2f;

    private bool hasHandled = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hasHandled) return;
        if (SceneManager.GetActiveScene().name != "Level_1") return;

        if (StageManager.Instance != null &&
            StageManager.Instance.isGameCleared &&
            StageManager.Instance.currentStage >= endingStage)
        {
            hasHandled = true;
            Debug.Log("������ �������� Ŭ���� ����! �������� �̵� �غ� ��...");
            StartCoroutine(LoadEndingAfterDelay());
        }
    }

    private IEnumerator LoadEndingAfterDelay()
    {
        Debug.Log("�ڷ�ƾ ���۵�, ��� ��...");
        yield return new WaitForSeconds(delayBeforeEnding);
        Debug.Log("�� �̵� �õ�: Ending");
        SceneManager.LoadScene(endingSceneName);
    }
}
