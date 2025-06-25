using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // ����ģʽ

    private int currentScore = 0;
    public int CurrentScore => currentScore; // ���⹫��ֻ������

    [Header("Scoring Settings")]
    [SerializeField] private int basePlacementScore = 10;
    [SerializeField] private int perfectMatchBonus = 5; // ÿ����ƥ��һ���߶���÷�

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ȷ��ֻ��һ��ʵ��
        }
        else
        {
            Instance = this;
        }
    }

    // ��ӷ���
    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"Score: {currentScore}");
        // δ�����������UI
    }

    // ���㲢��ӵؿ���õ÷�
    public void ScoreTilePlacement(TileData placedTileData, int matchedEdgesCount)
    {
        int scoreToAdd = basePlacementScore; // �������÷�

        // ����ƥ��ߵĶ���ӷ�
        scoreToAdd += matchedEdgesCount * perfectMatchBonus;

        // ���ﻹ���Ը��� TileData �е�����������ӷ��������磺
        // if (placedTileData.isSpecialTile) { scoreToAdd += 50; }

        AddScore(scoreToAdd);
    }

    // δ�����������÷����ķ��� (��������Ϸ)
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
    }
}