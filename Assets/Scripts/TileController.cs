using UnityEngine;
using System.Collections.Generic; // 需要这个命名空间

public class TileController : MonoBehaviour
{
    public HexCoord Coords { get; private set; } // 该地块的六边形坐标
    public TileData TileData { get; private set; } // 该地块的数据定义

    // 存储六个边缘的 MeshRenderer 引用。需要在 Unity Inspector 中手动赋值。
    [SerializeField] private MeshRenderer[] edgeRenderers = new MeshRenderer[6];

    // 用于高亮的材质。需要在 Unity Inspector 中拖入发光材质。
    [SerializeField] private Material highlightMaterial;

    // 存储每个边缘的原始材质，以便在取消高亮时恢复
    private Material[] originalEdgeMaterials = new Material[6];

    // 在 Initialize 方法中传入 GridManager 的引用，避免 FindObjectOfType 性能开销
    public void Initialize(HexCoord coords, TileData tileData, GridManager gridManager)
    {
        Coords = coords;
        TileData = tileData;
        gameObject.name = $"Tile ({coords.Q}, {coords.R}) - {tileData.name}"; // 方便调试

        // 设置地块的世界位置，使用传入的 GridManager 引用
        transform.position = gridManager.HexToWorld(Coords);

        // 首次初始化时保存所有边缘的原始材质
        for (int i = 0; i < 6; i++)
        {
            if (edgeRenderers[i] != null)
            {
                // 使用 sharedMaterial 避免创建新实例，影响性能
                originalEdgeMaterials[i] = edgeRenderers[i].sharedMaterial;
            }
            else
            {
                Debug.LogWarning($"TileController on {gameObject.name}: Edge Renderer at index {i} is not assigned!");
            }
        }
    }

    /// <summary>
    /// 获取指定方向的边缘类型。
    /// </summary>
    /// <param name="direction">要获取的六边形方向。</param>
    /// <returns>该方向的边缘类型。</returns>
    public EdgeType GetEdgeType(HexDirection direction)
    {
        if (TileData != null && (int)direction >= 0 && (int)direction < TileData.edges.Length)
        {
            return TileData.edges[(int)direction];
        }
        return EdgeType.None; // 默认返回 None
    }

    /// <summary>
    /// 获取与指定方向相反的边缘类型。
    /// </summary>
    /// <param name="direction">当前方向。</param>
    /// <returns>相反方向的边缘类型。</returns>
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        HexDirection oppositeDir = (HexDirection)(((int)direction + 3) % 6); // 相对方向总是 +3
        return GetEdgeType(oppositeDir);
    }

    /// <summary>
    /// 设置指定方向边缘的高亮状态。
    /// </summary>
    /// <param name="direction">要高亮的边缘方向。</param>
    /// <param name="highlight">是否高亮。</param>
    public void SetEdgeHighlight(HexDirection direction, bool highlight)
    {
        int index = (int)direction;
        if (index >= 0 && index < edgeRenderers.Length && edgeRenderers[index] != null)
        {
            if (highlight && highlightMaterial != null)
            {
                // 注意：这里使用 .material 会创建材质实例，可能增加 Draw Call。
                // 对于大量高亮，可以考虑使用 MaterialPropertyBlock。
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

    /// <summary>
    /// 清除所有边缘的高亮状态，恢复原始材质。
    /// </summary>
    public void ClearAllEdgeHighlights()
    {
        for (int i = 0; i < 6; i++)
        {
            SetEdgeHighlight((HexDirection)i, false);
        }
    }
}