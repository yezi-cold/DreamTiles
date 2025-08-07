using UnityEngine;

// 将枚举定义移到这里，因为它们与TileData紧密相关
public enum EdgeType
{
    None = 0,
    Grass = 1,
    Forest = 2,
    Water = 3,
    House = 4,
    Factory = 5,
}

public enum TileType
{
    Grass,
    Forest,
    Water,
    Mountain,
    Village
}


[CreateAssetMenu(fileName = "NewTileData", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Visuals")]
    public GameObject tilePrefab; // 该地块类型对应的视觉Prefab

    [Header("Gameplay Properties")]
    public TileType tileType; // <--- 【在这里添加这一行】
    public int baseScore = 1; // 放置此地块的基础得分

    [Header("Edges")]
    [Tooltip("定义六个方向的边缘类型，顺序与HexDirection枚举对应：Right, UpRight, UpLeft, Left, DownLeft, DownRight")]
    public EdgeType[] edges = new EdgeType[6]; // 六个边的类型
}