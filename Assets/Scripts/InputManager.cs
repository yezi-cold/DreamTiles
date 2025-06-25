using UnityEngine;
using UnityEngine.Rendering; // 确保这个 using 还在

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GridManager gridManager;
    // [SerializeField] private InputManager inputManager; // *** 移除此行！这是多余的 ***

    private TileData currentTileToPlace; // 当前待放置的地块数据

    // 新增引用 TileDeckManager
    [SerializeField] private TileDeckManager tileDeckManager;


    // 幽灵地块相关
    private GameObject ghostTileInstance;
    private Material ghostTileValidMaterial;
    private Material ghostTileInvalidMaterial;

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
        ghostTileValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ghostTileValidMaterial.color = new Color(0, 1, 0, 0.5f);

        ghostTileInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ghostTileInvalidMaterial.color = new Color(1, 0, 0, 0.5f);

        SetMaterialBlendMode(ghostTileValidMaterial);
        SetMaterialBlendMode(ghostTileInvalidMaterial);

        // InputManager 不再在 Start 中初始化 currentTileToPlace，而是等待 TileDeckManager 通知
        // TileDeckManager 的 Start() 方法会调用 DrawNewTile()，然后 DrawNewTile() 会调用 InputManager 的 SetCurrentTileToPlace
    }

    void Update()
    {
        // 射线检测
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int groundLayer = LayerMask.GetMask("Ground"); // 确保创建并设置此层

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 worldPos = hit.point;
            HexCoord targetCoord = gridManager.WorldToHex(worldPos);

            // 更新幽灵地块的位置和状态
            UpdateGhostTile(targetCoord);

            // 鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTile(targetCoord);
            }
        }
        else
        {
            // 鼠标没有悬停在地面上，隐藏幽灵地块
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
        }
    }

    // 创建幽灵地块实例
    private void CreateGhostTile(TileData tileData)
    {
        if (tileData == null || tileData.tilePrefab == null) return;

        // 销毁旧的幽灵地块（如果有的话）
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        ghostTileInstance = Instantiate(tileData.tilePrefab);
        // 确保幽灵地块的渲染器在场景中可见，并且设置材质
        MeshRenderer renderer = ghostTileInstance.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            // 如果Prefab的根对象没有MeshRenderer，检查其子对象
            renderer = ghostTileInstance.GetComponentInChildren<MeshRenderer>();
        }

        if (renderer != null)
        {
            // 设置为半透明模式
            // SetMaterialBlendMode(ghostTileValidMaterial); // 这些在 Start 中已经设置过了，不需要每次创建都设置
            // SetMaterialBlendMode(ghostTileInvalidMaterial); // 除非你每次都创建新的材质，否则不需要
            renderer.material = ghostTileValidMaterial; // 初始设置为有效材质
        }
        else
        {
            Debug.LogWarning("Ghost Tile Prefab is missing a MeshRenderer component!");
        }

        // 禁用其TileController或其他游戏逻辑组件
        TileController tc = ghostTileInstance.GetComponent<TileController>();
        if (tc != null) tc.enabled = false;
        // 可以禁用其他可能不需要的组件，例如Collider

        ghostTileInstance.transform.parent = this.transform; // 作为InputManager的子对象
        ghostTileInstance.name = "GhostTile";
        // ghostTileInstance.SetActive(false); // 初始隐藏，UpdateGhostTile 会处理其显示
    }

    // 更新幽灵地块的位置和材质
    private void UpdateGhostTile(HexCoord targetCoord)
    {
        if (ghostTileInstance == null || currentTileToPlace == null) return;

        ghostTileInstance.SetActive(true);
        ghostTileInstance.transform.position = gridManager.HexToWorld(targetCoord);

        // 检查放置是否有效
        bool canPlace = CanPlaceTile(targetCoord, currentTileToPlace);

        MeshRenderer renderer = ghostTileInstance.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = ghostTileInstance.GetComponentInChildren<MeshRenderer>();
        }

        if (renderer != null)
        {
            renderer.material = canPlace ? ghostTileValidMaterial : ghostTileInvalidMaterial;
        }
    }

    // 设置材质的混合模式为Fade (半透明)
    private void SetMaterialBlendMode(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.SetOverrideTag("RenderType", "Transparent");
    }

    // 设置当前待放置的地块
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;
        if (currentTileToPlace != null) // 只有当有牌时才创建幽灵地块
        {
            CreateGhostTile(tileData);
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(true); // 确保幽灵地块可见
            }
        }
        else // 没有牌了，隐藏幽灵地块
        {
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance); // 牌堆空了，直接销毁幽灵地块
                ghostTileInstance = null; // 清空引用
            }
        }
    }

    // 尝试放置地块
    private void TryPlaceTile(HexCoord targetCoord)
    {
        if (currentTileToPlace == null)
        {
            Debug.LogWarning("No tile to place! Waiting for new tile from deck.");
            return;
        }

        if (CanPlaceTile(targetCoord, currentTileToPlace))
        {
            gridManager.SpawnTile(targetCoord, currentTileToPlace);
            Debug.Log($"Tile placed at {targetCoord.Q},{targetCoord.R}");

            // *** 核心修改：放置成功后，让牌堆抽取下一张牌 ***
            if (tileDeckManager != null)
            {
                tileDeckManager.DrawNewTile();
            }
            else
            {
                // 如果没有牌堆管理器，就清空当前牌，导致无法继续放置
                currentTileToPlace = null; // *** 修正：使用 currentTileToPlace ***
                if (ghostTileInstance != null) // 放置后隐藏幽灵地块
                {
                    ghostTileInstance.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("Cannot place tile here: invalid position or mismatched edges.");
        }
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
}