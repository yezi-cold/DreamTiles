using UnityEngine;
using System.Collections.Generic; // 需要这个命名空间

//地块控制脚本
public class TileController : MonoBehaviour
{
    //--属性--
    public HexCoord Coords { get; private set; } // 公共属性，存储地块坐标。{ get; private set; } 表示外部只能读取，只有这个类内部可以设置
    public TileData TileData { get; private set; } // 公共属性，存储地块的数据资源

    //--字段--
    // 私有数组，存储六个边缘游戏对象的MeshRenderer组件，用于改变材质（高亮）。
    [SerializeField] private MeshRenderer[] edgeRenderers = new MeshRenderer[6];

    //  私有字段，存储用于高亮的材质。
    [SerializeField] private Material highlightMaterial;
    // 私有数组，用于备份边缘的原始材质，以便取消高亮时恢复。
    private Material[] originalEdgeMaterials = new Material[6];
    //私有字段，存储对 GridManager 的引用
    private GridManager gridManager;

    //--方法--
    // 初始化方法，传入坐标、数据资源、GridManager引用
    public void Initialize(HexCoord coords, TileData tileData, GridManager gridManager)
    {
        this.Coords = coords;//设置当前地块的坐标
        this.TileData = tileData; // 存储传入的地块数据
        this.gridManager = gridManager; // 存储对 GridManager 的引用

        //--局部变量--
        // for循环种的i是一个局部变量，只在循环内有效
        for (int i = 0; i < 6; i++)//循环六次，处理每个边缘
        {
            if (edgeRenderers[i] != null)
            {
                // 保存原始材质，使用 sharedMaterial 避免创建新实例，影响性能
                originalEdgeMaterials[i] = edgeRenderers[i].sharedMaterial;
            }
            else
            {
                //如果没有指定，打印警告信息，帮助调试
                Debug.LogWarning($"TileController on {gameObject.name}: Edge Renderer at index {i} is not assigned!");
            }
        }
    }

    // 获取指定方向的边缘类型。
    public EdgeType GetEdgeType(HexDirection direction)
    {
        //先检查tiledata是否存在，以及方向索引是否在有效的数组范围内
        if (TileData != null && (int)direction >= 0 && (int)direction < TileData.edges.Length)
        {
            //从tiledata中edges数组种返回对应方向的边缘类型
            return TileData.edges[(int)direction];
        }
        return EdgeType.None; // 如果有任何问题，返回none
    }

    //获取与指定方向‘相反’的边缘类型。
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        //局部变量，计算反方向的索引，六边形网格种，相对方向总是 +3
        HexDirection oppositeDir = (HexDirection)(((int)direction + 3) % 6); 
        //调用自己的 GetEdgeType 方法来获取相反方向的边缘类型，并返回
        return GetEdgeType(oppositeDir);
    }

    // 设置指定方向的边缘高亮状态。
    public void SetEdgeHighlight(HexDirection direction, bool highlight)
    {
        int index = (int)direction;//将方向枚举转换为整数索引
        //确保索引有效且对应的renderer已经在inspector种设置
        if (index >= 0 && index < edgeRenderers.Length && edgeRenderers[index] != null)
        {
            //如果是要求高亮，并且高亮的材质已经设置
            if (highlight && highlightMaterial != null)
            {
                // 将边缘的材质设置为高亮材质。
                // 注意：使用 .material 会为该对象创建一个新的材质实例
                edgeRenderers[index].material = highlightMaterial;
            }
            else
            {
                // 恢复原始材质
                if (originalEdgeMaterials[index] != null)
                {
                    edgeRenderers[index].material = originalEdgeMaterials[index];
                }
            }
        }
    }
    //清除所有边缘的高亮状态
    public void ClearAllEdgeHighlights()
    {
        for (int i = 0; i < 6; i++)//循环六次，处理每个边缘
        {
            // 调用 SetEdgeHighlight 方法，将每个边缘的高亮状态都设置为 false。
            SetEdgeHighlight((HexDirection)i, false);
        }
    }
    /// <summary>
    /// 获取与指定“世界方向”相反的边缘类型，会考虑地块自身的旋转。
    /// </summary>
    /// <param name="worldDirection">来自外部的世界方向</param>
    /// <returns>该地块在接触面上对应的边缘类型</returns>
    public EdgeType GetOppositeEdgeTypeInWorld(HexDirection worldDirection)
    {
        // 1. 从游戏对象的 Transform 组件获取当前地块的旋转索引
        // 我们假设地块的Y轴旋转总是60度的整数倍
        int rotationIndex = Mathf.RoundToInt(transform.rotation.eulerAngles.y / 60) % 6;

        // 2. 计算出世界坐标中的“相反”方向
        // 在六边形网格中，相反方向的索引总是当前方向 + 3
        int oppositeWorldDirIndex = ((int)worldDirection + 3) % 6;

        // 3. 将这个“相反的世界方向”转换为地块自身的“本地边缘索引”
        // 逻辑和 TileData 中的方法一样
        int localEdgeIndex = (oppositeWorldDirIndex - rotationIndex + 6) % 6;

        // 4. 从地块数据中返回正确的边缘类型
        return TileData.edges[localEdgeIndex];
    }
}