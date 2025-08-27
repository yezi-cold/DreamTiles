using UnityEngine;
using TMPro; // ���� TextMeshPro ��

/* UI������
   ���幦�ܣ����������Ϸ�е�����UIԪ�أ���˴��ļƷְ塣*/
public class UIManager : MonoBehaviour
{
    // --- �ֶ� ---
    [Header("������ʾUI")]
    [SerializeField] private TextMeshProUGUI prosperityText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI happinessText;

    // --- Unity�������ڷ��� ---

    // OnEnable �ڶ��󱻼���ʱ����
    private void OnEnable()
    {
        // ����ScoreManager�ķ��������¼���
        // ���¼�����ʱ���������Ǳ��ص� UpdateScoreDisplay ������
        ScoreManager.OnScoreUpdated += UpdateScoreDisplay;
    }

    // OnDisable �ڶ��󱻽��û�����ʱ����
    private void OnDisable()
    {
        // ȡ�����ģ�����һ����ϰ�ߣ����Է�ֹ�ڴ�й©��
        ScoreManager.OnScoreUpdated -= UpdateScoreDisplay;
    }

    private void Start()
    {
        // ��Ϸ��ʼʱ����������һ��UI������ʾ��ʼ����(0)��
        UpdateScoreDisplay();
    }

    // --- ˽�з��� ---

    // ���·�����ʾ
    private void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance == null) return;

        // ���UIԪ���Ƿ���ڣ�Ȼ��������ǵ��ı����ݡ�
        if (prosperityText != null)
            prosperityText.text = $"���ٶ�: {ScoreManager.Instance.ProsperityScore}";

        if (populationText != null)
            populationText.text = $"�˿�: {ScoreManager.Instance.PopulationScore}";

        if (happinessText != null)
            happinessText.text = $"�Ҹ���: {ScoreManager.Instance.HappinessScore}";
    }
}