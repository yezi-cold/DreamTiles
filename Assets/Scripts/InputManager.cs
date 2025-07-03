using UnityEngine;
using UnityEngine.Rendering; // ȷ����� using ����

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GridManager gridManager;

    private System.Collections.Generic.List<GameObject> activeHexOutlines = new System.Collections.Generic.List<GameObject>();

    // *** �����������ؿ���صĲ��ʺ�ʵ���ֶ� ***
    private GameObject ghostTileInstance;

    private TileData currentTileToPlace; // ��ǰ�����õĵؿ�����

    // �������� TileDeckManager
    [SerializeField] private TileDeckManager tileDeckManager;

    // ����߿����ɫ��Ȼ��������Ϊ�߿���
    [Header("Ghost Hex Outline Materials")]
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f); // Ĭ����ɫ��͸��
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f); // Ĭ�Ϻ�ɫ��͸��

    private Material hexOutlineValidMaterial;
    private Material hexOutlineInvalidMaterial;


    // ��������ת�ǶȺ͵�ǰ��ת״̬
    private int currentTileRotationIndex = 0; // 0-5����Ӧ HexDirection
    private const int NUM_HEX_DIRECTIONS = 6; // ��������

    [Header("Ghost Hex Outline")]
    [SerializeField]
    private GameObject ghostHexOutlinePrefab; // ������ʾ�յؿ�߿��Prefab


    [Header("Camera Control")] // ������������Ʋ���
    [SerializeField] private float zoomSpeed = 5f; // �����ٶ�
    [SerializeField] private float minZoom = 2f; // ��С����ֵ (���Ŵ�)
    [SerializeField] private float maxZoom = 10f; // �������ֵ (��С�Ŵ�)

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
        // ���������� TileDeckManager
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
        // ����߿���ʵĳ�ʼ��
        hexOutlineValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hexOutlineValidMaterial.color = validHexOutlineColor;

        hexOutlineInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;

        // ���ñ߿���ʵĻ��ģʽΪ��͸����Fade�������Ǳ����
        SetMaterialBlendMode(hexOutlineValidMaterial);
        SetMaterialBlendMode(hexOutlineInvalidMaterial);

        // InputManager ������ Start �г�ʼ�� currentTileToPlace�����ǵȴ� TileDeckManager ֪ͨ
        // TileDeckManager �� Start() ��������� DrawNewTile()��Ȼ�� DrawNewTile() ����� InputManager �� SetCurrentTileToPlace
    }

    void Update()
    {
        // ��������߼�
        HandleCameraZoom();

        // ���߼��
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int groundLayer = LayerMask.GetMask("Ground"); // ȷ�����������ô˲�

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 worldPos = hit.point;
            HexCoord targetCoord = gridManager.WorldToHex(worldPos);

            // *** ������������ؿ�ģ�͵ĸ����߼� ***
            UpdateGhostTile(targetCoord);

            // ��������߿����ʾ������
            HandleGhostHexOutlines();

            // ����м�������ת�ؿ�
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0 && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) // ֻ����û�а�Ctrlʱ����ת
            {
                RotateCurrentTile(scroll > 0); // true for clockwise, false for counter-clockwise
            }

            // ���������
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTile(targetCoord);
            }
        }
        else // ���û����ͣ�ڵ�����
        {
            // *** ������������ؿ�ģ�͵������߼� ***
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
            HideAllHexOutlines(); // ������������߿�
        }
    } // End of Update() method

    // ���������������������
    private void HandleCameraZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        // ֻ�е����� Ctrl ��ʱ�Ž�������
        if (scroll != 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            // �������
            if (gameCamera.orthographic)
            {
                gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
            // ͸�����
            else
            {
                gameCamera.fieldOfView = Mathf.Clamp(gameCamera.fieldOfView - scroll * zoomSpeed, minZoom, maxZoom);
            }
        }
    }

    // *** ������� CreateGhostTile ���� ***
    // �˷������ڽ���������ؿ飬�������޸�����ʣ�ʹ�䱣��ԭʼ��͸��״̬
    private void CreateGhostTile(TileData tileData)
    {
        if (tileData == null || tileData.tilePrefab == null) return;

        // ���پɵ�����ؿ飨����еĻ���
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        ghostTileInstance = Instantiate(tileData.tilePrefab);

        // *** �ؼ��޸ģ�������������ؿ�Ĳ���Ϊ��͸���������� ***
        // ����ؿ齫������ Prefab �����ԭʼ���ʣ��������޸ġ�
        // ��� Prefab ��������ǰ�͸���ģ��������ǰ�͸���ġ�
        // ��� Prefab ��������ǲ�͸���ģ��������ǲ�͸���ġ�
        // ����ֻ����������ʾ�͸�����ꡣ

        // ������TileController��������Ϸ�߼����
        TileController tc = ghostTileInstance.GetComponent<TileController>();
        if (tc != null) tc.enabled = false;
        // ���Խ����������ܲ���Ҫ�����������Collider

        ghostTileInstance.transform.parent = this.transform; // ��ΪInputManager���Ӷ���
        ghostTileInstance.name = "GhostTile";
        ghostTileInstance.SetActive(false); // ��ʼ���أ�Update ����������ʾ
    }

    // *** ������� UpdateGhostTile ���� ***
    // �˷������ڽ���������ؿ��λ�ú���ת���������޸������
    private void UpdateGhostTile(HexCoord targetCoord)
    {
        if (ghostTileInstance == null || currentTileToPlace == null) return;

        ghostTileInstance.SetActive(true);
        ghostTileInstance.transform.position = gridManager.HexToWorld(targetCoord);
        // Ӧ����ת
        ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60f, 0);

        // *** �ؼ��޸ģ������� Update ���޸�����ؿ�Ĳ��� ***
        // ������ Instantiate ʱ���Ѿ�ȷ��������Ҫÿ�θ��¶��������á�
    }


    // ��������ת�ؿ������Խ�����ײ���ͷ���
    private TileData GetRotatedTileData(TileData originalTileData, int rotationIndex)
    {
        if (rotationIndex == 0) return originalTileData; // δ��ת

        // ����һ����ʱ�� TileData �������洢��ת��ı�Ե
        TileData rotatedTileData = ScriptableObject.CreateInstance<TileData>();
        rotatedTileData.name = originalTileData.name + "_Rotated";
        rotatedTileData.tilePrefab = originalTileData.tilePrefab; // Prefab����

        // ��ת��Ե����
        rotatedTileData.edges = new EdgeType[NUM_HEX_DIRECTIONS];
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)
        {
            // ������ת�������
            int rotatedIndex = (i + rotationIndex) % NUM_HEX_DIRECTIONS;
            rotatedTileData.edges[rotatedIndex] = originalTileData.edges[i];
        }
        return rotatedTileData;
    }

    // ��������ת��ǰ���Ƶ��Ӿ�������
    private void RotateCurrentTile(bool clockwise)
    {
        if (currentTileToPlace == null) return; // û�еؿ鲻����ת

        if (clockwise)
        {
            currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        }
        else
        {
            currentTileRotationIndex = (currentTileRotationIndex - 1 + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;
            // ȷ�����������ɸ���
            if (currentTileRotationIndex < 0)
            {
                currentTileRotationIndex += NUM_HEX_DIRECTIONS;
            }
        }

        // ��ת�ؿ�󣬸�������ؿ����ת
        if (ghostTileInstance != null)
        {
            ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60f, 0);
        }

        // ˢ�����е�����߿��Ա����Ƿ�ӳ�µġ��ɷ����ԡ�״̬
        HandleGhostHexOutlines();
    }

    // ���ò��ʵĻ��ģʽΪ��͸�� (������ Hex Outline ����)
    private void SetMaterialBlendMode(Material material)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0); // �ر����д�룬��ֹ��͸�������ڵ�
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON"); // ����͸�����
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent; // ������Ⱦ����Ϊ͸��
        material.SetOverrideTag("RenderType", "Transparent");
    }

    // ���õ�ǰ�����õĵؿ�
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;
        currentTileRotationIndex = 0; // ÿ�γ鵽����ʱ������ת

        // *** ������������ؿ�ʵ���Ĵ����������߼� ***
        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);
            if (ghostTileInstance != null)
            {
                ghostTileInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                ghostTileInstance.SetActive(false); // ��ʼ���أ�Update ����������ʾ
            }
        }
        else // û������
        {
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance);
                ghostTileInstance = null;
            }
            HideAllHexOutlines();
        }

        // �����Ʊ仯ʱ��ˢ�±߿�
        HandleGhostHexOutlines();
    }

    // ���Է��õؿ�
    private void TryPlaceTile(HexCoord targetCoord)
    {
        if (currentTileToPlace == null)
        {
            Debug.LogWarning("No tile to place! Waiting for new tile from deck.");
            return;
        }

        // ʹ����ת��ĵؿ����ݽ��з�����֤
        TileData rotatedTileDataToPlace = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (CanPlaceTile(targetCoord, rotatedTileDataToPlace)) // ������ת��� TileData
        {
            // ������ת��� TileData �� SpawnTile
            gridManager.SpawnTile(targetCoord, rotatedTileDataToPlace);
            Debug.Log($"Tile placed at {targetCoord.Q},{targetCoord.R}");

            // ��������ؿ�
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance);
                ghostTileInstance = null;
            }

            if (tileDeckManager != null)
            {
                tileDeckManager.DrawNewTile(); // ���óɹ�����ȡ����
            }
            else
            {
                currentTileToPlace = null;
                // ���û���ƶѹ������������������꣬�����ر߿�
                HideAllHexOutlines();
            }
        }
        else
        {
            Debug.Log("Cannot place tile here: invalid position or mismatched edges.");
        }
    }

    // �޸� ShowHexOutline �Թ�����ʵ��
    private void ShowHexOutline(HexCoord targetCoord, bool isValidPlacement)
    {
        if (ghostHexOutlinePrefab == null)
        {
            Debug.LogWarning("Hex Outline Prefab is not assigned in InputManager!");
            return;
        }

        // ÿ�ε��� ShowHexOutline ���ᴴ��һ���µ�ʵ��������ӵ��б���
        GameObject newOutline = Instantiate(ghostHexOutlinePrefab);
        newOutline.name = $"HexOutline_{targetCoord.Q}_{targetCoord.R}";
        newOutline.transform.parent = this.transform; // ��ΪInputManager���Ӷ���
        newOutline.transform.position = gridManager.HexToWorld(targetCoord);
        newOutline.SetActive(true);

        MeshRenderer renderer = newOutline.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = newOutline.GetComponentInChildren<MeshRenderer>();
        }
        if (renderer != null)
        {
            // ʹ�ñ߿����
            renderer.material = isValidPlacement ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;
            renderer.material.color = isValidPlacement ? hexOutlineValidMaterial.color : hexOutlineInvalidMaterial.color;
        }
        activeHexOutlines.Add(newOutline); // ���´�����ʵ����ӵ��б���
    }


    // �޸� HideHexOutline Ϊ HideAllHexOutlines
    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)
        {
            if (outline != null)
            {
                Destroy(outline); // �������л�Ծ�ı߿�ʵ��
            }
        }
        activeHexOutlines.Clear(); // ����б�
    }


    // --- ���ķ�����֤�߼������ֲ��䣩 ---
    private bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        // 1. ���Ŀ��λ���Ƿ�Ϊ��
        if (gridManager.HasTileAt(targetCoord))
        {
            return false; // λ���ѱ�ռ��
        }

        // 2. ���Ŀ��λ���Ƿ�������һ���ѷ��õ��ھ� (���ܷ��������)
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
            return false; // ����������һ�����еؿ�����
        }

        // 3. ����������ڵؿ�ı�Ե�Ƿ�ƥ��
        for (int i = 0; i < 6; i++) // ����������������
        {
            HexDirection currentDir = (HexDirection)i;
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);

            // �������������ھ�
            if (gridManager.HasTileAt(neighborCoord))
            {
                TileController neighborTile = gridManager.GetTileAt(neighborCoord);

                // ��ȡ�µؿ����������ı�Ե����
                EdgeType newTileEdge = tileToPlace.edges[(int)currentDir];

                // ��ȡ�ھӵؿ����෴����ı�Ե����
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType(currentDir);

                // ƥ�����:
                // 1. ����µؿ�ı��� None����ǿ��ƥ�䣨����δ���Ŀհױߣ�
                // 2. ����ھӵؿ�ķ������ None����ǿ��ƥ��
                // 3. �������߱�����ȫƥ��
                if (newTileEdge != EdgeType.None &&
                    neighborOppositeEdge != EdgeType.None &&
                    newTileEdge != neighborOppositeEdge)
                {
                    return false; // ��Ե��ƥ��
                }
            }
        }

        return true; // ���м�鶼ͨ�������Է���
    }

    // HandleGhostHexOutlines �������ֲ��䣬��Ϊ��������Ǳ߿򣬶���������ؿ�ģ��
    private void HandleGhostHexOutlines()
    {
        HideAllHexOutlines(); // �����������оɵı߿�

        System.Collections.Generic.HashSet<HexCoord> potentialPlacementCoords = new System.Collections.Generic.HashSet<HexCoord>();

        // ������ĵؿ���Χ�Ŀ�λ��
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighbor = HexCoord.zero.GetNeighbor(i);
            if (!gridManager.HasTileAt(neighbor))
            {
                potentialPlacementCoords.Add(neighbor);
            }
        }

        // ��������ѷ��õؿ���ھӿ�λ��
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


        // ��������Ǳ�ڵķ���λ�ã�����ʾ��������
        foreach (HexCoord coord in potentialPlacementCoords)
        {
            // ������λ���Ƿ���Է��õ�ǰ���ϵ���
            TileData rotatedTileData = null;
            bool canPlace = false;
            if (currentTileToPlace != null)
            {
                rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);
                canPlace = CanPlaceTile(coord, rotatedTileData);
            }

            // ��ʹ����û���ƣ����߲��ܷ��õ�ǰ�ƣ�������Ȼ��ʾ�߿򣬵������ǲ�ͬ����ɫ
            ShowHexOutline(coord, canPlace);
        }
    }
}