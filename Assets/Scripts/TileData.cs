using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Tile System/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Visuals")]
    public GameObject tilePrefab; // �õؿ����Ͷ�Ӧ���Ӿ�Prefab

    [Header("Edges")]
    [Tooltip("������������ı�Ե���ͣ�˳����HexDirectionö�ٶ�Ӧ��Right, UpRight, UpLeft, Left, DownLeft, DownRight")]
    public EdgeType[] edges = new EdgeType[6]; // �����ߵ�����

    [Header("Gameplay Properties")]
    public int baseScore = 1; // ���ô˵ؿ�Ļ����÷�
    // δ��������Ӹ������ԣ����磺
    // public bool hasQuestMarker;
    // public EdgeType questType;
}