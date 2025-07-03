using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // MaterialBlendMode ö����Ȼ��������Ϊ�߿���õ�
    public enum MaterialBlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    [SerializeField] private Camera gameCamera;
    [SerializeField] private GridManager gridManager;

    private List<GameObject> activeHexOutlines = new List<GameObject>();

    // ����ؿ���� - �Ƴ� ghostTileValidColor �� ghostTileInvalidColor
    private GameObject ghostTileInstance;

    private TileData currentTileToPlace;

    [SerializeField] private TileDeckManager tileDeckManager;

    // ����ؿ����Ч/��Ч��ɫ�ֶ����Ƴ�����Ϊ���ٸı��������ɫ��͸����

    // ����߿����ɫ��Ȼ��������Ϊ�߿���
    [Header("Ghost Hex Outline Materials")]
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f); // Ĭ����ɫ��͸��
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f); // Ĭ�Ϻ�ɫ��͸��

    private Material hexOutlineValidMaterial;
    private Material hexOutlineInvalidMaterial;

    private int currentTileRotationIndex = 0;
    private const int NUM_HEX_DIRECTIONS = 6;

    [Header("Ghost Hex Outline")]
    [SerializeField] private GameObject ghostHexOutlinePrefab;
    private GameObject currentHexOutlineInstance;

    [Header("Camera Control")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float floatMaxZoom = 10f; // �����������ƺ�����Ӧ����maxZoom

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
        if (tileDeckManager == null)
        {
            tileDeckManager = FindObjectOfType<TileDeckManager>();
            if (tileDeckManager == null)
            {
                Debug.LogError("InputManager: TileDeckManager not found in scene! Some functionalities may be limited.");
            }
        }
    }

    void Start()
    {
        Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        if (defaultShader == null)
        {
            Debug.LogWarning("Universal Render Pipeline/Lit shader not found. Falling back to Standard shader. Please ensure URP is imported if you intend to use it.");
            defaultShader = Shader.Find("Standard");
        }
        if (defaultShader == null)
        {
            Debug.LogError("Neither URP/Lit nor Standard shader found. Ghost tile/outline materials might not render correctly.");
            return;
        }

        // ����߿���ʣ���Щ��Ȼ��Ҫ�����Ĳ��ʣ�
        hexOutlineValidMaterial = new Material(defaultShader);
        hexOutlineValidMaterial.color = validHexOutlineColor;
        SetMaterialBlendMode(hexOutlineValidMaterial, MaterialBlendMode.Fade); // �߿�ʹ�ð�͸��

        hexOutlineInvalidMaterial = new Material(defaultShader);
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;
        SetMaterialBlendMode(hexOutlineInvalidMaterial, MaterialBlendMode.Fade); // �߿�ʹ�ð�͸��
    }


    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
        HandleGhostTile(); // �����������ֻ��������ؿ��λ�ú���ת
        HandleGhostHexOutlines(); // �߿��߼�����
        HandleCameraZoom();
    }


    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
            {
                HexCoord clickedHexCoord = gridManager.WorldToHex(hit.point);
                TryPlaceTile(clickedHexCoord);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            RotateGhostTile();
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateGhostTile();
        }
    }

    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            // ������maxZoom �ֶ���������������һ��
            gameCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, floatMaxZoom);
        }
    }

    private void CreateGhostTile(TileData tileData)
    {
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        if (tileData == null || tileData.tilePrefab == null)
        {
            Debug.LogError("Attempted to create ghost tile with null TileData or null tilePrefab!");
            return;
        }

        ghostTileInstance = Instantiate(tileData.tilePrefab);
        // �Ƴ����нű�����ײ��ȣ�ʹ�䴿�������Ӿ�
        foreach (var component in ghostTileInstance.GetComponents<MonoBehaviour>())
        {
            Destroy(component);
        }
        foreach (var collider in ghostTileInstance.GetComponents<Collider>())
        {
            Destroy(collider);
        }
        ghostTileInstance.layer = LayerMask.NameToLayer("Ignore Raycast");

        // ****** �ؼ��޸ģ���ȫ�Ƴ�������ؿ���ʵ��޸� ******
        // ���ٶ� MeshRenderer �Ĳ��ʽ��� SetMaterialBlendMode ����ɫ����

        ghostTileInstance.SetActive(false); // ����ֱ����Ҫ
    }


    private void UpdateGhostTilePosition(HexCoord targetCoord) // �Ƴ� isValidPlacement ����
    {
        if (ghostTileInstance == null) return;

        Vector3 worldPos = gridManager.HexToWorld(targetCoord);
        ghostTileInstance.transform.position = worldPos;
        ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60, 0);

        // ****** �ؼ��޸ģ���ȫ�Ƴ�������ؿ���ʵ��޸� ******
        // ���ٸ�����Ч�Ըı������ɫ��͸����
    }


    private void HandleGhostTile()
    {
        if (currentTileToPlace == null)
        {
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
            return;
        }

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
        {
            HexCoord mouseHexCoord = gridManager.WorldToHex(hit.point);

            // ����ؿ鱾���ٸı���ʣ����� UpdateGhostTilePosition ������Ҫ canPlace
            UpdateGhostTilePosition(mouseHexCoord);
            ghostTileInstance.SetActive(true);
        }
        else
        {
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
        }
    }

    private void RotateGhostTile()
    {
        currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        if (ghostTileInstance != null)
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground")))
            {
                HexCoord mouseHexCoord = gridManager.WorldToHex(hit.point);
                // ����ؿ鱾���ٸı���ʣ����� UpdateGhostTilePosition ������Ҫ canPlace
                UpdateGhostTilePosition(mouseHexCoord);
            }
        }
    }

    private TileData GetRotatedTileData(TileData originalTileData, int rotationIndex)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();
        rotatedData.name = originalTileData.name + "_Rotated";
        rotatedData.tilePrefab = originalTileData.tilePrefab;
        rotatedData.tileType = originalTileData.tileType;

        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)
        {
            int originalIndex = (i - rotationIndex + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;
            rotatedData.edges[i] = originalTileData.edges[originalIndex];
        }
        return rotatedData;
    }

    private bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        if (gridManager.HasTileAt(targetCoord))
        {
            return false;
        }

        bool hasAdjacentTile = false;
        if (gridManager.GetAllPlacedTileCoords().Count == 0)
        {
            return targetCoord.Q == 0 && targetCoord.R == 0;
        }

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
            return false;
        }

        for (int i = 0; i < 6; i++)
        {
            HexDirection currentDir = (HexDirection)i;
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);

            if (gridManager.HasTileAt(neighborCoord))
            {
                TileController neighborTile = gridManager.GetTileAt(neighborCoord);
                EdgeType newTileEdge = tileToPlace.edges[(int)currentDir];
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType(currentDir);

                if (newTileEdge != EdgeType.None && neighborOppositeEdge != EdgeType.None)
                {
                    if (newTileEdge != neighborOppositeEdge)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }


    private void TryPlaceTile(HexCoord targetCoord)
    {
        if (currentTileToPlace == null)
        {
            Debug.LogWarning("No tile to place! Waiting for new tile from deck.");
            return;
        }

        TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (CanPlaceTile(targetCoord, rotatedTileData))
        {
            gridManager.SpawnTile(targetCoord, rotatedTileData, currentTileRotationIndex);
            Debug.Log($"Tile placed at {targetCoord.Q},{targetCoord.R}");

            if (tileDeckManager != null)
            {
                tileDeckManager.DrawNewTile();
            }
            else
            {
                Debug.LogWarning("TileDeckManager is not assigned. Cannot draw new tile.");
                currentTileToPlace = null;
            }
        }
        else
        {
            Debug.Log("Cannot place tile here: invalid position or mismatched edges.");
        }
    }

    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;
        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(true);
            }
        }
        else
        {
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
            HideAllHexOutlines();
        }
    }


    private void HandleGhostHexOutlines()
    {
        if (currentTileToPlace == null)
        {
            HideAllHexOutlines();
            return;
        }

        HideAllHexOutlines();

        HashSet<HexCoord> potentialPlacementCoords = new HashSet<HexCoord>();

        if (gridManager.GetAllPlacedTileCoords().Count == 0)
        {
            potentialPlacementCoords.Add(HexCoord.zero);
        }
        else
        {
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
        }

        foreach (HexCoord coord in potentialPlacementCoords)
        {
            TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);
            bool canPlace = CanPlaceTile(coord, rotatedTileData);

            if (ghostHexOutlinePrefab == null)
            {
                Debug.LogError("Ghost Hex Outline Prefab is NOT assigned in the Inspector! Cannot show outline.");
                continue;
            }

            GameObject newOutline = Instantiate(ghostHexOutlinePrefab, gridManager.HexToWorld(coord), Quaternion.identity, this.transform);

            if (newOutline == null)
            {
                Debug.LogError($"Instantiate(ghostHexOutlinePrefab) returned null for {coord}! This should not happen if Prefab is assigned.");
                continue;
            }

            newOutline.transform.localScale = Vector3.one * gridManager.TileSize;
            activeHexOutlines.Add(newOutline);

            MeshRenderer renderer = newOutline.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                Debug.LogError($"The instantiated ghostHexOutlinePrefab ('{ghostHexOutlinePrefab.name}') for {coord} is missing a MeshRenderer component! Please add one to the Prefab.");
            }
            else
            {
                renderer.material = canPlace ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;
                // �߿�Ĳ��ʺͻ��ģʽ���ֲ��䣬��Ϊ����ϣ���߿��ǰ�͸����
                SetMaterialBlendMode(renderer.material, MaterialBlendMode.Fade);
            }
        }
    }

    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)
        {
            if (outline != null)
            {
                Destroy(outline);
            }
        }
        activeHexOutlines.Clear();
    }

    private void SetMaterialBlendMode(Material material, MaterialBlendMode blendMode)
    {
        // ��������Shader���Ե�Ĭ��״̬���ٸ���blendMode����
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1; // �ָ�Ĭ����Ⱦ����

        switch (blendMode)
        {
            case MaterialBlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                // ��͸�����ʵ�Ĭ�������Ѿ�����
                break;
            case MaterialBlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int)RenderQueue.AlphaTest;
                break;
            case MaterialBlendMode.Fade: // ���ڵ��뵭��͸����
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0); // ��д����ȣ��Ա�͸������ȷ��Ⱦ
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)RenderQueue.Transparent;
                break;
            case MaterialBlendMode.Transparent: // ����Ԥ�� Alpha
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)RenderQueue.Transparent;
                break;
        }
    }
}