using UnityEngine;
using System.Collections.Generic;
/*地块放置器脚本
 整体功能: 接收InputManager的指令，管理当前待放置的地块。它负责创建和更新跟随鼠标的“幽灵地块”预览，
 以及在所有可放置位置显示“幽灵边框”。当确认放置时，它会请求GridManager来验证和生成地块。*/
public class TilePlacer : MonoBehaviour
{
    //--字段--
    [Header("Dependencies")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera gameCamera;

    [Header("Ghost Tile Settings")]
    [SerializeField] private GameObject ghostHexOutlinePrefab;//幽灵边框的预制体
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f);//有效位置的边框颜色
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f);//无效位置的边框颜色

    private TileData currentTileToPlace;//当前等待放置的地块数据
    private int currentTileRotationIndex = 0;//当前地块的旋转索引（0-5）
    private GameObject ghostTileInstance;//幽灵地块的游戏对象实例
    private List<GameObject> activeHexOutlines = new List<GameObject>();//存储当前显示的所有幽灵边框的实例

    private Material hexOutlineValidMaterial;//用于有效位置的动态创建的材质
    private Material hexOutlineInvalidMaterial;//用于无效位置的动态创建的材质

    private const int NUM_HEX_DIRECTIONS = 6;//私有常量，表示六边形的方向总数。

    void Awake()
    {
        // 自动查找依赖
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Start()
    {
        //--局部变量--
        //找到项目中的默认lit着色器，用于创建半透明材质
        Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        if (defaultShader == null) defaultShader = Shader.Find("Standard");
        
        //动态创建用于有效和无效位置的边框材质
        hexOutlineValidMaterial = new Material(defaultShader);
        hexOutlineValidMaterial.color = validHexOutlineColor;
        SetMaterialBlendMode(hexOutlineValidMaterial, MaterialBlendMode.Fade); // 设置为半透明模式。

        hexOutlineInvalidMaterial = new Material(defaultShader);
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;
        SetMaterialBlendMode(hexOutlineInvalidMaterial, MaterialBlendMode.Fade);
    }

    void Update()
    {
        // 如果手上有牌
        if (currentTileToPlace != null)
        {
            UpdateGhostPreviews();//每帧更新幽灵地块和幽灵边框的位置及状态
        }
    }

    //--公共方法--
    // 处理放置请求（由inputmanager调用）
    public void HandlePlacement(Vector3 mousePosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(mousePosition);//从摄像机向鼠标位置发射一条射线
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground"))) // 如果射线碰到了"Ground"图层的物体。
        {
            HexCoord clickedHexCoord = gridManager.WorldToHex(hit.point);// 将碰撞点的世界坐标转换为网格坐标。
            TryPlaceTile(clickedHexCoord);//尝试在该网络坐标放置地块
        }
    }

    //处理旋转请求（由inputmanager调用）
    public void HandleRotation()
    {
        // 旋转索引加1，并用取余运算确保其在0-5之间循环。
        currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        // 旋转后立刻更新预览
        UpdateGhostPreviews();
    }

    //处置当前要放置的地块（由GridManager在抽牌调用）
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;//更新当前手牌

        // 销毁旧的幽灵地块
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);//创建新的幽灵地块
            ghostTileInstance.SetActive(true);//激活它
        }
        else
        {
            HideAllHexOutlines();// 如果没有地块了（牌抽完），隐藏所有边框
        }

        UpdateGhostPreviews();// 拿到新牌后立即更新一次预览。
    }

    //--私有方法--
    private void UpdateGhostPreviews()//更新幽灵显示状态
    {
        HandleGhostTile();//更新幽灵地块跟随鼠标
        HandleGhostHexOutlines();//在所有潜在的可放置位置显示边框，并根据gridmanager的CanPlaceTile结果设置颜色。
    }

    #region Ghost Tile Logic
    //处理幽灵地块的显示和跟随
    private void HandleGhostTile()
    {
        if (ghostTileInstance == null) return;//如果没有幽灵地块，返回null，就不用更新它。

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition); // Ghost Tile 总是跟随最新鼠标位置
        //如果射线碰到了地面，就将幽灵地块的位置和旋转设置成跟随鼠标的位置和旋转
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            HexCoord mouseHexCoord = gridManager.WorldToHex(hit.point);//将碰撞点的世界坐标转换为网格坐标。
            Vector3 worldPos = gridManager.HexToWorld(mouseHexCoord);//将网格坐标转换为世界坐标。
            ghostTileInstance.transform.position = worldPos;//设置幽灵地块的位置。
            ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60, 0);//设置幽灵地块的旋转。
            ghostTileInstance.SetActive(true);//激活它。
        }
        else
        {
            ghostTileInstance.SetActive(false);//否则就隐藏它。
        }
    }

    //创建一个纯视觉的，没有碰撞和脚本的地块副本
    private void CreateGhostTile(TileData tileData)
    {
        ghostTileInstance = Instantiate(tileData.tilePrefab);//克隆地块预制体
        // 销毁所有组件，只保留网格和渲染器
        foreach (var component in ghostTileInstance.GetComponents<MonoBehaviour>()) Destroy(component);
        // 销毁所有碰撞器
        foreach (var collider in ghostTileInstance.GetComponents<Collider>()) Destroy(collider);
        // 重置层级
        ghostTileInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
        // 隐藏它
        ghostTileInstance.SetActive(false);
    }
    #endregion

    #region Ghost Outline Logic
    //处理幽灵边框的显示和更新
    private void HandleGhostHexOutlines()
    {
        HideAllHexOutlines();//隐藏所有边框

        if (currentTileToPlace == null) return;//如果没有牌，就不用显示边框。

        HashSet<HexCoord> potentialCoords = GetPotentialPlacementCoords();//获取所有潜在的放置位置
        
        
        foreach (HexCoord coord in potentialCoords)//遍历所有潜在的放置位置
        {
            TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);//获取旋转后的牌
            bool canPlace = gridManager.CanPlaceTile(coord, rotatedTileData);//根据网格管理器的规则判断是否可以放置
            //克隆边框预制体
            GameObject outline = Instantiate(ghostHexOutlinePrefab, gridManager.HexToWorld(coord), Quaternion.identity, this.transform);
           
            outline.transform.localScale = Vector3.one * gridManager.TileSize;//设置边框的大小

            MeshRenderer renderer = outline.GetComponent<MeshRenderer>();//获取边框的渲染器
           
            renderer.material = canPlace ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;//设置边框的材质

            activeHexOutlines.Add(outline);//添加到激活边框列表中
        }
    }

    //获取所有潜在的放置位置
    private HashSet<HexCoord> GetPotentialPlacementCoords()
    {
        //创建一个HashSet来存储潜在的放置位置
        HashSet<HexCoord> potentialCoords = new HashSet<HexCoord>();
        //如果还没有放置任何牌，就添加中心位置
        if (gridManager.GetAllPlacedTileCoords().Count == 0)
        {
            potentialCoords.Add(HexCoord.Zero);//添加中心位置
        }
        else
        {
            //遍历所有已经放置的牌
            foreach (var placedCoord in gridManager.GetAllPlacedTileCoords())
            {
                for (int i = 0; i < 6; i++)//遍历六个方向
                {
                    HexCoord neighbor = placedCoord.GetNeighbor(i);//获取相邻位置
                    //如果相邻位置没有牌，就添加到潜在位置列表中
                    if (!gridManager.HasTileAt(neighbor))
                    {
                        potentialCoords.Add(neighbor);
                    }
                }
            }
        }
        return potentialCoords;//返回潜在的放置位置列表
    }

    //隐藏所有边框
    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)//遍历激活边框列表
        {
            if (outline != null) Destroy(outline);//如果边框实例不为空，就销毁它。
        }
        activeHexOutlines.Clear();//清空激活的边框列表
    }
    #endregion

    #region Placement and Rule Logic

    //尝试在目标网格坐标放置牌
    private void TryPlaceTile(HexCoord targetCoord)
    {
        //获取旋转后的牌
        TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (gridManager.CanPlaceTile(targetCoord, rotatedTileData))//如果网格管理器允许放置，就放置牌
        {
            gridManager.SpawnTile(targetCoord, rotatedTileData, currentTileRotationIndex);//生成地块
        }
        else
        {
            Debug.Log("Cannot place tile here.");
        }
    }

    //获取旋转后的牌
    private TileData GetRotatedTileData(TileData originalTileData, int rotation)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();//克隆牌数据
        rotatedData.name = originalTileData.name + "_Rotated";//给旋转后的牌一个新的名字
        rotatedData.tilePrefab = originalTileData.tilePrefab;//设置旋转后的牌的预制体
        rotatedData.tileType = originalTileData.tileType;//设置旋转后的牌的类型
        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];//设置旋转后的牌的边类型
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)//遍历六个方向
        {
            int originalIndex = (i - rotation + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;//计算原始边的索引
            rotatedData.edges[i] = originalTileData.edges[originalIndex];//设置旋转后的边类型
        }
        return rotatedData;//返回旋转后的牌数据
    }
    #endregion

    #region Material Blending Utility
    // 这个工具方法可以保留，因为它与边框的渲染紧密相关
    public enum MaterialBlendMode { Opaque, Cutout, Fade, Transparent }// 材质混合模式枚举

    //设置材质的混合模式
    private void SetMaterialBlendMode(Material material, MaterialBlendMode blendMode)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);//源混合模式
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);//目标混合模式
        material.SetInt("_ZWrite", 1);//深度写入
        material.DisableKeyword("_ALPHATEST_ON");//关闭透明度测试
        material.DisableKeyword("_ALPHABLEND_ON");//关闭透明混合
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");// 关闭预乘混合
        material.renderQueue = -1;//渲染队列

        switch (blendMode)//根据混合模式设置材质属性
        {
            case MaterialBlendMode.Fade://半透明
                material.SetOverrideTag("RenderType", "Transparent");//渲染类型
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//源混合模式
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//目标混合模式
                material.SetInt("_ZWrite", 0);//关闭深度写入
                material.EnableKeyword("_ALPHABLEND_ON");//开启透明混合
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;//渲染队列
                break;
                // ... 其他模式 ...
        }
    }
    #endregion
}