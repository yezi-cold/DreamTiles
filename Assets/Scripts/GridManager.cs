using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridRadius = 4;
    [SerializeField] private float tileSize = 1.0f;

    // ����ֱ������hexTilePrefab����������TileData����TileData�������ĸ�Prefab
    [Header("Initial Tile Data")]
    [SerializeField] private TileData startTileData; // ��Ϸ��ʼʱ�ĵ�һ���ؿ�����

    // �洢 TileController ʵ���������� GameObject
    private Dictionary<HexCoord, TileController> grid = new Dictionary<HexCoord, TileController>();

    // ����һ�����ԣ��������ű����Բ�ѯ��������
    public IReadOnlyDictionary<HexCoord, TileController> Grid => grid;

    private void Start()
    {
        if (startTileData == null)
        {
            Debug.LogError("Start Tile Data is not set in the GridManager!");
            return;
        }

        // ��Ϸ��ʼʱ��ֻ������λ������һ����ʼ�ؿ�
        SpawnTile(HexCoord.zero, startTileData); // HexCoord.zero ������֮��Ҫ��ӵģ���ʾ(0,0)
        // GenerateGrid(); // ������Startʱ�����������񣬶��Ƕ�̬����
    }

    // �޸� SpawnTile �����Խ��� TileData
    public TileController SpawnTile(HexCoord coord, TileData tileData)
    {
        if (tileData.tilePrefab == null)
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;
        }

        Vector3 worldPosition = HexToWorld(coord);

        GameObject newTileGO = Instantiate(
            tileData.tilePrefab, // ʹ��TileData�ж����Prefab
            worldPosition,
            Quaternion.identity,
            this.transform
        );

        // ��ȡ����ʼ�� TileController
        TileController newTileController = newTileGO.GetComponent<TileController>();
        if (newTileController == null)
        {
            Debug.LogError("HexTile_Prefab is missing TileController component!");
            Destroy(newTileGO); // ���ٴ����ʵ��
            return null;
        }
        // *** �����޸ģ��� Initialize ��������� 'this' (�� GridManager ���������) ***
        newTileController.Initialize(coord, tileData, this); //

        grid.Add(coord, newTileController);
        // *** ���������㲢��ӷ��� ***
        if (ScoreManager.Instance != null)
        {
            int matchedEdges = CalculateMatchedEdges(coord, tileData); // �����µĸ�������������ƥ�����
            ScoreManager.Instance.ScoreTilePlacement(tileData, matchedEdges);
        }

        return newTileController; // ����ʵ���Ա������ű�ʹ��
    }

    // *** ����������������������õؿ�ʱƥ��ı��� ***
    private int CalculateMatchedEdges(HexCoord placedCoord, TileData placedTileData)
    {
        int matchedCount = 0;
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
                // ֻ�е����߶���Ϊ None ��������ȫһ�²���������ƥ��
                if (newTileEdge != EdgeType.None &&
                    neighborOppositeEdge != EdgeType.None &&
                    newTileEdge == neighborOppositeEdge) // ע�������� ==
                {
                    matchedCount++;
                }
            }
        }
        return matchedCount;
    }

    // ... HexToWorld �������ֲ��� ...
    public Vector3 HexToWorld(HexCoord coord)
    {
        float x = tileSize * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2 * coord.R);
        float z = tileSize * (3.0f / 2.0f * coord.R);

        return new Vector3(x, 0, z);
    }
    // ����ת�����������������굽����������
    public HexCoord WorldToHex(Vector3 worldPosition)
    {
        // ������������ת��������ϵ�ı�׼��ʽ (�����ڼ⳯��������)
        float q = (worldPosition.x * Mathf.Sqrt(3) / 3 - worldPosition.z / 3) / tileSize;
        float r = (worldPosition.z * 2.0f / 3.0f) / tileSize;

        // �����������뵽���������������
        return HexRound(q, r);
    }

    // ��������������������㷨 (������ѧ)
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

    // ���һ�����������ĳ�������Ƿ��Ѿ��еؿ�
    public bool HasTileAt(HexCoord coord)
    {
        return grid.ContainsKey(coord);
    }

    // ���һ����������ȡĳ������ĵؿ������
    public TileController GetTileAt(HexCoord coord)
    {
        grid.TryGetValue(coord, out TileController tile);
        return tile;
    }

    // ��ȡ�����ѷ��õؿ������
    public System.Collections.Generic.List<HexCoord> GetAllPlacedTileCoords()
    {
        return new System.Collections.Generic.List<HexCoord>(grid.Keys);
    }
}