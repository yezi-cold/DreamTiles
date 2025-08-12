using UnityEngine;
using System.Collections.Generic;
/*�ؿ�������ű�
 ���幦��: ����InputManager��ָ�����ǰ�����õĵؿ顣�����𴴽��͸��¸������ġ�����ؿ顱Ԥ����
 �Լ������пɷ���λ����ʾ������߿򡱡���ȷ�Ϸ���ʱ����������GridManager����֤�����ɵؿ顣*/
public class TilePlacer : MonoBehaviour
{
    //--�ֶ�--
    [Header("Dependencies")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera gameCamera;

    [Header("Ghost Tile Settings")]
    [SerializeField] private GameObject ghostHexOutlinePrefab;//����߿��Ԥ����
    [SerializeField] private Color validHexOutlineColor = new Color(0, 1, 0, 0.5f);//��Чλ�õı߿���ɫ
    [SerializeField] private Color invalidHexOutlineColor = new Color(1, 0, 0, 0.5f);//��Чλ�õı߿���ɫ

    private TileData currentTileToPlace;//��ǰ�ȴ����õĵؿ�����
    private int currentTileRotationIndex = 0;//��ǰ�ؿ����ת������0-5��
    private GameObject ghostTileInstance;//����ؿ����Ϸ����ʵ��
    private List<GameObject> activeHexOutlines = new List<GameObject>();//�洢��ǰ��ʾ����������߿��ʵ��

    private Material hexOutlineValidMaterial;//������Чλ�õĶ�̬�����Ĳ���
    private Material hexOutlineInvalidMaterial;//������Чλ�õĶ�̬�����Ĳ���

    private const int NUM_HEX_DIRECTIONS = 6;//˽�г�������ʾ�����εķ���������

    void Awake()
    {
        // �Զ���������
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Start()
    {
        //--�ֲ�����--
        //�ҵ���Ŀ�е�Ĭ��lit��ɫ�������ڴ�����͸������
        Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        if (defaultShader == null) defaultShader = Shader.Find("Standard");
        
        //��̬����������Ч����Чλ�õı߿����
        hexOutlineValidMaterial = new Material(defaultShader);
        hexOutlineValidMaterial.color = validHexOutlineColor;
        SetMaterialBlendMode(hexOutlineValidMaterial, MaterialBlendMode.Fade); // ����Ϊ��͸��ģʽ��

        hexOutlineInvalidMaterial = new Material(defaultShader);
        hexOutlineInvalidMaterial.color = invalidHexOutlineColor;
        SetMaterialBlendMode(hexOutlineInvalidMaterial, MaterialBlendMode.Fade);
    }

    void Update()
    {
        // �����������
        if (currentTileToPlace != null)
        {
            UpdateGhostPreviews();//ÿ֡��������ؿ������߿��λ�ü�״̬
        }
    }

    //--��������--
    // �������������inputmanager���ã�
    public void HandlePlacement(Vector3 mousePosition)
    {
        Ray ray = gameCamera.ScreenPointToRay(mousePosition);//������������λ�÷���һ������
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground"))) // �������������"Ground"ͼ������塣
        {
            HexCoord clickedHexCoord = gridManager.WorldToHex(hit.point);// ����ײ�����������ת��Ϊ�������ꡣ
            TryPlaceTile(clickedHexCoord);//�����ڸ�����������õؿ�
        }
    }

    //������ת������inputmanager���ã�
    public void HandleRotation()
    {
        // ��ת������1������ȡ������ȷ������0-5֮��ѭ����
        currentTileRotationIndex = (currentTileRotationIndex + 1) % NUM_HEX_DIRECTIONS;
        // ��ת�����̸���Ԥ��
        UpdateGhostPreviews();
    }

    //���õ�ǰҪ���õĵؿ飨��GridManager�ڳ��Ƶ��ã�
    public void SetCurrentTileToPlace(TileData tileData)
    {
        currentTileToPlace = tileData;//���µ�ǰ����

        // ���پɵ�����ؿ�
        if (ghostTileInstance != null)
        {
            Destroy(ghostTileInstance);
        }

        if (currentTileToPlace != null)
        {
            CreateGhostTile(tileData);//�����µ�����ؿ�
            ghostTileInstance.SetActive(true);//������
        }
        else
        {
            HideAllHexOutlines();// ���û�еؿ��ˣ��Ƴ��꣩���������б߿�
        }

        UpdateGhostPreviews();// �õ����ƺ���������һ��Ԥ����
    }

    //--˽�з���--
    private void UpdateGhostPreviews()//����������ʾ״̬
    {
        HandleGhostTile();//��������ؿ�������
        HandleGhostHexOutlines();//������Ǳ�ڵĿɷ���λ����ʾ�߿򣬲�����gridmanager��CanPlaceTile���������ɫ��
    }

    #region Ghost Tile Logic
    //��������ؿ����ʾ�͸���
    private void HandleGhostTile()
    {
        if (ghostTileInstance == null) return;//���û������ؿ飬����null���Ͳ��ø�������

        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition); // Ghost Tile ���Ǹ����������λ��
        //������������˵��棬�ͽ�����ؿ��λ�ú���ת���óɸ�������λ�ú���ת
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            HexCoord mouseHexCoord = gridManager.WorldToHex(hit.point);//����ײ�����������ת��Ϊ�������ꡣ
            Vector3 worldPos = gridManager.HexToWorld(mouseHexCoord);//����������ת��Ϊ�������ꡣ
            ghostTileInstance.transform.position = worldPos;//��������ؿ��λ�á�
            ghostTileInstance.transform.rotation = Quaternion.Euler(0, currentTileRotationIndex * 60, 0);//��������ؿ����ת��
            ghostTileInstance.SetActive(true);//��������
        }
        else
        {
            ghostTileInstance.SetActive(false);//�������������
        }
    }

    //����һ�����Ӿ��ģ�û����ײ�ͽű��ĵؿ鸱��
    private void CreateGhostTile(TileData tileData)
    {
        ghostTileInstance = Instantiate(tileData.tilePrefab);//��¡�ؿ�Ԥ����
        // �������������ֻ�����������Ⱦ��
        foreach (var component in ghostTileInstance.GetComponents<MonoBehaviour>()) Destroy(component);
        // ����������ײ��
        foreach (var collider in ghostTileInstance.GetComponents<Collider>()) Destroy(collider);
        // ���ò㼶
        ghostTileInstance.layer = LayerMask.NameToLayer("Ignore Raycast");
        // ������
        ghostTileInstance.SetActive(false);
    }
    #endregion

    #region Ghost Outline Logic
    //��������߿����ʾ�͸���
    private void HandleGhostHexOutlines()
    {
        HideAllHexOutlines();//�������б߿�

        if (currentTileToPlace == null) return;//���û���ƣ��Ͳ�����ʾ�߿�

        HashSet<HexCoord> potentialCoords = GetPotentialPlacementCoords();//��ȡ����Ǳ�ڵķ���λ��
        
        
        foreach (HexCoord coord in potentialCoords)//��������Ǳ�ڵķ���λ��
        {
            TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);//��ȡ��ת�����
            bool canPlace = gridManager.CanPlaceTile(coord, rotatedTileData);//��������������Ĺ����ж��Ƿ���Է���
            //��¡�߿�Ԥ����
            GameObject outline = Instantiate(ghostHexOutlinePrefab, gridManager.HexToWorld(coord), Quaternion.identity, this.transform);
           
            outline.transform.localScale = Vector3.one * gridManager.TileSize;//���ñ߿�Ĵ�С

            MeshRenderer renderer = outline.GetComponent<MeshRenderer>();//��ȡ�߿����Ⱦ��
           
            renderer.material = canPlace ? hexOutlineValidMaterial : hexOutlineInvalidMaterial;//���ñ߿�Ĳ���

            activeHexOutlines.Add(outline);//��ӵ�����߿��б���
        }
    }

    //��ȡ����Ǳ�ڵķ���λ��
    private HashSet<HexCoord> GetPotentialPlacementCoords()
    {
        //����һ��HashSet���洢Ǳ�ڵķ���λ��
        HashSet<HexCoord> potentialCoords = new HashSet<HexCoord>();
        //�����û�з����κ��ƣ����������λ��
        if (gridManager.GetAllPlacedTileCoords().Count == 0)
        {
            potentialCoords.Add(HexCoord.Zero);//�������λ��
        }
        else
        {
            //���������Ѿ����õ���
            foreach (var placedCoord in gridManager.GetAllPlacedTileCoords())
            {
                for (int i = 0; i < 6; i++)//������������
                {
                    HexCoord neighbor = placedCoord.GetNeighbor(i);//��ȡ����λ��
                    //�������λ��û���ƣ�����ӵ�Ǳ��λ���б���
                    if (!gridManager.HasTileAt(neighbor))
                    {
                        potentialCoords.Add(neighbor);
                    }
                }
            }
        }
        return potentialCoords;//����Ǳ�ڵķ���λ���б�
    }

    //�������б߿�
    private void HideAllHexOutlines()
    {
        foreach (GameObject outline in activeHexOutlines)//��������߿��б�
        {
            if (outline != null) Destroy(outline);//����߿�ʵ����Ϊ�գ�����������
        }
        activeHexOutlines.Clear();//��ռ���ı߿��б�
    }
    #endregion

    #region Placement and Rule Logic

    //������Ŀ���������������
    private void TryPlaceTile(HexCoord targetCoord)
    {
        //��ȡ��ת�����
        TileData rotatedTileData = GetRotatedTileData(currentTileToPlace, currentTileRotationIndex);

        if (gridManager.CanPlaceTile(targetCoord, rotatedTileData))//������������������ã��ͷ�����
        {
            gridManager.SpawnTile(targetCoord, rotatedTileData, currentTileRotationIndex);//���ɵؿ�
        }
        else
        {
            Debug.Log("Cannot place tile here.");
        }
    }

    //��ȡ��ת�����
    private TileData GetRotatedTileData(TileData originalTileData, int rotation)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();//��¡������
        rotatedData.name = originalTileData.name + "_Rotated";//����ת�����һ���µ�����
        rotatedData.tilePrefab = originalTileData.tilePrefab;//������ת����Ƶ�Ԥ����
        rotatedData.tileType = originalTileData.tileType;//������ת����Ƶ�����
        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];//������ת����Ƶı�����
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)//������������
        {
            int originalIndex = (i - rotation + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;//����ԭʼ�ߵ�����
            rotatedData.edges[i] = originalTileData.edges[originalIndex];//������ת��ı�����
        }
        return rotatedData;//������ת���������
    }
    #endregion

    #region Material Blending Utility
    // ������߷������Ա�������Ϊ����߿����Ⱦ�������
    public enum MaterialBlendMode { Opaque, Cutout, Fade, Transparent }// ���ʻ��ģʽö��

    //���ò��ʵĻ��ģʽ
    private void SetMaterialBlendMode(Material material, MaterialBlendMode blendMode)
    {
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);//Դ���ģʽ
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);//Ŀ����ģʽ
        material.SetInt("_ZWrite", 1);//���д��
        material.DisableKeyword("_ALPHATEST_ON");//�ر�͸���Ȳ���
        material.DisableKeyword("_ALPHABLEND_ON");//�ر�͸�����
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");// �ر�Ԥ�˻��
        material.renderQueue = -1;//��Ⱦ����

        switch (blendMode)//���ݻ��ģʽ���ò�������
        {
            case MaterialBlendMode.Fade://��͸��
                material.SetOverrideTag("RenderType", "Transparent");//��Ⱦ����
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//Դ���ģʽ
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//Ŀ����ģʽ
                material.SetInt("_ZWrite", 0);//�ر����д��
                material.EnableKeyword("_ALPHABLEND_ON");//����͸�����
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;//��Ⱦ����
                break;
                // ... ����ģʽ ...
        }
    }
    #endregion
}