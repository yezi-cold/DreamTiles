using System.Collections.Generic;
using UnityEngine;
/*���������
����Ϸ�ĺ����߼�֮һ����������������еؿ��λ�á����ɺ����ݴ洢��
���幦��: ������ά��һ��������������ֵ����ݽṹ�������𽫵ؿ��������ȷ������λ�ã�
���������꣨�����λ�ã�ת��Ϊ�������꣬����������֤�ؿ��ܷ���õĺ��Ĺ���*/
public class GridManager : MonoBehaviour
{
    //--�ֶ�--

    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1.0f;//˽���ֶΣ����嵥�������εĴ�С
                                                 
    [Header("Initial Tile Data")]
    [SerializeField] private TileData startTileData; // ˽���ֶΣ������Ϸ��ʼʱ������������ʼ�ؿ�����ݡ�

    //˽���ֵ䣬��������ĺ������ݽṹ
    //���������ε����ֵ꣬�Ƕ�Ӧ�����ϵ� TileController ʵ��
    private Dictionary<HexCoord, TileController> grid = new Dictionary<HexCoord, TileController>();

    //--����--
    public float TileSize => tileSize; // ����ֻ�����ԣ��������ű����԰�ȫ�ػ�ȡtileSize��ֵ��
    public IReadOnlyDictionary<HexCoord, TileController> Grid => grid;//����ֻ�����ԣ���˽�е�grid�ֵ���ֻ������ʽ��¶�������ű���
   
    //--unity�������ں���--
    private void Start()
    {
        if (startTileData == null)//�����ʼλ���Ƿ������˵ؿ�
        {
            Debug.LogError("Start Tile Data is not set in the GridManager!");
            return;//��ǰ�˳���������ֹ�����������
        }

        // ��Ϸ��ʼʱ�����������ģ�0��0�����ɳ�ʼ�ؿ飬��ʼ��תΪ0
        SpawnTile(HexCoord.Zero, startTileData, 0); // HexCoord.zero ������֮��Ҫ��ӵģ���ʾ(0,0)
    }
    //--��������--
    //��ָ����������һ���ؿ顣
    public TileController SpawnTile(HexCoord coord, TileData tileData, int rotationIndex)
    {
        if (tileData.tilePrefab == null)//���ؿ��������Ƿ��й�����Ԥ���塣
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;//���û�У���ӡ���󷵻�null
        }
        // --- �ֲ����� ---
        Vector3 worldPosition = HexToWorld(coord); // ����������������ת��ΪUnity�������ꡣ
        Quaternion worldRotation = Quaternion.Euler(0, rotationIndex * 60, 0); // ������ת��������������ת�Ƕȡ�

        //ʵ�����ؿ�Ԥ���壬����һ���µ���Ϸ����,�������ɵĵؿ������ΪGridManager������Ӷ��󣬱��ڳ�������
        GameObject newTileGO = Instantiate(tileData.tilePrefab, worldPosition, worldRotation, this.transform);

        // ��ȡ�µؿ��ϵ� TileController ������������� Initialize ����
        TileController newTileController = newTileGO.GetComponent<TileController>();
        if (newTileController == null)
        {
            Debug.LogError("HexTile_Prefab is missing TileController component!");
            Destroy(newTileGO); // ���ٴ����ʵ��
            return null;
        }

        // �����µؿ�� Initialize �����������ꡢ���ݡ��������������Ϣ���ݸ�����
        newTileController.Initialize(coord, tileData, this);

        //�������ɵĵؿ���ӵ������ֵ���
        grid.Add(coord, newTileController);

        //����calculateMatchedEdges���������µؿ����ھ�ƥ��ı�����
        int matchedEdges = CalculateMatchedEdges(coord, tileData);

        // ֪ͨGameManager�ؿ��ѷ��ã������������Ϣ��GameManager�Ḻ������ļƷֺͳ������̡�
        GameManager.Instance.OnTilePlaced(coord, tileData, matchedEdges);
        return newTileController; // �����´����ĵؿ������ʵ����
    }

    //������������ת��Ϊunity��������
    public Vector3 HexToWorld(HexCoord coord)
    {
        //�����������������ѧ��ʽ��������ת����
        float x = tileSize * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2 * coord.R);
        float z = tileSize * (3.0f / 2.0f * coord.R);
        return new Vector3(x, 0, z);//���ؼ������3d��������
    }
    //��unity��������ת��Ϊ����������
    public HexCoord WorldToHex(Vector3 worldPosition)
    {
        //�����������������ѧ��ʽ��������ת����
        float q = (worldPosition.x * Mathf.Sqrt(3) / 3 - worldPosition.z / 3) / tileSize;
        float r = (worldPosition.z * 2.0f / 3.0f) / tileSize;
        return HexRound(q, r);//�Ը�����������������룬�õ����յ������������ꡣ
    }

    //���ָ�������Ƿ���ڵؿ�
    public bool HasTileAt(HexCoord coord)
    {
        return grid.ContainsKey(coord);//����ֵ����Ƿ���ָ������ļ���
    }

    //��ȡָ������ĵؿ������
    public TileController GetTileAt(HexCoord coord)
    {
        //���Դ��ֵ��л�ȡֵ
        grid.TryGetValue(coord, out TileController tile);
        //�����ҵ��ؿ�����������û���򷵻�null
        return tile;
    }
    
    //��ȡ�����ѷ��õؿ�������б�
    public List<HexCoord> GetAllPlacedTileCoords()
    {
        //����һ������grid�ֵ����м������꣩�����б�
        return new List<HexCoord>(grid.Keys);
    }

    //���ؿ��Ƿ���Է�����Ŀ������ĺ����߼�
    public bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        // ����1��Ŀ��λ�ò������еؿ�
        if (HasTileAt(targetCoord)) return false;

        // ����2������ǵ�һ���ؿ飬�������ԭ�� (0,0)
        if (GetAllPlacedTileCoords().Count == 0)
        {
            return targetCoord == HexCoord.zero;
        }

        // ����3��������һ���ѷ��õĵؿ�����
        bool hasAdjacentTile = false;
        for (int i = 0; i < 6; i++)
        {
            if (HasTileAt(targetCoord.GetNeighbor(i)))
            {
                hasAdjacentTile = true;
                break; // ֻҪ�ҵ�һ���ھӾ���������������ѭ����
            }
        }
        if (!hasAdjacentTile) return false;

        // ����4���������ڵı߱���ƥ��
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);
                EdgeType newTileEdge = tileToPlace.edges[i];
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType((HexDirection)i);

                if (newTileEdge != EdgeType.None && neighborOppositeEdge != EdgeType.None)
                {
                    if (newTileEdge != neighborOppositeEdge) return false; // ������߶��ж��嵫��ƥ�䣬���ܷ��á�
                }
            }
        }
        return true; // ���й���ͨ��������true��
    }

    // -- ˽�и������� --
    private int CalculateMatchedEdges(HexCoord placedCoord, TileData placedTileData)
    {
        int matchedCount = 0;//�ֲ����������ڼ���
        for (int i = 0; i < 6; i++)
        {
            HexDirection currentDir = (HexDirection)i;
            HexCoord neighborCoord = placedCoord.GetNeighbor(i);

            // �����������Ƿ����ھ�
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);

                // ��ȡ�µؿ����������ı�Ե����
                EdgeType newTileEdge = placedTileData.edges[(int)currentDir];

                // ��ȡ�ھӵؿ����෴����ı�Ե����
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType(currentDir);

                // �����Եƥ�� (ע��������߼��� CanPlaceTile ���в�ͬ��CanPlaceTile ���ϸ�ģ������Ǽ���ƥ������)
                // ֻ�е�����������ȫһ���Ҷ��ж��壬����������ƥ��
                if (newTileEdge != EdgeType.None && neighborOppositeEdge != EdgeType.None &&
                    newTileEdge == neighborOppositeEdge) // ע�������� ==
                {
                    matchedCount++;//ƥ������1
                }
            }
        }
        return matchedCount;
    }

    //�Ը�����������������������룬�õ��������ꡣ
    private HexCoord HexRound(float q, float r)
    {
        float s = -q - r; // s ����

        int rx = Mathf.RoundToInt(q);
        int ry = Mathf.RoundToInt(r);
        int rz = Mathf.RoundToInt(s);

        float x_diff = Mathf.Abs(rx - q);
        float y_diff = Mathf.Abs(ry - r);
        float z_diff = Mathf.Abs(rz - s);

        if (x_diff > y_diff && x_diff > z_diff)
        {
            rx = -ry - rz;
        }
        else if (y_diff > z_diff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new HexCoord(rx, ry);
    }


    
}