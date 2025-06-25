using UnityEngine;
using UnityEngine.Rendering; // ȷ����� using ����

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GridManager gridManager;
    // [SerializeField] private InputManager inputManager; // *** �Ƴ����У����Ƕ���� ***

    private TileData currentTileToPlace; // ��ǰ�����õĵؿ�����

    // �������� TileDeckManager
    [SerializeField] private TileDeckManager tileDeckManager;


    // ����ؿ����
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
        ghostTileValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ghostTileValidMaterial.color = new Color(0, 1, 0, 0.5f);

        ghostTileInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ghostTileInvalidMaterial.color = new Color(1, 0, 0, 0.5f);

        SetMaterialBlendMode(ghostTileValidMaterial);
        SetMaterialBlendMode(ghostTileInvalidMaterial);

        // InputManager ������ Start �г�ʼ�� currentTileToPlace�����ǵȴ� TileDeckManager ֪ͨ
        // TileDeckManager �� Start() ��������� DrawNewTile()��Ȼ�� DrawNewTile() ����� InputManager �� SetCurrentTileToPlace
    }

    void Update()
    {
        // ���߼��
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        int groundLayer = LayerMask.GetMask("Ground"); // ȷ�����������ô˲�

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 worldPos = hit.point;
            HexCoord targetCoord = gridManager.WorldToHex(worldPos);

            // ��������ؿ��λ�ú�״̬
            UpdateGhostTile(targetCoord);

            // ���������
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceTile(targetCoord);
            }
        }
        else
        {
            // ���û����ͣ�ڵ����ϣ���������ؿ�
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(false);
            }
        }
    }

    // ��������ؿ�ʵ��
    private void CreateGhostTile(TileData tileData)
    {
        if (tileData == null || tileData.tilePrefab == null) return;

        // ���پɵ�����ؿ飨����еĻ���
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        ghostTileInstance = Instantiate(tileData.tilePrefab);
        // ȷ������ؿ����Ⱦ���ڳ����пɼ����������ò���
        MeshRenderer renderer = ghostTileInstance.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            // ���Prefab�ĸ�����û��MeshRenderer��������Ӷ���
            renderer = ghostTileInstance.GetComponentInChildren<MeshRenderer>();
        }

        if (renderer != null)
        {
            // ����Ϊ��͸��ģʽ
            // SetMaterialBlendMode(ghostTileValidMaterial); // ��Щ�� Start ���Ѿ����ù��ˣ�����Ҫÿ�δ���������
            // SetMaterialBlendMode(ghostTileInvalidMaterial); // ������ÿ�ζ������µĲ��ʣ�������Ҫ
            renderer.material = ghostTileValidMaterial; // ��ʼ����Ϊ��Ч����
        }
        else
        {
            Debug.LogWarning("Ghost Tile Prefab is missing a MeshRenderer component!");
        }

        // ������TileController��������Ϸ�߼����
        TileController tc = ghostTileInstance.GetComponent<TileController>();
        if (tc != null) tc.enabled = false;
        // ���Խ����������ܲ���Ҫ�����������Collider

        ghostTileInstance.transform.parent = this.transform; // ��ΪInputManager���Ӷ���
        ghostTileInstance.name = "GhostTile";
        // ghostTileInstance.SetActive(false); // ��ʼ���أ�UpdateGhostTile �ᴦ������ʾ
    }

    // ��������ؿ��λ�úͲ���
    private void UpdateGhostTile(HexCoord targetCoord)
    {
        if (ghostTileInstance == null || currentTileToPlace == null) return;

        ghostTileInstance.SetActive(true);
        ghostTileInstance.transform.position = gridManager.HexToWorld(targetCoord);

        // �������Ƿ���Ч
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

    // ���ò��ʵĻ��ģʽΪFade (��͸��)
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

    // ���õ�ǰ�����õĵؿ�
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;
        if (currentTileToPlace != null) // ֻ�е�����ʱ�Ŵ�������ؿ�
        {
            CreateGhostTile(tileData);
            if (ghostTileInstance != null)
            {
                ghostTileInstance.SetActive(true); // ȷ������ؿ�ɼ�
            }
        }
        else // û�����ˣ���������ؿ�
        {
            if (ghostTileInstance != null)
            {
                Destroy(ghostTileInstance); // �ƶѿ��ˣ�ֱ����������ؿ�
                ghostTileInstance = null; // �������
            }
        }
    }

    // ���Է��õؿ�
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

            // *** �����޸ģ����óɹ������ƶѳ�ȡ��һ���� ***
            if (tileDeckManager != null)
            {
                tileDeckManager.DrawNewTile();
            }
            else
            {
                // ���û���ƶѹ�����������յ�ǰ�ƣ������޷���������
                currentTileToPlace = null; // *** ������ʹ�� currentTileToPlace ***
                if (ghostTileInstance != null) // ���ú���������ؿ�
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
}