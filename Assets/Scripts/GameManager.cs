using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 引用所有的子系统/管理器
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