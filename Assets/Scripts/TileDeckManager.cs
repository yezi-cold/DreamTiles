using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 引入ling库，虽然当前版本未直接使用其方法，但通常在处理集合时很有用。

//牌堆管理器：这个脚本负责管理可用的地块，就像管理一副牌一样
public class TileDeckManager : MonoBehaviour
{
    //--内嵌类--
    [System.Serializable]//让这个类可以在inspector中显示。
    public class TileTypeEntry //定义一个数据结构，用于在inspector中方便的设置每周地块的数量
    {
        public TileData tileData;//地块数据
        public int amount; // 该类型地块在牌堆中的数量
    }

    //--字段--
    [Header("Deck Settings")]
    [Tooltip("定义牌堆中包含的地块类型及其数量")]
    [SerializeField] private List<TileTypeEntry> tileTypesInDeck;//私有列表，用于在inspector中配置牌堆

    //私有队列（queue），用于存储洗好的牌，队列是先进先出的数据结构，非常适合模拟抽牌。
    private Queue<TileData> deck = new Queue<TileData>(); // 牌堆，使用队列方便抽取
    private TileData currentHandTile; // 私有字段，存储玩家当前手上的地块。

    //--属性--
    public TileData CurrentHandTile => currentHandTile;//公共只读属性，玩家当前手上的地块
    public int RemainingTilesCount => deck.Count; // 公共只读属性，获取牌堆中剩余的牌数。


    void Start()
    {
        InitializeDeck();//初始化牌堆
        DrawNewTile(); // 游戏开始时抽取第一张牌
    }

    //--公共方法--
    //抽取新牌的方法
    public void DrawNewTile()
    {
        if (deck.Count > 0)//检查牌堆是否还有牌
        {
            currentHandTile = deck.Dequeue(); // 从牌堆顶部取出一张牌
            GameManager.Instance.OnNewTileDrawn(currentHandTile);// 通知 GameManager 新牌已抽取，gamemanager会负责通知tileplacer
        }
        else//如果牌堆为空
        {
            currentHandTile = null; // 手牌设置为空。
            GameManager.Instance.OnNewTileDrawn(null);//通知gamemanager牌已抽完。
        }
    }

    //--私有方法--
    //初始化并洗牌
    private void InitializeDeck()
    {
        //局部变量，一个临时的列表，用于洗牌前存放所有地块
        List<TileData> tempDeckList = new List<TileData>();

        //遍历在inspector中设置的每一种地块。
        foreach (var entry in tileTypesInDeck)
        {
            if (entry.tileData != null)//确保设置了地块数据
            {
                //根据设置的数量（amount），将地块数据多次添加到临时列表中
                for (int i = 0; i < entry.amount; i++)
                {
                    tempDeckList.Add(entry.tileData);
                }
            }
        }
        Shuffle(tempDeckList);// 调用洗牌方法
        // 将洗好牌的列表转换为队列，完成牌堆的创建
        deck = new Queue<TileData>(tempDeckList);
        Debug.Log($"Deck initialized with {deck.Count} tiles.");
    }

    //洗牌算法
    private void Shuffle(List<TileData> list)
    {
        //局部变量，一个随机数生成器。
        System.Random rng = new System.Random(); // 使用 System.Random 而非 Unity.Random，因为它是可重复的
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);//随机选择一个索引
            TileData value = list[k];//交换元素。
            list[k] = list[n];
            list[n] = value;
        }
    }
}