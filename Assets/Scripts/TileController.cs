using UnityEngine;

public class TileController : MonoBehaviour
{
    public HexCoord myCoord { get; private set; } // 该地块的六边形坐标
    public TileData myTileData { get; private set; } // 该地块的数据定义

    // 初始化地块
    public void Initialize(HexCoord coord, TileData tileData)
    {
        myCoord = coord;
        myTileData = tileData;
        gameObject.name = $"Tile ({coord.Q}, {coord.R}) - {tileData.name}"; // 方便调试

        // 未来可能在这里根据tileData调整地块的视觉外观（如材质、子对象）
        // 例如：GetComponent<MeshRenderer>().material.color = tileData.displayColor;
    }

    // 获取特定方向的边缘类型
    public EdgeType GetEdgeType(HexDirection direction)
    {
        return myTileData.edges[(int)direction];
    }

    // 获取反方向的边缘类型（用于匹配）
    // 这是一个核心函数，用于判断两个相邻地块的边是否匹配
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        // 六边形的反方向是 (direction + 3) % 6
        return myTileData.edges[((int)direction + 3) % 6];
    }
}