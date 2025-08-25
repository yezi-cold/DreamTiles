using UnityEngine;

//地块数据脚本
//顶定义游戏中每一种地块的所有属性（外观，类型，分数，边的类型）。它使用了scriptableobject（可编写脚本对象），可以创建数据资源文件。
public enum EdgeType//定义 边的类型
{
    None = 0,//空白或者未定义
    Grass = 1,//草地
    Forest = 2,//森林
    Water = 3,//水
    House = 4,//房子
    Factory = 5,//工厂
    Road = 6,//道路
}

public enum TileType//定义地块的主体类型
{
    Park,
    River,
    Residential,
    Industrial,
}

//--类定义--
//[CreateAssetMenu] 这个特性可以在unity的‘assets/create’菜单中显示创建该资源文件的选项。
[CreateAssetMenu(fileName = "NewTileData", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject//继承自ScriptableObject，表示这是一个可以创建的资源文件
{
    //--字段--
    //这些公共字段回直接显示在unity的Inspector面板上，可以在资源文件里进行配置。
    [Header("Visuals")]//此特性可以在inspector面板上添加一个标题，用于划分字段
    public GameObject tilePrefab; // 公共字段用于存放该地块对应的prefab

    [Header("Gameplay Properties")]
    public TileType tileType; // 公共字段，定义这个地块的类型（来自上面的枚举）
    public int baseScore = 1; // 公共字段，放置此地块的基础得分

    [Header("Edges")]
    //[tooltip]此特性在你鼠标悬停在字段上时显示一个提示信息。

    [Tooltip("定义六个方向的边缘类型，顺序与HexDirection枚举对应：Right, UpRight, UpLeft, Left, DownLeft, DownRight")]
    public EdgeType[] edges = new EdgeType[6]; // 公共数组字段，存储六个边的类型。数组的顺序对应 HexDirection 枚举的顺序
}