using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // 单例模式

    private int currentScore = 0;
    public int CurrentScore => currentScore; // 对外公开只读分数

    [Header("Scoring Settings")]
    [SerializeField] private int basePlacementScore = 10;
    [SerializeField] private int perfectMatchBonus = 5; // 每完美匹配一条边额外得分

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 确保只有一个实例
        }
        else
        {
            Instance = this;
        }
    }

    // 添加分数
    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"Score: {currentScore}");
        // 未来在这里更新UI
    }

    // 计算并添加地块放置得分
    public void ScoreTilePlacement(TileData placedTileData, int matchedEdgesCount)
    {
        int scoreToAdd = basePlacementScore; // 基础放置分

        // 完美匹配边的额外加分
        scoreToAdd += matchedEdgesCount * perfectMatchBonus;

        // 这里还可以根据 TileData 中的其他属性添加分数，例如：
        // if (placedTileData.isSpecialTile) { scoreToAdd += 50; }

        AddScore(scoreToAdd);
    }

    // 未来可以有重置分数的方法 (用于新游戏)
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
    }
}