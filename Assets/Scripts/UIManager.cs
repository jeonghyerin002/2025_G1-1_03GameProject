using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject HelpPanel;
    public TextMeshProUGUI scoreText;
    int score = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // 중복 방지
    }


    public void ShowScore(int amount)
    {
       
        scoreText.text = "Score: " + amount;
    }

  

    // Start is called before the first frame update
    public void GameStartButtonAction()
    {
        SceneManager.LoadScene("CardScene");

    }

    public void OpenOptionsPanel()
    {
        HelpPanel.SetActive(true);
    }

    public void CloseHelpPanel()
    {
        HelpPanel.SetActive(false);
    }

    public void OnClickStar()
    {
        SceneManager.LoadScene("DialogueScene");

    }

    public void OnClickGoToTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
