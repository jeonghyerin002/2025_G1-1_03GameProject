using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MergeInfoUI : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject infoPanel;                // 정보 패널
    public TextMeshProUGUI ruleNameText;        // 규칙 이름
    public TextMeshProUGUI cardCountText;       // 카드 개수
    public TextMeshProUGUI successRateText;     // 성공 확률
    public TextMeshProUGUI rewardText;          // 보상 정보
    public Image probabilityBar;                // 확률 바
    public TextMeshProUGUI statusText;          // 상태 메시지

    [Header("색상 설정")]
    public Color highChanceColor = Color.green;     // 높은 확률
    public Color mediumChanceColor = Color.yellow;  // 중간 확률
    public Color lowChanceColor = Color.red;        // 낮은 확률
    public Color noChanceColor = Color.gray;        // 불가능

    void Start()
    {
        // 시작시 숨김
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    // 합성 정보 업데이트
    public void UpdateMergeInfo(int cardCount, string ruleName, float successRate, int reward, bool canMerge)
    {
        if (infoPanel != null)
            infoPanel.SetActive(cardCount >= 2);

        if (cardCount < 2)
        {
            HideInfo();
            return;
        }

        // 카드 개수
        if (cardCountText != null)
            cardCountText.text = $"카드: {cardCount}장";

        // 규칙 이름
        if (ruleNameText != null)
            ruleNameText.text = canMerge ? ruleName : "합성 불가";

        // 성공 확률
        if (successRateText != null)
        {
            if (canMerge)
                successRateText.text = $"성공: {(successRate * 100):F0}%";
            else
                successRateText.text = "성공: 0%";
        }

        // 보상 정보
        if (rewardText != null)
        {
            if (canMerge)
                rewardText.text = $"보상: +{reward}점";
            else
                rewardText.text = "보상: 없음";
        }

        // 확률 바 업데이트
        UpdateProbabilityBar(successRate, canMerge);

        // 상태 메시지
        UpdateStatusMessage(successRate, canMerge);
    }

    void UpdateProbabilityBar(float rate, bool canMerge)
    {
        if (probabilityBar == null) return;

        probabilityBar.fillAmount = canMerge ? rate : 0f;

        // 색상 설정
        Color barColor;
        if (!canMerge)
            barColor = noChanceColor;
        else if (rate >= 0.7f)
            barColor = highChanceColor;
        else if (rate >= 0.4f)
            barColor = mediumChanceColor;
        else
            barColor = lowChanceColor;

        probabilityBar.color = barColor;
    }

    void UpdateStatusMessage(float rate, bool canMerge)
    {
        if (statusText == null) return;

        string message;
        Color textColor;

        if (!canMerge)
        {
            message = "합성 불가능";
            textColor = noChanceColor;
        }
        else
        {
            message = $"성공 확률: {(rate * 100):F0}%";

            if (rate >= 0.8f)
                textColor = highChanceColor;
            else if (rate >= 0.5f)
                textColor = mediumChanceColor;
            else
                textColor = lowChanceColor;
        }

        statusText.text = message;
        statusText.color = textColor;
    }

    public void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    // 빈 상태로 표시
    public void ShowEmptyState()
    {
        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (ruleNameText != null)
            ruleNameText.text = "카드를 선택하세요";

        if (cardCountText != null)
            cardCountText.text = "카드: 0장";

        if (successRateText != null)
            successRateText.text = "성공: -%";

        if (rewardText != null)
            rewardText.text = "보상: -";

        if (probabilityBar != null)
        {
            probabilityBar.fillAmount = 0f;
            probabilityBar.color = noChanceColor;
        }

        if (statusText != null)
        {
            statusText.text = "카드 2장 이상 선택 필요";
            statusText.color = noChanceColor;
        }
    }
}