using UnityEngine;
using TMPro; // 引入 TextMeshPro 库

/* UI管理器
   整体功能：负责更新游戏中的所有UI元素，如此处的计分板。*/
public class UIManager : MonoBehaviour
{
    // --- 字段 ---
    [Header("分数显示UI")]
    [SerializeField] private TextMeshProUGUI prosperityText;
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI happinessText;

    // --- Unity生命周期方法 ---

    // OnEnable 在对象被激活时调用
    private void OnEnable()
    {
        // 订阅ScoreManager的分数更新事件。
        // 当事件触发时，调用我们本地的 UpdateScoreDisplay 方法。
        ScoreManager.OnScoreUpdated += UpdateScoreDisplay;
    }

    // OnDisable 在对象被禁用或销毁时调用
    private void OnDisable()
    {
        // 取消订阅，这是一个好习惯，可以防止内存泄漏。
        ScoreManager.OnScoreUpdated -= UpdateScoreDisplay;
    }

    private void Start()
    {
        // 游戏开始时，立即更新一次UI，以显示初始分数(0)。
        UpdateScoreDisplay();
    }

    // --- 私有方法 ---

    // 更新分数显示
    private void UpdateScoreDisplay()
    {
        if (ScoreManager.Instance == null) return;

        // 检查UI元素是否存在，然后更新它们的文本内容。
        if (prosperityText != null)
            prosperityText.text = $"繁荣度: {ScoreManager.Instance.ProsperityScore}";

        if (populationText != null)
            populationText.text = $"人口: {ScoreManager.Instance.PopulationScore}";

        if (happinessText != null)
            happinessText.text = $"幸福度: {ScoreManager.Instance.HappinessScore}";
    }
}