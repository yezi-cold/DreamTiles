using UnityEngine;
using System.Collections.Generic;

public class TilePlacer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera gameCamera;

    [Header("Ghost Tile Settings")]
    [SerializeField] private GameObject ghostHexOutlinePrefab;
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f);

    private TileData currentTileToPlace;
    private int currentTileRotationIndex = 0;
    private GameObject ghostTileInstance;
    private List<GameObject> activeHexOutlines = new List<GameObject>();

    private Material hexOutlineValidMaterial;
    private Material hexOutlineInvalidMaterial;

    private const int NUM_HEX_DIRECTIONS = 6;

    void Awake()
    {
        // �Զ���������
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Start()
    {
        // ��������߿�Ĳ���
        Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        if (defaultShader == null) defaultShader = Shader.Find("Standard");

        hexOutlineValidMaterial = new Material(defaultShader);
        hexOutlineValidMaterial.color = validHexOutlineColor;
        SetMaterialBlendMode(hexOutlineValidMaterial, MaterialBlendMode.Fade);

        hexOutlineInvalidMaterial = new Material(defaultShader);
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;
        SetMaterialBlendMode(hexOutlineInvalidMaterial, MaterialBlendMode.Fade);
    }

    void Update()
    {
        // ������ѭ��������ʵʱ��ʾ����Ԥ��
        if (currentTileToPlace != null)
        {
            UpdateGhostPreviews();
        }
    }

    // ������������InputManager����
    public void HandlePlacement(Vector3 mousePosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            HexCoord clickedHexCoord = gridManager.WorldToHex(hit.point);
            TryPlaceTile(clickedHexCoord);
        }
    }

    // ������������InputManager����
    public void HandleRotation()
    {
        currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        // ��ת����Ҫ���̸���Ԥ��
        UpdateGhostPreviews();
    }

    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;

        // ����ɵ�����ؿ�
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);
            ghostTileInstance.SetActive(true);
        }
        else
        {
            // ���û�еؿ��ˣ������Ƴ����ˣ�����������Ԥ��
            HideAllHexOutlines();
        }

        // �κ�ʱ��ؿ�仯����ǿ�Ƹ���һ��Ԥ��
        UpdateGhostPreviews();
    }

    private void UpdateGhostPreviews()
    {
        HandleGhostTile();
        HandleGhostHexOutlines();
    }

    #region Ghost Tile Logic
    private void HandleGhostTile()
    {
        if (ghostTileInstance == null) return;

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition); // Ghost Tile ���Ǹ����������λ��
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            HexCoord mouseHexCoord = gridManager.WorldToHex(hit.point);
            Vector3 worldPos = gridManager.HexToWorld(mouseHexCoord);
            ghostTileInstance.transform.position = worldPos;
            ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60, 0);
            ghostTileInstance.SetActive(true);
        }
        else
        {
            ghostTileInstance.SetActive(false);
        }
    }

    private void CreateGhostTile(TileData tileData)
    {
        ghostTileInstance = Instantiate(tileData.tilePrefab);
        // �Ƴ����в���Ҫ�����
        foreach (var component in ghostTileInstance.GetComponents<MonoBehaviour>()) Destroy(component);
        foreach (var collider in ghostTileInstance.GetComponents<Collider>()) Destroy(collider);
        ghostTileInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
        ghostTileInstance.SetActive(false);
    }
    #endregion

    #region Ghost Outline Logic
    private void HandleGhostHexOutlines()
    {
        HideAllHexOutlines();

        if (currentTileToPlace == null) return;

        HashSet<HexCoord> potentialCoords = GetPotentialPlacementCoords();

        foreach (HexCoord coord in potentialCoords)
        {
            TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);
            bool canPlace = gridManager.CanPlaceTile(coord, rotatedTileData);

            GameObject outline = Instantiate(ghostHexOutlinePrefab, gridManager.HexToWorld(coord), Quaternion.identity, this.transform);
            outline.transform.localScale = Vector3.one * gridManager.TileSize;

            MeshRenderer renderer = outline.GetComponent<MeshRenderer>();
            renderer.material = canPlace ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;

            activeHexOutlines.Add(outline);
        }
    }

    private HashSet<HexCoord> GetPotentialPlacementCoords()
    {
        HashSet<HexCoord> potentialCoords = new HashSet<HexCoord>();
        if (gridManager.GetAllPlacedTileCoords().Count == 0)
        {
            potentialCoords.Add(HexCoord.zero);
        }
        else
        {
            foreach (var placedCoord in gridManager.GetAllPlacedTileCoords())
            {
                for (int i = 0; i < 6; i++)
                {
                    HexCoord neighbor = placedCoord.GetNeighbor(i);
                    if (!gridManager.HasTileAt(neighbor))
                    {
                        potentialCoords.Add(neighbor);
                    }
                }
            }
        }
        return potentialCoords;
    }

    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)
        {
            if (outline != null) Destroy(outline);
        }
        activeHexOutlines.Clear();
    }
    #endregion

    #region Placement and Rule Logic
    private void TryPlaceTile(HexCoord targetCoord)
    {
        TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (gridManager.CanPlaceTile(targetCoord, rotatedTileData))
        {
            gridManager.SpawnTile(targetCoord, rotatedTileData, currentTileRotationIndex);
        }
        else
        {
            Debug.Log("Cannot place tile here.");
        }
    }

    
    private TileData GetRotatedTileData(TileData originalTileData, int rotation)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();
        rotatedData.name = originalTileData.name + "_Rotated";
        rotatedData.tilePrefab = originalTileData.tilePrefab;
        rotatedData.tileType = originalTileData.tileType;

        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)
        {
            int originalIndex = (i - rotation + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;
            rotatedData.edges[i] = originalTileData.edges[originalIndex];
        }
        return rotatedData;
    }
    #endregion

    #region Material Blending Utility
    // ������߷������Ա�������Ϊ����߿����Ⱦ�������
    public enum MaterialBlendMode { Opaque, Cutout, Fade, Transparent }

    private void SetMaterialBlendMode(Material material, MaterialBlendMode blendMode)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;

        switch (blendMode)
        {
            case MaterialBlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
                // ... ����ģʽ ...
        }
    }
    #endregion
}