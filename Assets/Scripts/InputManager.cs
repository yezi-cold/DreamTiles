using UnityEngine;
using UnityEngine.Rendering; // 确保这个 using 还在

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GridManager gridManager;

    private System.Collections.Generic.List<GameObject> activeHexOutlines = new System.Collections.Generic.List<GameObject>();

    // *** 重新添加幽灵地块相关的材质和实例字段 ***
    private GameObject ghostTileInstance;

    private TileData currentTileToPlace; // 当前待放置的地块数据

    // 新增引用 TileDeckManager
    [SerializeField] private TileDeckManager tileDeckManager;

    // 幽灵边框的颜色仍然保留，因为边框还在
    [Header("Ghost Hex Outline Materials")]
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f); // 默认绿色半透明
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f); // 默认红色半透明

    private Material hexOutlineValidMaterial;
    private Material hexOutlineInvalidMaterial;


    // 新增：旋转角度和当前旋转状态
    private int currentTileRotationIndex = 0; // 0-5，对应 HexDirection
    private const int NUM_HEX_DIRECTIONS = 6; // 六个方向

    [Header("Ghost Hex Outline")]
    [SerializeField]
    private GameObject ghostHexOutlinePrefab; // 用于显示空地块边框的Prefab


    [Header("Camera Control")] // 新增：相机控制参数
    [SerializeField] private float zoomSpeed = 5f; // 缩放速度
    [SerializeField] private float minZoom = 2f; // 最小缩放值 (最大放大)
    [SerializeField] private float maxZoom = 10f; // 最大缩放值 (最小放大)

    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                Debug.LogError("InputManager: No camera assigned and no MainCamera found!");
            }
        }
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("InputManager: No GridManager assigned and none found in scene!");
            }
        }
        // 新增：查找 TileDeckManager
        if (tileDeckManager == null)
        {
            tileDeckManager = FindObjectOfType<TileDeckManager>();
            if (tileDeckManager == null)
            {
                Debug.LogError("InputManager: TileDeckManager not found in scene!");
            }
        }
    }

    void Start()
    {
        // 幽灵边框材质的初始化
        hexOutlineValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hexOutlineValidMaterial.color = validHexOutlineColor;

        hexOutlineInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;

        // 设置边框材质的混合模式为半透明（Fade），这是必须的
        SetMaterialBlendMode(hexOutlineValidMaterial);
        SetMaterialBlendMode(hexOutlineInvalidMaterial);

        // InputManager 不再在 Start 中初始化 currentTileToPlace，而是等待 TileDeckManager 通知
        // TileDeckManager 的 Start() 方法会调用 DrawNewTile()，然后 DrawNewTile() 会调用 InputManager 的 SetCurrentTileToPlace
    }

    void Update()
    {
        // 相机缩放逻辑
        HandleCameraZoom();

        // 射线检测
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int groundLayer = LayerMask.GetMask("Ground"); // 确保创建并设置此层

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 worldPos = hit.point;
            HexCoord targetCoord = gridManager.WorldToHex(worldPos);

            // *** 重新启用幽灵地块模型的更新逻辑 ***
            UpdateGhostTile(targetCoord);

            // 处理幽灵边框的显示和隐藏
            HandleGhostHexOutlines();

            // 鼠标中键滚动旋转地块
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0 && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) // 只有在没有按Ctrl时才旋转
            {
                RotateCurrentTile(scroll > 0); // true for clockwise, false for counter-clockwise
            }

            // 鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTile(targetCoord);
            }
        }
        else // 鼠标没有悬停在地面上
        {
            // *** 重新启用幽灵地块模型的隐藏逻辑 ***
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
            HideAllHexOutlines(); // 隐藏所有幽灵边框
        }
    } // End of Update() method

    // 新增方法：处理相机缩放
    private void HandleCameraZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        // 只有当按下 Ctrl 键时才进行缩放
        if (scroll != 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            // 正交相机
            if (gameCamera.orthographic)
            {
                gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
            // 透视相机
            else
            {
                gameCamera.fieldOfView = Mathf.Clamp(gameCamera.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }
    }

    // *** 重新添加 CreateGhostTile 方法 ***
    // 此方法现在将创建幽灵地块，但不会修改其材质，使其保持原始不透明状态
    private void CreateGhostTile(TileData tileData)
    {
        if (tileData == null || tileData.tilePrefab == null) return;

        // 销毁旧的幽灵地块（如果有的话）
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        ghostTileInstance = Instantiate(tileData.tilePrefab);

        // *** 关键修改：不再设置幽灵地块的材质为半透明高亮材质 ***
        // 幽灵地块将保持其 Prefab 自身的原始材质，不进行修改。
        // 如果 Prefab 自身材质是半透明的，那它就是半透明的。
        // 如果 Prefab 自身材质是不透明的，那它就是不透明的。
        // 我们只负责让它显示和跟随鼠标。

        // 禁用其TileController或其他游戏逻辑组件
        TileController tc = ghostTileInstance.GetComponent<TileController>();
        if (tc != null) tc.enabled = false;
        // 可以禁用其他可能不需要的组件，例如Collider

        ghostTileInstance.transform.parent = this.transform; // 作为InputManager的子对象
        ghostTileInstance.name = "GhostTile";
        ghostTileInstance.SetActive(false); // 初始隐藏，Update 会根据情况显示
    }

    // *** 重新添加 UpdateGhostTile 方法 ***
    // 此方法现在将更新幽灵地块的位置和旋转，但不会修改其材质
    private void UpdateGhostTile(HexCoord targetCoord)
    {
        if (ghostTileInstance == null || currentTileToPlace == null) return;

        ghostTileInstance.SetActive(true);
        ghostTileInstance.transform.position = gridManager.HexToWorld(targetCoord);
        // 应用旋转
        ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60f, 0);

        // *** 关键修改：不再在 Update 中修改幽灵地块的材质 ***
        // 材质在 Instantiate 时就已经确定，不需要每次更新都重新设置。
    }


    // 新增：旋转地块数据以进行碰撞检测和放置
    private TileData GetRotatedTileData(TileData originalTileData, int rotationIndex)
    {
        if (rotationIndex == 0) return originalTileData; // 未旋转

        // 创建一个临时的 TileData 对象来存储旋转后的边缘
        TileData rotatedTileData = ScriptableObject.CreateInstance<TileData>();
        rotatedTileData.name = originalTileData.name + "_Rotated";
        rotatedTileData.tilePrefab = originalTileData.tilePrefab; // Prefab不变

        // 旋转边缘数组
        rotatedTileData.edges = new EdgeType[NUM_HEX_DIRECTIONS];
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)
        {
            // 计算旋转后的索引
            int rotatedIndex = (i + rotationIndex) % NUM_HEX_DIRECTIONS;
            rotatedTileData.edges[rotatedIndex] = originalTileData.edges[i];
        }
        return rotatedTileData;
    }

    // 新增：旋转当前手牌的视觉和数据
    private void RotateCurrentTile(bool clockwise)
    {
        if (currentTileToPlace == null) return; // 没有地块不能旋转

        if (clockwise)
        {
            currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        }
        else
        {
            currentTileRotationIndex = (currentTileRotationIndex - 1 + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;
            // 确保索引不会变成负数
            if (currentTileRotationIndex < 0)
            {
                currentTileRotationIndex += NUM_HEX_DIRECTIONS;
            }
        }

        // 旋转地块后，更新幽灵地块的旋转
        if (ghostTileInstance != null)
        {
            ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60f, 0);
        }

        // 刷新所有的幽灵边框，以便它们反映新的“可放置性”状态
        HandleGhostHexOutlines();
    }

    // 设置材质的混合模式为半透明 (仅用于 Hex Outline 材质)
    private void SetMaterialBlendMode(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0); // 关闭深度写入，防止半透明物体遮挡
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON"); // 启用透明混合
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; // 设置渲染队列为透明
        material.SetOverrideTag("RenderType", "Transparent");
    }

    // 设置当前待放置的地块
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;
        currentTileRotationIndex = 0; // 每次抽到新牌时重置旋转

        // *** 重新启用幽灵地块实例的创建和销毁逻辑 ***
        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);
            if (ghostTileInstance != null)
            {
                ghostTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                ghostTileInstance.SetActive(false); // 初始隐藏，Update 会根据情况显示
            }
        }
        else // 没有牌了
        {
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance);
                ghostTileInstance = null;
            }
            HideAllHexOutlines();
        }

        // 当手牌变化时，刷新边框
        HandleGhostHexOutlines();
    }

    // 尝试放置地块
    private void TryPlaceTile(HexCoord targetCoord)
    {
        if (currentTileToPlace == null)
        {
            Debug.LogWarning("No tile to place! Waiting for new tile from deck.");
            return;
        }

        // 使用旋转后的地块数据进行放置验证
        TileData rotatedTileDataToPlace = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (CanPlaceTile(targetCoord, rotatedTileDataToPlace)) // 传入旋转后的 TileData
        {
            // 传递旋转后的 TileData 给 SpawnTile
            gridManager.SpawnTile(targetCoord, rotatedTileDataToPlace);
            Debug.Log($"Tile placed at {targetCoord.Q},{targetCoord.R}");

            // 销毁幽灵地块
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance);
                ghostTileInstance = null;
            }

            if (tileDeckManager != null)
            {
                tileDeckManager.DrawNewTile(); // 放置成功，抽取新牌
            }
            else
            {
                currentTileToPlace = null;
                // 如果没有牌堆管理器，并且牌已用完，则隐藏边框
                HideAllHexOutlines();
            }
        }
        else
        {
            Debug.Log("Cannot place tile here: invalid position or mismatched edges.");
        }
    }

    // 修改 ShowHexOutline 以管理多个实例
    private void ShowHexOutline(HexCoord targetCoord, bool isValidPlacement)
    {
        if (ghostHexOutlinePrefab == null)
        {
            Debug.LogWarning("Hex Outline Prefab is not assigned in InputManager!");
            return;
        }

        // 每次调用 ShowHexOutline 都会创建一个新的实例，并添加到列表中
        GameObject newOutline = Instantiate(ghostHexOutlinePrefab);
        newOutline.name = $"HexOutline_{targetCoord.Q}_{targetCoord.R}";
        newOutline.transform.parent = this.transform; // 作为InputManager的子对象
        newOutline.transform.position = gridManager.HexToWorld(targetCoord);
        newOutline.SetActive(true);

        MeshRenderer renderer = newOutline.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = newOutline.GetComponentInChildren<MeshRenderer>();
        }
        if (renderer != null)
        {
            // 使用边框材质
            renderer.material = isValidPlacement ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;
            renderer.material.color = isValidPlacement ? hexOutlineValidMaterial.color : hexOutlineInvalidMaterial.color;
        }
        activeHexOutlines.Add(newOutline); // 将新创建的实例添加到列表中
    }


    // 修改 HideHexOutline 为 HideAllHexOutlines
    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)
        {
            if (outline != null)
            {
                Destroy(outline); // 销毁所有活跃的边框实例
            }
        }
        activeHexOutlines.Clear(); // 清空列表
    }


    // --- 核心放置验证逻辑（保持不变） ---
    private bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        // 1. 检查目标位置是否为空
        if (gridManager.HasTileAt(targetCoord))
        {
            return false; // 位置已被占据
        }

        // 2. 检查目标位置是否有至少一个已放置的邻居 (不能放在虚空中)
        bool hasAdjacentTile = false;
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);
            if (gridManager.HasTileAt(neighborCoord))
            {
                hasAdjacentTile = true;
                break;
            }
        }
        if (!hasAdjacentTile)
        {
            return false; // 必须至少与一个现有地块相邻
        }

        // 3. 检查所有相邻地块的边缘是否匹配
        for (int i = 0; i < 6; i++) // 遍历所有六个方向
        {
            HexDirection currentDir = (HexDirection)i;
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);

            // 如果这个方向有邻居
            if (gridManager.HasTileAt(neighborCoord))
            {
                TileController neighborTile = gridManager.GetTileAt(neighborCoord);

                // 获取新地块在这个方向的边缘类型
                EdgeType newTileEdge = tileToPlace.edges[(int)currentDir];

                // 获取邻居地块在相反方向的边缘类型
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType(currentDir);

                // 匹配规则:
                // 1. 如果新地块的边是 None，则不强制匹配（兼容未来的空白边）
                // 2. 如果邻居地块的反向边是 None，则不强制匹配
                // 3. 否则，两者必须完全匹配
                if (newTileEdge != EdgeType.None &&
                    neighborOppositeEdge != EdgeType.None &&
                    newTileEdge != neighborOppositeEdge)
                {
                    return false; // 边缘不匹配
                }
            }
        }

        return true; // 所有检查都通过，可以放置
    }

    // HandleGhostHexOutlines 方法保持不变，因为它处理的是边框，而不是幽灵地块模型
    private void HandleGhostHexOutlines()
    {
        HideAllHexOutlines(); // 首先隐藏所有旧的边框

        System.Collections.Generic.HashSet<HexCoord> potentialPlacementCoords = new System.Collections.Generic.HashSet<HexCoord>();

        // 添加中心地块周围的空位置
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighbor = HexCoord.zero.GetNeighbor(i);
            if (!gridManager.HasTileAt(neighbor))
            {
                potentialPlacementCoords.Add(neighbor);
            }
        }

        // 添加所有已放置地块的邻居空位置
        foreach (var placedTileCoord in gridManager.GetAllPlacedTileCoords())
        {
            for (int i = 0; i < 6; i++)
            {
                HexCoord neighbor = placedTileCoord.GetNeighbor(i);
                if (!gridManager.HasTileAt(neighbor))
                {
                    potentialPlacementCoords.Add(neighbor);
                }
            }
        }


        // 遍历所有潜在的放置位置，并显示幽灵网格
        foreach (HexCoord coord in potentialPlacementCoords)
        {
            // 检查这个位置是否可以放置当前手上的牌
            TileData rotatedTileData = null;
            bool canPlace = false;
            if (currentTileToPlace != null)
            {
                rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);
                canPlace = CanPlaceTile(coord, rotatedTileData);
            }

            // 即使手上没有牌，或者不能放置当前牌，我们仍然显示边框，但可能是不同的颜色
            ShowHexOutline(coord, canPlace);
        }
    }
}