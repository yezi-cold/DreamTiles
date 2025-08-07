using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用于 Shuffle 方法

public class TileDeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [Tooltip("定义牌堆中包含的地块类型及其数量")]
    [SerializeField] private List<TileTypeEntry> tileTypesInDeck;

    [System.Serializable]
    public class TileTypeEntry
    {
        public TileData tileData;
        public int amount; // 该类型地块在牌堆中的数量
    }

    private Queue<TileData> deck = new Queue<TileData>(); // 牌堆，使用队列方便抽取
    private TileData currentHandTile; // 玩家当前手上的地块

    public TileData CurrentHandTile => currentHandTile;
    public int RemainingTilesCount => deck.Count; // 用于UI显示


    void Start()
    {
        InitializeDeck();
        DrawNewTile(); // 游戏开始时抽取第一张牌
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

        // 洗牌
        Shuffle(tempDeckList);

        // 将洗好的牌放入队列
        deck = new Queue<TileData>(tempDeckList);

        Debug.Log($"Deck initialized with {deck.Count} tiles.");
    }

    // 抽取新牌的方法
    public void DrawNewTile()
    {
        if (deck.Count > 0)
        {
            currentHandTile = deck.Dequeue(); // 从牌堆顶部取出一张牌
            GameManager.Instance.OnNewTileDrawn(currentHandTile);// 通知 GameManager 新牌已抽取

        }
        else
        {
            currentHandTile = null; // 牌堆为空
            GameManager.Instance.OnNewTileDrawn(null);         
        }
    }

    // 简单的洗牌算法 (Fisher-Yates Shuffle)
    private void Shuffle(List<TileData> list)
    {
        System.Random rng = new System.Random(); // 使用 System.Random 而非 Unity.Random，因为它是可重复的
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