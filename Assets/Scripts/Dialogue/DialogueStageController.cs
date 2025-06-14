using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueStageController : MonoBehaviour
{
    [Header("UI ���")]
    public Button continueButton;
    public GameObject actionPanel;

    [Header("��ȭ ������")]
    public DialogueDataSO[] character1Dialogues;
    public DialogueDataSO[] character2Dialogues;
    public DialogueDataSO[] character3Dialogues;
    public DialogueDataSO[] character4Dialogues;
    public DialogueDataSO[] character5Dialogues;
    public DialogueDataSO[] character6Dialogues;
    public DialogueDataSO[] character7Dialogues;
    public DialogueDataSO[] character8Dialogues;

    private DialogueManager dialogueManager;
    private int dialogueStep = 0; // 0: ĳ����1, 1: ĳ����2
    private bool allDialoguesFinished = false;

    void Start()
    {        
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null && actionPanel != null)
            dialogueManager.actionPanel = actionPanel;

        if (continueButton != null)
            continueButton.onClick.AddListener(Continue);

        if (actionPanel != null)
            actionPanel.SetActive(false);

        // ���� �������� �� �α�
        int currentStage = 1;
        if (StageManager.Instance != null)
        {
            currentStage = StageManager.Instance.currentStage;          
        }
        else
        {
            Debug.LogError("StageManager.Instance�� null!");
        }

        dialogueStep = 0;
     
        PlayDialogue(currentStage, dialogueStep);
    }

    void PlayDialogue(int stage, int step)
    {
        Debug.Log("���̾�α� ����"+ dialogueStep);
        DialogueDataSO dialogue = null;

        if (step == 0 && character1Dialogues != null && stage <= character1Dialogues.Length)
        {
            dialogue = character1Dialogues[stage - 1];
          
        }
        else if (step == 1 && character2Dialogues != null && stage <= character2Dialogues.Length)
        {
            dialogue = character2Dialogues[stage - 1];           
        }
       
        else
        {
            Debug.LogWarning($"��ȭ�� ã�� �� ����! Stage: {stage}, Step: {step}");
        }

        if (dialogue != null && dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogue);
        }
        else
        {
            Debug.Log(1);
            ShowContinueButton();
        }
    }

    public void OnDialogueFinished()
    {
        Debug.Log($"��ȭ ����! ���� step: {dialogueStep}");

        if (dialogueStep == 0)
        {
            // ù ��° ��ȭ ���� �� �� ��° ��ȭ ����
            dialogueStep = 1;
            int currentStage = StageManager.Instance != null ? StageManager.Instance.currentStage : 1;
            PlayDialogue(currentStage, dialogueStep);
        }
        else
        {
            // ��� ��ȭ ���� �� ��ư ǥ��
            allDialoguesFinished = true;
            ShowContinueButton();
        }
    }

    void ShowContinueButton()
    {
        if (dialogueManager != null)
        {
            dialogueManager.DialoguePanel.SetActive(false);
            if (actionPanel != null)
                actionPanel.SetActive(true);
        }
    }

    public void Continue()
    {
        
        if (StageManager.Instance != null)
        {
            if (StageManager.Instance.isGameCleared)
            {
              
                // �������� ���� �� �α�
                
                StageManager.Instance.currentStage++;               
                StageManager.Instance.isGameCleared = false;              
                StageManager.Instance.LoadDialogueScene();
            }
            else
            {               
                StageManager.Instance.LoadGameScene();
            }
        }
        else
        {
            Debug.LogError("StageManager.Instance�� null�Դϴ�!");
        }
    }
}