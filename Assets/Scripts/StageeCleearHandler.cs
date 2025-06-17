using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageeCleearHandler : MonoBehaviour
{

    public int endingStage = 5;
    public string endingSceneName = "Ending";
    public float delayBeforeeEnding = 2f;

    private bool hasHandIed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       if(StageManager.Instance.isGameCleared && StageManager.Instance.currentStage >= endingStage)
        {
            Debug.Log("모든 스테이지 클리어! 엔딩으로 이동합니다.");
            SceneManager.LoadScene("Ending");
        }
    }
}
