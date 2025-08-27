using UnityEngine;
using System;
using System.Collections.Generic; // ����List�����ڴ洢�����б�

/* ����������
   ���幦�ܣ�ʹ�õ���ģʽ��������Ϸ�е����з��������ٶȡ��˿ڡ��Ҹ��ȣ���
   ���ṩһ����ѯ�ӿ�����ȡ�Ʒֹ���*/
public class ScoreManager : MonoBehaviour
{
    // --- ���� ---
    public static ScoreManager Instance { get; private set; }

    // --- �¼� ---
    public static event Action OnScoreUpdated;

    // --- ���� ---
    public int ProsperityScore { get; private set; } = 0;
    public int PopulationScore { get; private set; } = 0;
    public int HappinessScore { get; private set; } = 0;

    // ������ ��������һ���б�������������еļƷֹ�����Դ ������
    // ����Ҫ������Ŀ�д��������мƷֹ�����Դ�ļ�����Project�����ϵ�������б��
    [Header("�Ʒֹ�������")]
    [SerializeField]
    private List<ScoringRules> ScoringRules;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // ������ ��������һ���������������ڲ�ѯ�������� ������
    // ��������������ֵؿ����͵���ϣ���������Ӧ�ķ����仯��
    public ScoreModifier GetAdjacencyBonus(EdgeType typeA, EdgeType typeB)
    {
        foreach (var rule in ScoringRules)
        {
            // �������Ƿ�ƥ�䣨�������
            if ((rule.typeA == typeA && rule.typeB == typeB) ||
                (rule.typeA == typeB && rule.typeB == typeA))
            {
                // ���ع����ж���ķ���ֵ
                return rule.scoresForA;
            }
        }
        // ���û���ҵ�ƥ��Ĺ��򣬷���һ������ֵ��Ϊ0��ʵ������ʾ���ӷ�Ҳ���۷֡�
        return new ScoreModifier();
    }

    public void AddScores(int prosperity, int population, int happiness)
    {
        ProsperityScore += prosperity;
        PopulationScore += population;
        HappinessScore += happiness;

        // ������κη����仯���Ŵ�ӡ��־�ʹ����¼�
        if (prosperity != 0 || population != 0 || happiness != 0)
        {
            Debug.Log($"�����仯: ���ٶ� +{prosperity}, �˿� +{population}, �Ҹ��� +{happiness}");
            Debug.Log($"��ǰ�ܷ�: ���ٶ�={ProsperityScore}, �˿�={PopulationScore}, �Ҹ���={HappinessScore}");
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