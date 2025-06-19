using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject helpPanel;
    public TextMeshProUGUI scoreText;
    int score = 0;

    public GameObject[] helpPages;
    public Button nextButton;
    public Button prevButton;

    private int currentPage = 0;


    private void Start()
    {
        ShowPage(currentPage);

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);
    }


    void ShowPage(int index)
    {
        for (int i = 0; i < helpPages.Length; i++)
        {
            helpPages[i].SetActive(i == index);
        }

        prevButton.interactable = index > 0;
        nextButton.interactable = index < helpPages.Length - 1;

    }

    void NextPage()
    {
        if (currentPage < helpPages.Length -1)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    public void OpenHelpPanel()
    {
        helpPanel.SetActive(true);
        currentPage = 0;
        ShowPage(currentPage);
        
    }

    public void CloseHelpPanel()
    {
        helpPanel.SetActive(false);
    }


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
        SceneManager.LoadScene("Dialog_0");

    }

    public void GoToMainMenu()
    {
        Debug.Log("메인 화면으로 이동!");
        SceneManager.LoadScene("Level_0"); 
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
