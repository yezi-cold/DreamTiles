using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridRadius = 4;
    [SerializeField] private float tileSize = 1.0f;
    public float TileSize => tileSize; // <--- **新增这一行**


    // 不再直接引用hexTilePrefab，而是引用TileData，由TileData决定用哪个Prefab
    [Header("Initial Tile Data")]
    [SerializeField] private TileData startTileData; // 游戏开始时的第一个地块数据

    // 存储 TileController 实例，而不是 GameObject
    private Dictionary<HexCoord, TileController> grid = new Dictionary<HexCoord, TileController>();

    // 公开一个属性，让其他脚本可以查询网格数据
    public IReadOnlyDictionary<HexCoord, TileController> Grid => grid;

    private void Start()
    {
        if (startTileData == null)
        {
            Debug.LogError("Start Tile Data is not set in the GridManager!");
            return;
        }

        // 游戏开始时，只在中心位置生成一个起始地块
        SpawnTile(HexCoord.zero, startTileData,0); // HexCoord.zero 是我们之后要添加的，表示(0,0)
    }

    // 修改 SpawnTile 方法以接受 TileData
    public TileController SpawnTile(HexCoord coord, TileData tileData, int rotationIndex)
    {
        if (tileData.tilePrefab == null)
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;
        }

        Vector3 worldPosition = HexToWorld(coord);
        Quaternion worldRotation = Quaternion.Euler(0, rotationIndex * 60, 0);
        GameObject newTileGO = Instantiate(
            tileData.tilePrefab, // 使用TileData中定义的Prefab
            worldPosition,
           worldRotation,
            this.transform
        );

        // 获取并初始化 TileController
        TileController newTileController = newTileGO.GetComponent<TileController>();
        if (newTileController == null)
        {
            Debug.LogError("HexTile_Prefab is missing TileController component!");
            Destroy(newTileGO); // 销毁错误的实例
            return null;
        }
        // 关键修改：在 Initialize 方法中添加 'this' (即 GridManager 自身的引用)
        newTileController.Initialize(coord, tileData, this);

        grid.Add(coord, newTileController);
        // *** 新增：计算并添加分数 ***
        if (ScoreManager.Instance != null)
        {
            int matchedEdges = CalculateMatchedEdges(coord, tileData); // 调用新的辅助方法来计算匹配边数
            ScoreManager.Instance.ScoreTilePlacement(tileData, matchedEdges);
        }

        return newTileController; // 返回实例以便其他脚本使用
    }

    // *** 新增：辅助方法来计算放置地块时匹配的边数 ***
    private int CalculateMatchedEdges(HexCoord placedCoord, TileData placedTileData)
    {
        int matchedCount = 0;
        for (int i = 0; i < 6; i++)
        {
            HexDirection currentDir = (HexDirection)i;
            HexCoord neighborCoord = placedCoord.GetNeighbor(i);

            // 检查这个方向是否有邻居
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);

                // 获取新地块在这个方向的边缘类型
                EdgeType newTileEdge = placedTileData.edges[(int)currentDir];

                // 获取邻居地块在相反方向的边缘类型
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType(currentDir);

                // 如果边缘匹配 (注意这里的逻辑与 CanPlaceTile 略有不同，CanPlaceTile 是严格的，这里是计算匹配数量)
                // 只有当两者都不为 None 且类型完全一致才算作完美匹配
                if (newTileEdge != EdgeType.None &&
                    neighborOppositeEdge != EdgeType.None &&
                    newTileEdge == neighborOppositeEdge) // 注意这里是 ==
                {
                    matchedCount++;
                }
            }
        }
        return matchedCount;
    }

    public Vector3 HexToWorld(HexCoord coord)
    {
        float x = tileSize * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2 * coord.R);
        float z = tileSize * (3.0f / 2.0f * coord.R);

        return new Vector3(x, 0, z);
    }

    public HexCoord WorldToHex(Vector3 worldPosition)
    {
        float q = (worldPosition.x * Mathf.Sqrt(3) / 3 - worldPosition.z / 3) / tileSize;
        float r = (worldPosition.z * 2.0f / 3.0f) / tileSize;

        return HexRound(q, r);
    }

    private HexCoord HexRound(float q, float r)
    {
        float s = -q - r; // s 坐标

        int rx = Mathf.RoundToInt(q);
        int ry = Mathf.RoundToInt(r);
        int rz = Mathf.RoundToInt(s);

        float x_diff = Mathf.Abs(rx - q);
        float y_diff = Mathf.Abs(ry - r);
        float z_diff = Mathf.Abs(rz - s);

        if (x_diff > y_diff && x_diff > z_diff)
        {
            rx = -ry - rz;
        }
        else if (y_diff > z_diff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new HexCoord(rx, ry);
    }

    public bool HasTileAt(HexCoord coord)
    {
        return grid.ContainsKey(coord);
    }

    public TileController GetTileAt(HexCoord coord)
    {
        grid.TryGetValue(coord, out TileController tile);
        return tile;
    }

    // 新增方法：获取所有已放置地块的坐标
    public System.Collections.Generic.List<HexCoord> GetAllPlacedTileCoords()
    {
        // 修正：使用正确的字典变量名 'grid'
        return new System.Collections.Generic.List<HexCoord>(grid.Keys);
    }
}