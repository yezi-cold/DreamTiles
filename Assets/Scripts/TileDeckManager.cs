using System.Collections.Generic;
using UnityEngine;
using System.Linq; // ����ling�⣬��Ȼ��ǰ�汾δֱ��ʹ���䷽������ͨ���ڴ�����ʱ�����á�

//�ƶѹ�����������ű����������õĵؿ飬�������һ����һ��
public class TileDeckManager : MonoBehaviour
{
    //--��Ƕ��--
    [System.Serializable]//������������inspector����ʾ��
    public class TileTypeEntry //����һ�����ݽṹ��������inspector�з��������ÿ�ܵؿ������
    {
        public TileData tileData;//�ؿ�����
        public int amount; // �����͵ؿ����ƶ��е�����
    }

    //--�ֶ�--
    [Header("Deck Settings")]
    [Tooltip("�����ƶ��а����ĵؿ����ͼ�������")]
    [SerializeField] private List<TileTypeEntry> tileTypesInDeck;//˽���б�������inspector�������ƶ�

    //˽�ж��У�queue�������ڴ洢ϴ�õ��ƣ��������Ƚ��ȳ������ݽṹ���ǳ��ʺ�ģ����ơ�
    private Queue<TileData> deck = new Queue<TileData>(); // �ƶѣ�ʹ�ö��з����ȡ
    private TileData currentHandTile; // ˽���ֶΣ��洢��ҵ�ǰ���ϵĵؿ顣

    //--����--
    public TileData CurrentHandTile => currentHandTile;//����ֻ�����ԣ���ҵ�ǰ���ϵĵؿ�
    public int RemainingTilesCount => deck.Count; // ����ֻ�����ԣ���ȡ�ƶ���ʣ���������


    void Start()
    {
        InitializeDeck();//��ʼ���ƶ�
        DrawNewTile(); // ��Ϸ��ʼʱ��ȡ��һ����
    }

    //--��������--
    //��ȡ���Ƶķ���
    public void DrawNewTile()
    {
        if (deck.Count > 0)//����ƶ��Ƿ�����
        {
            currentHandTile = deck.Dequeue(); // ���ƶѶ���ȡ��һ����
            GameManager.Instance.OnNewTileDrawn(currentHandTile);// ֪ͨ GameManager �����ѳ�ȡ��gamemanager�Ḻ��֪ͨtileplacer
        }
        else//����ƶ�Ϊ��
        {
            currentHandTile = null; // ��������Ϊ�ա�
            GameManager.Instance.OnNewTileDrawn(null);//֪ͨgamemanager���ѳ��ꡣ
        }
    }

    //--˽�з���--
    //��ʼ����ϴ��
    private void InitializeDeck()
    {
        //�ֲ�������һ����ʱ���б�����ϴ��ǰ������еؿ�
        List<TileData> tempDeckList = new List<TileData>();

        //������inspector�����õ�ÿһ�ֵؿ顣
        foreach (var entry in tileTypesInDeck)
        {
            if (entry.tileData != null)//ȷ�������˵ؿ�����
            {
                //�������õ�������amount�������ؿ����ݶ����ӵ���ʱ�б���
                for (int i = 0; i < entry.amount; i++)
                {
                    tempDeckList.Add(entry.tileData);
                }
            }
        }
        Shuffle(tempDeckList);// ����ϴ�Ʒ���
        // ��ϴ���Ƶ��б�ת��Ϊ���У�����ƶѵĴ���
        deck = new Queue<TileData>(tempDeckList);
        Debug.Log($"Deck initialized with {deck.Count} tiles.");
    }

    //ϴ���㷨
    private void Shuffle(List<TileData> list)
    {
        //�ֲ�������һ���������������
        System.Random rng = new System.Random(); // ʹ�� System.Random ���� Unity.Random����Ϊ���ǿ��ظ���
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);//���ѡ��һ������
            TileData value = list[k];//����Ԫ�ء�
            list[k] = list[n];
            list[n] = value;
        }
    }
}