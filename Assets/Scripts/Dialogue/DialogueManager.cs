using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 요소 - Inspector에서 연결")]
    public GameObject DialoguePanel;
    public Image characterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("기본 설정")]
    public Sprite defaultCharacterImage;

    [Header("타이핑 효과 설정")]
    public float typingSpeed = 0.05f;
    public bool skipTypingOnClick = true;

    [Header("완료 후 UI")]
    public GameObject actionPanel; // 버튼들이 있는 패널

    private DialogueDataSO currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        DialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(HandleNextInput);

        // 처음엔 액션 패널 숨기기
        if (actionPanel != null)
            actionPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            HandleNextInput();
        }
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < textToType.Length; i++)
        {
            dialogueText.text += textToType[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        isTyping = false;

        if (currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Count)
        {
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
        }
    }

    void ShowCurrentLine()
    {
        if (currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Count)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
        }

        string currentText = currentDialogue.dialogueLines[currentLineIndex];
        typingCoroutine = StartCoroutine(TypeText(currentText));
    }

    void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isDialogueActive = false;
        isTyping = false;

        // DialogueStageController에게 대사 끝났다고 알림
        DialogueStageController controller = FindObjectOfType<DialogueStageController>();
        if (controller != null)
        {
            controller.OnDialogueFinished();
        }
        else
        {
            // 기존 방식 (모든 대사가 끝남)
            DialoguePanel.SetActive(false);

            if (actionPanel != null)
            {
                actionPanel.SetActive(true);
            }
        }

        currentLineIndex = 0;
    }

    public void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.dialogueLines.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    public void HandleNextInput()
    {
        if (isTyping && skipTypingOnClick)
        {
            CompleteTyping();
        }
        else if (!isTyping)
        {
            ShowNextLine();
        }
    }

    public void SkipDialogue()
    {
        EndDialogue();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void StartDialogue(DialogueDataSO dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        // 액션 패널 숨기고 대사창 켜기
        if (actionPanel != null)
            actionPanel.SetActive(false);

        DialoguePanel.SetActive(true);
        characterNameText.text = dialogue.characterName;

        if (characterImage != null)
        {
            if (dialogue.characterImage != null)
            {
                characterImage.sprite = dialogue.characterImage;
            }
            else
            {
                characterImage.sprite = defaultCharacterImage;
            }
        }

        ShowCurrentLine();
    }
}