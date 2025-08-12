using UnityEngine;
//分数管理脚本
public class ScoreManager : MonoBehaviour
{
    //--单例模式--
    //单例模式确保一个类只有一个实例，并提供全局访问
    public static ScoreManager Instance { get; private set; } //公共静态属性，用于在任何地方访问socreManager实例

    //--字段--
    private int currentScore = 0;//私有整数字段，存储当前分数
    [Header("Scoring Settings")]
    [SerializeField] private int basePlacementScore = 10;//放置地块的基础分。
    [SerializeField] private int perfectMatchBonus = 5; // 每额外匹配一条边额外得分

    //--属性--
    public int CurrentScore => currentScore; // 公共只读属性，外部可以读取分数，但不能直接修改。

    //--unity生命周期方法--
    //awake在所有start方法之前被调用，非常适合拿来实现单例模式
    private void Awake()
    {
        //检查是否已经存在一个实例。
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 如果已经存在一个实例，销毁当前这个重复的对象
        }
        else
        {
            Instance = this;//否则，将当前实例赋值给静态属性Instance
        }
    }


    //--公共方法--
    //增加指定数量的分数
    public void AddScore(int amount)
    {
        currentScore += amount;//当前分数增加amount分数
        Debug.Log($"Score: {currentScore}");//控制台打印当前分数，便于调试。
        // 未来在这里添加更新ui显示的代码。
    }

    // 计算并增加一次地块放置的得分
    public void ScoreTilePlacement(TileData placedTileData, int matchedEdgesCount)
    {
        int scoreToAdd = basePlacementScore; // 局部变量，初始为基础放置分
        scoreToAdd += matchedEdgesCount * perfectMatchBonus;//加上匹配边的奖励分.
       // 这里是未来扩展得分规则的示例注释
       // if (placedTileData.isSpecialTile) { scoreToAdd += 50; }
       
        AddScore(scoreToAdd);//调用addscore方法增加计算出的总分.
    }

    // 重置分数用于新游戏。
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
    }
}