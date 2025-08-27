using UnityEngine;
using System;
using System.Collections.Generic; // 引入List，用于存储规则列表

/* 分数管理器
   整体功能：使用单例模式，管理游戏中的所有分数（繁荣度、人口、幸福度），
   并提供一个查询接口来获取计分规则。*/
public class ScoreManager : MonoBehaviour
{
    // --- 单例 ---
    public static ScoreManager Instance { get; private set; }

    // --- 事件 ---
    public static event Action OnScoreUpdated;

    // --- 属性 ---
    public int ProsperityScore { get; private set; } = 0;
    public int PopulationScore { get; private set; } = 0;
    public int HappinessScore { get; private set; } = 0;

    // ▼▼▼ 【新增】一个列表来存放我们所有的计分规则资源 ▼▼▼
    // 你需要把在项目中创建的所有计分规则资源文件，从Project窗口拖到这里的列表里。
    [Header("计分规则配置")]
    [SerializeField]
    private List<ScoringRules> ScoringRules;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // ▼▼▼ 【新增】一个公共方法，用于查询分数规则 ▼▼▼
    // 这个方法会检查两种地块类型的组合，并返回相应的分数变化。
    public ScoreModifier GetAdjacencyBonus(EdgeType typeA, EdgeType typeB)
    {
        foreach (var rule in ScoringRules)
        {
            // 检查规则是否匹配（正向或反向）
            if ((rule.typeA == typeA && rule.typeB == typeB) ||
                (rule.typeA == typeB && rule.typeB == typeA))
            {
                // 返回规则中定义的分数值
                return rule.scoresForA;
            }
        }
        // 如果没有找到匹配的规则，返回一个所有值都为0的实例，表示不加分也不扣分。
        return new ScoreModifier();
    }

    public void AddScores(int prosperity, int population, int happiness)
    {
        ProsperityScore += prosperity;
        PopulationScore += population;
        HappinessScore += happiness;

        // 如果有任何分数变化，才打印日志和触发事件
        if (prosperity != 0 || population != 0 || happiness != 0)
        {
            Debug.Log($"分数变化: 繁荣度 +{prosperity}, 人口 +{population}, 幸福度 +{happiness}");
            Debug.Log($"当前总分: 繁荣度={ProsperityScore}, 人口={PopulationScore}, 幸福度={HappinessScore}");
            OnScoreUpdated?.Invoke();
        }
    }

    public void ResetScores()
    {
        ProsperityScore = 0;
        PopulationScore = 0;
        HappinessScore = 0;
        OnScoreUpdated?.Invoke();
    }
}