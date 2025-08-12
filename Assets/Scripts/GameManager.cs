using UnityEngine;
/*游戏总管理器脚本
 * 负责协调各个子系统之间的通信。整体功能: 使用单例模式，让自己成为一个全局唯一的协调者。
 * 它监听来自一个管理器（如GridManager）的事件，然后向另一个管理器（如ScoreManager或TileDeckManager）发出指令。
 * 这使得各个子系统之间不需要互相直接引用，降低了耦合度，让项目结构更清晰*/
public class GameManager : MonoBehaviour
{
    //--单例--
    public static GameManager Instance { get; private set; }

    //--字段--
    // 引用所有的子系统/管理器，这些都需要在unity inspector中手动拖拽赋值
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private TileDeckManager tileDeckManager;
    [SerializeField] private ScoreManager scoreManager;
    // 未来还可以添加 UIManager, AudioManager 等

    private void Awake()
    {
        // 单例模式标准写法
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- 公开的协调方法 ---

    // 当地块被成功放置后，由GridManager调用
    public void OnTilePlaced(HexCoord coord, TileData tileData, int matchedEdgesCount)
    {
        // 协调1：通知ScoreManager计分
        scoreManager.ScoreTilePlacement(tileData, matchedEdgesCount);

        // 协调2：通知TileDeckManager抽下一张牌
        tileDeckManager.DrawNewTile();
    }

    // 当牌堆抽出一张新牌后，由TileDeckManager调用
    public void OnNewTileDrawn(TileData newTile)
    {
        // 协调：通知TilePlacer更新手上的牌
        tilePlacer.SetCurrentTileToPlace(newTile);
    }
}