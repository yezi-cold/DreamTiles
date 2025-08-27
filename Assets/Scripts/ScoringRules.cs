using UnityEngine;

// [CreateAssetMenu] �����ǿ����� Assets/Create �˵��д����������͵���Դ�ļ���
[CreateAssetMenu(fileName = "NewScoringRules", menuName = "Scoring/ScoringRules")]
public class ScoringRules : ScriptableObject
{
    [Header("����ƥ������")]
    [Tooltip("��һ����Ե������")]
    public EdgeType typeA;

    [Tooltip("�ڶ�����Ե������")]
    public EdgeType typeB;

    [Header("�����仯��ֵ")]
    [Tooltip("���������͵ı�Ե����ʱ�����������ı仯")]
    public ScoreModifier scoresForA;
}