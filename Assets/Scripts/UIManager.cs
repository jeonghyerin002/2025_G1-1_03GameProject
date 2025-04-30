using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject HelpPanel;

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
}
