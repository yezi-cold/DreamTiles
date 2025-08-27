using System.Collections.Generic;
using System.Linq;
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

    //����׷������ؿ� 
    private bool riverPlaced = false; // �Ƿ��Ѿ����ù������ؿ�
    private bool roadPlaced = false;  // �Ƿ��Ѿ����ù���·�ؿ�

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
        // --- �ֲ����� ---
        Vector3 worldPosition = HexToWorld(coord); // ����������������ת��ΪUnity�������ꡣ
        Quaternion worldRotation = Quaternion.Euler(0, rotationIndex * 60, 0); // ������ת��������������ת�Ƕȡ�

        //ʵ�����ؿ�Ԥ���壬����һ���µ���Ϸ����,�������ɵĵؿ������ΪGridManager������Ӷ��󣬱��ڳ�������
        GameObject newTileGO = Instantiate(tileData.tilePrefab, worldPosition, worldRotation, this.transform);

        // ��ȡ�µؿ��ϵ� TileController ������������� Initialize ����
        TileController newTileController = newTileGO.GetComponent<TileController>();

        if (tileData.tilePrefab == null)//���ؿ��������Ƿ��й�����Ԥ���塣
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;//���û�У���ӡ���󷵻�null
        }

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

        // �ڷ��óɹ��󣬸�������ؿ��׷��״̬ 
        if (!riverPlaced && tileData.edges.Contains(EdgeType.River))
        {
            riverPlaced = true;
            Debug.Log("��һ�������ѱ�����!");
        }
        if (!roadPlaced && tileData.edges.Contains(EdgeType.Road))
        {
            roadPlaced = true;
            Debug.Log("��һ����·�ѱ�����!");
        }
        // ������ �������������µļƷַ��� ������
        CalculateAndAddScores(coord, tileData, rotationIndex);

        return newTileController;
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
        // ��������1����ǰλ��û�еؿ�
        if (HasTileAt(targetCoord))
        {
            return false;
        }

        // ��������2�����ڴ������еؿ�
        bool hasNeighbor = false;
        for (int i = 0; i < 6; i++)
        {
            if (HasTileAt(targetCoord.GetNeighbor(i)))
            {
                hasNeighbor = true;
                break;
            }
        }
        // �������Ϸ�տ�ʼ��ֻ�г�ʼ�ؿ飩����������ԭ���Ա�
        // ����Ѿ��кܶ�ؿ��ˣ�����ƾ�շ���
        if (!hasNeighbor && grid.Count > 1)
        {
            return false;
        }
        // �����һ������ų�ʼ�ؿ�����
        if (grid.Count == 1 && !hasNeighbor)
        {
            return false;
        }


        // --- ����ؿ����Ҫ�� ---
        bool hasRiverEdge = tileToPlace.edges.Contains(EdgeType.River);
        bool hasRoadEdge = tileToPlace.edges.Contains(EdgeType.Road);

        // �������ͨ�ؿ飬ֻҪ���������������
        if (!hasRiverEdge && !hasRoadEdge)
        {
            return true;
        }

        // ���������ؿ飬����Ƿ��ǵ�һ�γ���
        if (hasRiverEdge && !riverPlaced)
        {
            return true; // ��һ�������ؿ飬�����������
        }
        if (hasRoadEdge && !roadPlaced)
        {
            return true; // ��һ����·�ؿ飬�����������
        }

        // ����ǵڶ��μ��Ժ���ֵ�����ؿ飬�����ҵ�ƥ����ھ�
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);
                EdgeType newTileEdge = tileToPlace.edges[i];
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeTypeInWorld((HexDirection)i);

                // ����������
                if (hasRiverEdge && newTileEdge == EdgeType.River && neighborOppositeEdge == EdgeType.River)
                {
                    return true; // �ҵ���һ����Ч�ĺ������ӵ�
                }
                // ����·����
                if (hasRoadEdge && newTileEdge == EdgeType.Road && neighborOppositeEdge == EdgeType.Road)
                {
                    return true; // �ҵ���һ����Ч�ĵ�·���ӵ�
                }
            }
        }

        // �����������ھӣ���û���ҵ���������ӵ�
        return false;
    }

    // -- ˽�и������� --
    // ������ ����������������������Ĺ�����㲢��ӷ��� ������
    private void CalculateAndAddScores(HexCoord placedCoord, TileData placedTileData, int rotationIndex)
    {
        //������ʱ�ķ�����������ʼ��Ϊ0
        int prosperityDelta = 0;
        int populationDelta = 0;
        int happinessDelta = 0;

        // ����6��������ھӣ����ÿ���Ӵ��ߵ�����
        for (int i = 0; i < 6; i++)
        {
            //��ȡ��ǰ�����ھ�����
            HexCoord neighborCoord = placedCoord.GetNeighbor(i);
            //�����������Ƿ����ھ�
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);

                // ��ȡ�µؿ��ڵ�ǰ���緽��(i)�ı�Ե����
                EdgeType newTileEdge = placedTileData.GetEdgeForWorldDirection(i, rotationIndex);

                // ��ȡ�ھӵؿ������ǽӴ��������ߵı�Ե����
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeTypeInWorld((HexDirection)i);

                // ��ѯScoreManager�Ĺ������ȡ��Ա�Ե���ӵķ���
                ScoreModifier modifier = ScoreManager.Instance.GetAdjacencyBonus(newTileEdge, neighborOppositeEdge);

                // �ۼӴӹ����л�õķ���
                prosperityDelta += modifier.prosperity;
                populationDelta += modifier.population;
                happinessDelta += modifier.happiness;
            }
        }

        // ���������������������ӵ�ScoreManager
        ScoreManager.Instance.AddScores(prosperityDelta, populationDelta, happinessDelta);

        // ֪ͨGameManager�ؿ��ѷ��ã�����ֻ���𴥷�����
        GameManager.Instance.OnTilePlaced();
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
    //��ȡ��ת�����
    public TileData GetRotatedTileData(TileData originalTileData, int rotation)
    {
        TileData rotatedData = ScriptableObject.CreateInstance<TileData>();//��¡������
        rotatedData.name = originalTileData.name + "_Rotated";//����ת�����һ���µ�����
        rotatedData.tilePrefab = originalTileData.tilePrefab;//������ת����Ƶ�Ԥ����
        rotatedData.edgeType = originalTileData.edgeType;//������ת����Ƶ�����

        const int NUM_HEX_DIRECTIONS = 6;
        rotatedData.edges = new EdgeType[NUM_HEX_DIRECTIONS];//������ת����Ƶı�����
        for (int i = 0; i < NUM_HEX_DIRECTIONS; i++)//������������
        {
            int originalIndex = (i - rotation + NUM_HEX_DIRECTIONS) % NUM_HEX_DIRECTIONS;//����ԭʼ�ߵ�����
            rotatedData.edges[i] = originalTileData.edges[originalIndex];//������ת��ı�����
        }
        return rotatedData;//������ת���������
    }

}