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


    public GameObject menuPanel;
    public Button mainMenuButton;
   


    private bool isMenuOpen = false;

    void Start()
    {

        if (SceneManager.GetActiveScene().name != "Level_0")
        {
            gameObject.SetActive(false);
            return;
        }

        ShowPage(currentPage);

        nextButton.onClick.AddListener(NextPage);
        prevButton.onClick.AddListener(PrevPage);

        
        
    }

    private void Update()
    {
       
    }

    void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);
    }

    void CloseMenu()
    {
        isMenuOpen = false;
        menuPanel.SetActive(false);
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Level_0");
    }

    void ShowPage(int index)
    {

        if (helpPages == null || helpPages.Length == 0 || prevButton == null || nextButton == null)
        {
            Debug.LogWarning("UIManager: UI references are not set. Skipping ShowPage.");
            return;
        }

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

    public void GameExit()
    {
        Application.Quit();
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
