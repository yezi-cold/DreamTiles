using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*网格管理器
是游戏的核心逻辑之一，负责管理场景中所有地块的位置、生成和数据存储。
整体功能: 创建和维护一个六边形网格的字典数据结构。它负责将地块放置在正确的物理位置，
将世界坐标（鼠标点击位置）转换为网格坐标，并包含了验证地块能否放置的核心规则。*/
public class GridManager : MonoBehaviour
{
    //--字段--

    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1.0f;//私有字段，定义单个六边形的大小
                                                 
    [Header("Initial Tile Data")]
    [SerializeField] private TileData startTileData; // 私有字段，存放游戏开始时放置在中心起始地块的数据。

    //私有字典，这是网格的核心数据结构
    //键是六边形的坐标，值是对应坐标上的 TileController 实例
    private Dictionary<HexCoord, TileController> grid = new Dictionary<HexCoord, TileController>();

    //用于追踪特殊地块 
    private bool riverPlaced = false; // 是否已经放置过河流地块
    private bool roadPlaced = false;  // 是否已经放置过道路地块

    //--属性--
    public float TileSize => tileSize; // 公共只读属性，让其他脚本可以安全地获取tileSize的值。
    public IReadOnlyDictionary<HexCoord, TileController> Grid => grid;//公共只读属性，将私有的grid字典以只读的形式暴露给其他脚本。
   
    //--unity生命周期函数--
    private void Start()
    {
        if (startTileData == null)//检查起始位置是否设置了地块
        {
            Debug.LogError("Start Tile Data is not set in the GridManager!");
            return;//提前退出方法，防止后续代码出错
        }

        // 游戏开始时，在网格中心（0，0）生成初始地块，初始旋转为0
        SpawnTile(HexCoord.Zero, startTileData, 0); // HexCoord.zero 是我们之后要添加的，表示(0,0)
    }
    //--公共方法--
    //在指定坐标生成一个地块。
    public TileController SpawnTile(HexCoord coord, TileData tileData, int rotationIndex)
    {
        // --- 局部变量 ---
        Vector3 worldPosition = HexToWorld(coord); // 将六边形网格坐标转换为Unity世界坐标。
        Quaternion worldRotation = Quaternion.Euler(0, rotationIndex * 60, 0); // 根据旋转索引计算世界旋转角度。

        //实例化地块预制体，生成一个新的游戏对象,将新生成的地块对象作为GridManager对象的子对象，便于场景管理。
        GameObject newTileGO = Instantiate(tileData.tilePrefab, worldPosition, worldRotation, this.transform);

        // 获取新地块上的 TileController 组件，并调用其 Initialize 方法
        TileController newTileController = newTileGO.GetComponent<TileController>();

        if (tileData.tilePrefab == null)//检查地块数据中是否有关联的预制体。
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;//如果没有，打印错误返回null
        }

        if (newTileController == null)
        {
            Debug.LogError("HexTile_Prefab is missing TileController component!");
            Destroy(newTileGO); // 销毁错误的实例
            return null;
        }

        // 调用新地块的 Initialize 方法，将坐标、数据、网格管理器等信息传递给它。
        newTileController.Initialize(coord, tileData, this);

        //将新生成的地块添加到网格字典中
        grid.Add(coord, newTileController);

        // 在放置成功后，更新特殊地块的追踪状态 
        if (!riverPlaced && tileData.edges.Contains(EdgeType.River))
        {
            riverPlaced = true;
            Debug.Log("第一条河流已被放置!");
        }
        if (!roadPlaced && tileData.edges.Contains(EdgeType.Road))
        {
            roadPlaced = true;
            Debug.Log("第一条道路已被放置!");
        }
        // ▼▼▼ 【新增】调用新的计分方法 ▼▼▼
        CalculateAndAddScores(coord, tileData, rotationIndex);

        return newTileController;
    }

    //将六边形坐标转换为unity世界坐标
    public Vector3 HexToWorld(HexCoord coord)
    {
        //基于六边形网格的数学公式进行坐标转换。
        float x = tileSize * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2 * coord.R);
        float z = tileSize * (3.0f / 2.0f * coord.R);
        return new Vector3(x, 0, z);//返回计算出的3d世界坐标
    }
    //将unity世界坐标转换为六边形坐标
    public HexCoord WorldToHex(Vector3 worldPosition)
    {
        //基于六边形网格的数学公式进行坐标转换。
        float q = (worldPosition.x * Mathf.Sqrt(3) / 3 - worldPosition.z / 3) / tileSize;
        float r = (worldPosition.z * 2.0f / 3.0f) / tileSize;
        return HexRound(q, r);//对浮点坐标进行四舍五入，得到最终的整数网格坐标。
    }

    //检查指定坐标是否存在地块
    public bool HasTileAt(HexCoord coord)
    {
        return grid.ContainsKey(coord);//检查字典中是否有指定坐标的键。
    }

    //获取指定坐标的地块控制器
    public TileController GetTileAt(HexCoord coord)
    {
        //尝试从字典中获取值
        grid.TryGetValue(coord, out TileController tile);
        //返回找到地块控制器，如果没有则返回null
        return tile;
    }
    
    //获取所有已放置地块的坐标列表
    public List<HexCoord> GetAllPlacedTileCoords()
    {
        //返回一个包含grid字典所有键（坐标）的新列表
        return new List<HexCoord>(grid.Keys);
    }

    //检查地块是否可以放置在目标坐标的核心逻辑
    public bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        // 基础条件1：当前位置没有地块
        if (HasTileAt(targetCoord))
        {
            return false;
        }

        // 基础条件2：相邻处必须有地块
        bool hasNeighbor = false;
        for (int i = 0; i < 6; i++)
        {
            if (HasTileAt(targetCoord.GetNeighbor(i)))
            {
                hasNeighbor = true;
                break;
            }
        }
        // 如果是游戏刚开始（只有初始地块），则必须放在原点旁边
        // 如果已经有很多地块了，则不能凭空放置
        if (!hasNeighbor && grid.Count > 1)
        {
            return false;
        }
        // 处理第一块紧挨着初始地块的情况
        if (grid.Count == 1 && !hasNeighbor)
        {
            return false;
        }


        // --- 特殊地块放置要求 ---
        bool hasRiverEdge = tileToPlace.edges.Contains(EdgeType.River);
        bool hasRoadEdge = tileToPlace.edges.Contains(EdgeType.Road);

        // 如果是普通地块，只要满足基础条件即可
        if (!hasRiverEdge && !hasRoadEdge)
        {
            return true;
        }

        // 如果是特殊地块，检查是否是第一次出现
        if (hasRiverEdge && !riverPlaced)
        {
            return true; // 第一个河流地块，可以随意放置
        }
        if (hasRoadEdge && !roadPlaced)
        {
            return true; // 第一个道路地块，可以随意放置
        }

        // 如果是第二次及以后出现的特殊地块，必须找到匹配的邻居
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);
                EdgeType newTileEdge = tileToPlace.edges[i];
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeTypeInWorld((HexDirection)i);

                // 检查河流连接
                if (hasRiverEdge && newTileEdge == EdgeType.River && neighborOppositeEdge == EdgeType.River)
                {
                    return true; // 找到了一个有效的河流连接点
                }
                // 检查道路连接
                if (hasRoadEdge && newTileEdge == EdgeType.Road && neighborOppositeEdge == EdgeType.Road)
                {
                    return true; // 找到了一个有效的道路连接点
                }
            }
        }

        // 遍历完所有邻居，都没有找到必须的连接点
        return false;
    }

    // -- 私有辅助方法 --
    // ▼▼▼ 【核心新增方法】根据你的规则计算并添加分数 ▼▼▼
    private void CalculateAndAddScores(HexCoord placedCoord, TileData placedTileData, int rotationIndex)
    {
        //创建临时的分数容器，初始都为0
        int prosperityDelta = 0;
        int populationDelta = 0;
        int happinessDelta = 0;

        // 遍历6个方向的邻居，检查每条接触边的类型
        for (int i = 0; i < 6; i++)
        {
            //获取当前方向邻居坐标
            HexCoord neighborCoord = placedCoord.GetNeighbor(i);
            //检查这个方向是否有邻居
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);

                // 获取新地块在当前世界方向(i)的边缘类型
                EdgeType newTileEdge = placedTileData.GetEdgeForWorldDirection(i, rotationIndex);

                // 获取邻居地块与我们接触的那条边的边缘类型
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeTypeInWorld((HexDirection)i);

                // 查询ScoreManager的规则表，获取这对边缘连接的分数
                ScoreModifier modifier = ScoreManager.Instance.GetAdjacencyBonus(newTileEdge, neighborOppositeEdge);

                // 累加从规则中获得的分数
                prosperityDelta += modifier.prosperity;
                populationDelta += modifier.population;
                happinessDelta += modifier.happiness;
            }
        }

        // 将计算出的总增量分数添加到ScoreManager
        ScoreManager.Instance.AddScores(prosperityDelta, populationDelta, happinessDelta);

        // 通知GameManager地块已放置，现在只负责触发抽牌
        GameManager.Instance.OnTilePlaced();
    }

    //对浮点六边形坐标进行四舍五入，得到整数坐标。
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
    //获取旋转后的牌
    public TileData GetRotatedTileData(TileData originalTileData, int rotation)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();//克隆牌数据
        rotatedData.name = originalTileData.name + "_Rotated";//给旋转后的牌一个新的名字
        rotatedData.tilePrefab = originalTileData.tilePrefab;//设置旋转后的牌的预制体
        rotatedData.edgeType = originalTileData.edgeType;//设置旋转后的牌的类型

        const int NUM_HEX_DIRECTIONS = 6;
        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];//设置旋转后的牌的边类型
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)//遍历六个方向
        {
            int originalIndex = (i - rotation + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;//计算原始边的索引
            rotatedData.edges[i] = originalTileData.edges[originalIndex];//设置旋转后的边类型
        }
        return rotatedData;//返回旋转后的牌数据
    }

}