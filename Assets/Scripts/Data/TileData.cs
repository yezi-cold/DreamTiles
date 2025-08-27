using UnityEngine;

//�ؿ����ݽű�
//��������Ϸ��ÿһ�ֵؿ���������ԣ���ۣ����ͣ��������ߵ����ͣ�����ʹ����scriptableobject���ɱ�д�ű����󣩣����Դ���������Դ�ļ���
public enum EdgeType//���� �ߵ�����
{
    None = 0,//�հ׻���δ����
    Grass = 1,//�ݵ�
    Park = 2,//��԰
    River = 3,//��
    Residential = 4,//סլ
    Industrial = 5,//��ҵ
    Road = 6,//��·
}

//public enum TileType//����ؿ����������
//{
//    None,
//    Park,
//    River,
//    Residential,
//    Industrial,
//}

//--�ඨ��--
//[CreateAssetMenu] ������Կ�����unity�ġ�assets/create���˵�����ʾ��������Դ�ļ���ѡ�
[CreateAssetMenu(fileName = "NewTileData", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject//�̳���ScriptableObject����ʾ����һ�����Դ�������Դ�ļ�
{
    //--�ֶ�--
    //��Щ�����ֶλ�ֱ����ʾ��unity��Inspector����ϣ���������Դ�ļ���������á�
    [Header("Visuals")]//�����Կ�����inspector��������һ�����⣬���ڻ����ֶ�
    public GameObject tilePrefab; // �����ֶ����ڴ�Ÿõؿ��Ӧ��prefab

    //[Header("Gameplay Properties")]
    //public TileType tileType; // �����ֶΣ���������ؿ�����ͣ����������ö�٣�

    [Header("Edges")]
    //[tooltip]���������������ͣ���ֶ���ʱ��ʾһ����ʾ��Ϣ��

    [Tooltip("������������ı�Ե���ͣ�˳����HexDirectionö�ٶ�Ӧ��Right, UpRight, UpLeft, Left, DownLeft, DownRight")]
    public EdgeType[] edges = new EdgeType[6]; // ���������ֶΣ��洢�����ߵ����͡������˳���Ӧ HexDirection ö�ٵ�˳��
    internal EdgeType edgeType;
    /// <summary>
    /// ��ȡ�ؿ���ĳ�������緽���ϵı�Ե���ͣ��ῼ�ǵؿ��������ת��
    /// </summary>
    /// <param name="worldDirection">���緽������� (0=��, 1=����, ...)</param>
    /// <param name="rotationIndex">�ؿ����ת���� (0-5)</param>
    /// <returns>��Ӧ���緽���ϵı�Ե����</returns>
    public EdgeType GetEdgeForWorldDirection(int worldDirection, int rotationIndex)
    {
        // �����߼������緽���ȥ��ת�����͵õ��˵ؿ�����ı��ر�Ե������
        // ���磺����֪����ת��1��(60��)�ĵؿ飬�����緽���ҡ�(����0)��ʲô�ߡ�
        // ��������� (0 - 1) = -1��
        // Ϊ�˴�����������+6��ȡ�࣬(-1 + 6) % 6 = 5��
        // ��������Ӧ��ȥ���ҵؿ���������������Ϊ5�ıߡ�
        int localIndex = (worldDirection - rotationIndex + 6) % 6;
        return edges[localIndex];
    }
}