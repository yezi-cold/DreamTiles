using UnityEngine;
//��������ű�
public class ScoreManager : MonoBehaviour
{
    //--����ģʽ--
    //����ģʽȷ��һ����ֻ��һ��ʵ�������ṩȫ�ַ���
    public static ScoreManager Instance { get; private set; } //������̬���ԣ��������κεط�����socreManagerʵ��

    //--�ֶ�--
    private int currentScore = 0;//˽�������ֶΣ��洢��ǰ����
    [Header("Scoring Settings")]
    [SerializeField] private int basePlacementScore = 10;//���õؿ�Ļ����֡�
    [SerializeField] private int perfectMatchBonus = 5; // ÿ����ƥ��һ���߶���÷�

    //--����--
    public int CurrentScore => currentScore; // ����ֻ�����ԣ��ⲿ���Զ�ȡ������������ֱ���޸ġ�

    //--unity�������ڷ���--
    //awake������start����֮ǰ�����ã��ǳ��ʺ�����ʵ�ֵ���ģʽ
    private void Awake()
    {
        //����Ƿ��Ѿ�����һ��ʵ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ����Ѿ�����һ��ʵ�������ٵ�ǰ����ظ��Ķ���
        }
        else
        {
            Instance = this;//���򣬽���ǰʵ����ֵ����̬����Instance
        }
    }


    //--��������--
    //����ָ�������ķ���
    public void AddScore(int amount)
    {
        currentScore += amount;//��ǰ��������amount����
        Debug.Log($"Score: {currentScore}");//����̨��ӡ��ǰ���������ڵ��ԡ�
        // δ����������Ӹ���ui��ʾ�Ĵ��롣
    }

    // ���㲢����һ�εؿ���õĵ÷�
    public void ScoreTilePlacement(TileData placedTileData, int matchedEdgesCount)
    {
        int scoreToAdd = basePlacementScore; // �ֲ���������ʼΪ�������÷�
        scoreToAdd += matchedEdgesCount * perfectMatchBonus;//����ƥ��ߵĽ�����.
       // ������δ����չ�÷ֹ����ʾ��ע��
       // if (placedTileData.isSpecialTile) { scoreToAdd += 50; }
       
        AddScore(scoreToAdd);//����addscore�������Ӽ�������ܷ�.
    }

    // ���÷�����������Ϸ��
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
    }
}