using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridRadius = 4;
    [SerializeField] private float tileSize = 1.0f;
    public float TileSize => tileSize; // <--- **������һ��**
    private GridManager gridManager;

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
        SpawnTile(HexCoord.zero, startTileData, 0); // HexCoord.zero ������֮��Ҫ��ӵģ���ʾ(0,0)
    }

    // �޸� SpawnTile �����Խ��� TileData
    public TileController SpawnTile(HexCoord coord, TileData tileData, int rotationIndex)
    {
        if (tileData.tilePrefab == null)
        {
            Debug.LogError($"Tile Prefab is not set for {tileData.name}!");
            return null;
        }

        Vector3 worldPosition = HexToWorld(coord);
        Quaternion worldRotation = Quaternion.Euler(0, rotationIndex * 60, 0);
        GameObject newTileGO = Instantiate(
            tileData.tilePrefab, // ʹ��TileData�ж����Prefab
            worldPosition,
           worldRotation,
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
        // �ؼ��޸ģ��� Initialize ��������� 'this' (�� GridManager ���������)
        newTileController.Initialize(coord, tileData, this);

        grid.Add(coord, newTileController);
        //***���������㲢��ӷ��� * **
        //if (ScoreManager.Instance != null)
        //{
        //    int matchedEdges = CalculateMatchedEdges(coord, tileData); // �����µĸ�������������ƥ�����
        //    ScoreManager.Instance.ScoreTilePlacement(tileData, matchedEdges);
        //}
        int matchedEdges = CalculateMatchedEdges(coord, tileData);
        GameManager.Instance.OnTilePlaced(coord, tileData, matchedEdges);
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

    public Vector3 HexToWorld(HexCoord coord)
    {
        float x = tileSize * (Mathf.Sqrt(3) * coord.Q + Mathf.Sqrt(3) / 2 * coord.R);
        float z = tileSize * (3.0f / 2.0f * coord.R);

        return new Vector3(x, 0, z);
    }

    public HexCoord WorldToHex(Vector3 worldPosition)
    {
        float q = (worldPosition.x * Mathf.Sqrt(3) / 3 - worldPosition.z / 3) / tileSize;
        float r = (worldPosition.z * 2.0f / 3.0f) / tileSize;

        return HexRound(q, r);
    }

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

    public bool HasTileAt(HexCoord coord)
    {
        return grid.ContainsKey(coord);
    }

    public TileController GetTileAt(HexCoord coord)
    {
        grid.TryGetValue(coord, out TileController tile);
        return tile;
    }

    // ������������ȡ�����ѷ��õؿ������
    public System.Collections.Generic.List<HexCoord> GetAllPlacedTileCoords()
    {
        // ������ʹ����ȷ���ֵ������ 'grid'
        return new System.Collections.Generic.List<HexCoord>(grid.Keys);
    }
    public bool CanPlaceTile(HexCoord targetCoord, TileData tileToPlace)
    {
        // ����1��Ŀ��λ�ò������еؿ�
        if (HasTileAt(targetCoord))
        {
            return false;
        }

        // �����Լ�飺ȷ������� tileToPlace ���� null
        if (tileToPlace == null)
        {
            Debug.LogError("CanPlaceTile ���������ã�������� tileToPlace �ǿյģ����жϲ�����");
            return false;
        }

        // ����2������Ƿ��ǵ�һ���ؿ�
        var allPlacedTiles = GetAllPlacedTileCoords();
        if (allPlacedTiles.Count == 0)
        {
            return targetCoord == HexCoord.zero;
        }

        // ����3������Ƿ������еؿ�����
        bool hasAdjacentTile = false;
        for (int i = 0; i < 6; i++)
        {
            if (HasTileAt(targetCoord.GetNeighbor(i)))
            {
                hasAdjacentTile = true;
                break;
            }
        }
        if (!hasAdjacentTile)
        {
            return false;
        }

        // ����4������������ڵı��Ƿ�ƥ��
        for (int i = 0; i < 6; i++)
        {
            HexCoord neighborCoord = targetCoord.GetNeighbor(i);
            if (HasTileAt(neighborCoord))
            {
                TileController neighborTile = GetTileAt(neighborCoord);

                // ---���������������ӽ�׳�Լ�顿---
                if (neighborTile == null)
                {
                    Debug.LogError($"�������ݳ���ì�ܣ����� {neighborCoord} ��ʾ�еؿ飬���޷���ȡ��ʵ����");
                    continue; // ���������������ھ�
                }
                if (neighborTile.TileData == null)
                {
                    Debug.LogError($"λ������ {neighborCoord} �ĵؿ�û�й����� TileData���������ʼ�����̡�");
                    continue; // ���������������ھ�
                }
                // ---����������---

                EdgeType newTileEdge = tileToPlace.edges[i];
                EdgeType neighborOppositeEdge = neighborTile.GetOppositeEdgeType((HexDirection)i);

                if (newTileEdge != EdgeType.None && neighborOppositeEdge != EdgeType.None)
                {
                    if (newTileEdge != neighborOppositeEdge)
                    {
                        return false;
                    }
                }
            }
        }

        // ���й���ͨ��
        return true;
    }
}