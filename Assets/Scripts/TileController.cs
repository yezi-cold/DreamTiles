using UnityEngine;

public class TileController : MonoBehaviour
{
    public HexCoord myCoord { get; private set; } // �õؿ������������
    public TileData myTileData { get; private set; } // �õؿ�����ݶ���

    // ��ʼ���ؿ�
    public void Initialize(HexCoord coord, TileData tileData)
    {
        myCoord = coord;
        myTileData = tileData;
        gameObject.name = $"Tile ({coord.Q}, {coord.R}) - {tileData.name}"; // �������

        // δ���������������tileData�����ؿ���Ӿ���ۣ�����ʡ��Ӷ���
        // ���磺GetComponent<MeshRenderer>().material.color = tileData.displayColor;
    }

    // ��ȡ�ض�����ı�Ե����
    public EdgeType GetEdgeType(HexDirection direction)
    {
        return myTileData.edges[(int)direction];
    }

    // ��ȡ������ı�Ե���ͣ�����ƥ�䣩
    // ����һ�����ĺ����������ж��������ڵؿ�ı��Ƿ�ƥ��
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        // �����εķ������� (direction + 3) % 6
        return myTileData.edges[((int)direction + 3) % 6];
    }
}