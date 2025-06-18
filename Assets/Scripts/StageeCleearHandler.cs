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
            Debug.Log("마지막 스테이지 클리어 감지! 엔딩으로 이동 준비 중...");
            StartCoroutine(LoadEndingAfterDelay());
        }
    }

    private IEnumerator LoadEndingAfterDelay()
    {
        Debug.Log("코루틴 시작됨, 대기 중...");
        yield return new WaitForSeconds(delayBeforeEnding);
        Debug.Log("씬 이동 시도: Ending");
        SceneManager.LoadScene(endingSceneName);
    }
}
