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

    // 引用 InputManager，以便在抽取新牌时通知它
    [SerializeField] private InputManager inputManager;

    private void Awake()
    {
        if (inputManager == null)
        {
            inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                Debug.LogError("TileDeckManager: InputManager not found in scene!");
            }
        }
    }

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
            Debug.Log($"Drew new tile: {currentHandTile.name}. Remaining: {deck.Count}");

            // 通知 InputManager 更新当前待放置的地块
            if (inputManager != null)
            {
                inputManager.SetCurrentTileToPlace(currentHandTile);
            }
        }
        else
        {
            currentHandTile = null; // 牌堆为空
            Debug.Log("No more tiles left in the deck! Game Over (or end of round).");
            // 未来在这里触发游戏结束逻辑
            if (inputManager != null)
            {
                inputManager.SetCurrentTileToPlace(null); // 清空 InputManager 的当前牌
            }
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