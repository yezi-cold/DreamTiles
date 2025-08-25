using UnityEngine;

//�ؿ����ݽű�
//��������Ϸ��ÿһ�ֵؿ���������ԣ���ۣ����ͣ��������ߵ����ͣ�����ʹ����scriptableobject���ɱ�д�ű����󣩣����Դ���������Դ�ļ���
public enum EdgeType//���� �ߵ�����
{
    None = 0,//�հ׻���δ����
    Grass = 1,//�ݵ�
    Forest = 2,//ɭ��
    Water = 3,//ˮ
    House = 4,//����
    Factory = 5,//����
    Road = 6,//��·
}

public enum TileType//����ؿ����������
{
    Park,
    River,
    Residential,
    Industrial,
}

//--�ඨ��--
//[CreateAssetMenu] ������Կ�����unity�ġ�assets/create���˵�����ʾ��������Դ�ļ���ѡ�
[CreateAssetMenu(fileName = "NewTileData", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject//�̳���ScriptableObject����ʾ����һ�����Դ�������Դ�ļ�
{
    //--�ֶ�--
    //��Щ�����ֶλ�ֱ����ʾ��unity��Inspector����ϣ���������Դ�ļ���������á�
    [Header("Visuals")]//�����Կ�����inspector��������һ�����⣬���ڻ����ֶ�
    public GameObject tilePrefab; // �����ֶ����ڴ�Ÿõؿ��Ӧ��prefab

    [Header("Gameplay Properties")]
    public TileType tileType; // �����ֶΣ���������ؿ�����ͣ����������ö�٣�
    public int baseScore = 1; // �����ֶΣ����ô˵ؿ�Ļ����÷�

    [Header("Edges")]
    //[tooltip]���������������ͣ���ֶ���ʱ��ʾһ����ʾ��Ϣ��

    [Tooltip("������������ı�Ե���ͣ�˳����HexDirectionö�ٶ�Ӧ��Right, UpRight, UpLeft, Left, DownLeft, DownRight")]
    public EdgeType[] edges = new EdgeType[6]; // ���������ֶΣ��洢�����ߵ����͡������˳���Ӧ HexDirection ö�ٵ�˳��
}