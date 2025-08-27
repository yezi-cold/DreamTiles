using UnityEngine;

// [CreateAssetMenu] 让我们可以在 Assets/Create 菜单中创建这种类型的资源文件。
[CreateAssetMenu(fileName = "NewScoringRules", menuName = "Scoring/ScoringRules")]
public class ScoringRules : ScriptableObject
{
    [Header("规则匹配条件")]
    [Tooltip("第一个边缘的类型")]
    public EdgeType typeA;

    [Tooltip("第二个边缘的类型")]
    public EdgeType typeB;

    [Header("分数变化数值")]
    [Tooltip("当两种类型的边缘相邻时，产生分数的变化")]
    public ScoreModifier scoresForA;
}