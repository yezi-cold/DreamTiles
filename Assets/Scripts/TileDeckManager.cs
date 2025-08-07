using System.Collections.Generic;
using UnityEngine;
using System.Linq; // ���� Shuffle ����

public class TileDeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [Tooltip("�����ƶ��а����ĵؿ����ͼ�������")]
    [SerializeField] private List<TileTypeEntry> tileTypesInDeck;

    [System.Serializable]
    public class TileTypeEntry
    {
        public TileData tileData;
        public int amount; // �����͵ؿ����ƶ��е�����
    }

    private Queue<TileData> deck = new Queue<TileData>(); // �ƶѣ�ʹ�ö��з����ȡ
    private TileData currentHandTile; // ��ҵ�ǰ���ϵĵؿ�

    public TileData CurrentHandTile => currentHandTile;
    public int RemainingTilesCount => deck.Count; // ����UI��ʾ


    void Start()
    {
        InitializeDeck();
        DrawNewTile(); // ��Ϸ��ʼʱ��ȡ��һ����
    }

    private void InitializeDeck()
    {
        List<TileData> tempDeckList = new List<TileData>();

        foreach (var entry in tileTypesInDeck)
        {
            if (entry.tileData != null)
            {
                for (int i = 0; i < entry.amount; i++)
                {
                    tempDeckList.Add(entry.tileData);
                }
            }
            else
            {
                Debug.LogWarning($"TileDeckManager: A TileTypeEntry has no TileData assigned! Skipping.");
            }
        }

        // ϴ��
        Shuffle(tempDeckList);

        // ��ϴ�õ��Ʒ������
        deck = new Queue<TileData>(tempDeckList);

        Debug.Log($"Deck initialized with {deck.Count} tiles.");
    }

    // ��ȡ���Ƶķ���
    public void DrawNewTile()
    {
        if (deck.Count > 0)
        {
            currentHandTile = deck.Dequeue(); // ���ƶѶ���ȡ��һ����
            GameManager.Instance.OnNewTileDrawn(currentHandTile);// ֪ͨ GameManager �����ѳ�ȡ

        }
        else
        {
            currentHandTile = null; // �ƶ�Ϊ��
            GameManager.Instance.OnNewTileDrawn(null);         
        }
    }

    // �򵥵�ϴ���㷨 (Fisher-Yates Shuffle)
    private void Shuffle(List<TileData> list)
    {
        System.Random rng = new System.Random(); // ʹ�� System.Random ���� Unity.Random����Ϊ���ǿ��ظ���
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            TileData value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}