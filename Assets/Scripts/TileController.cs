using UnityEngine;
using System.Collections.Generic; // ��Ҫ��������ռ�

//�ؿ���ƽű�
public class TileController : MonoBehaviour
{
    //--����--
    public HexCoord Coords { get; private set; } // �������ԣ��洢�ؿ����ꡣ{ get; private set; } ��ʾ�ⲿֻ�ܶ�ȡ��ֻ��������ڲ���������
    public TileData TileData { get; private set; } // �������ԣ��洢�ؿ��������Դ

    //--�ֶ�--
    // ˽�����飬�洢������Ե��Ϸ�����MeshRenderer��������ڸı���ʣ���������
    [SerializeField] private MeshRenderer[] edgeRenderers = new MeshRenderer[6];

    //  ˽���ֶΣ��洢���ڸ����Ĳ��ʡ�
    [SerializeField] private Material highlightMaterial;
    // ˽�����飬���ڱ��ݱ�Ե��ԭʼ���ʣ��Ա�ȡ������ʱ�ָ���
    private Material[] originalEdgeMaterials = new Material[6];
    //˽���ֶΣ��洢�� GridManager ������
    private GridManager gridManager;

    //--����--
    // ��ʼ���������������ꡢ������Դ��GridManager����
    public void Initialize(HexCoord coords, TileData tileData, GridManager gridManager)
    {
        this.Coords = coords;//���õ�ǰ�ؿ������
        this.TileData = tileData; // �洢����ĵؿ�����
        this.gridManager = gridManager; // �洢�� GridManager ������

        //--�ֲ�����--
        // forѭ���ֵ�i��һ���ֲ�������ֻ��ѭ������Ч
        for (int i = 0; i < 6; i++)//ѭ�����Σ�����ÿ����Ե
        {
            if (edgeRenderers[i] != null)
            {
                // ����ԭʼ���ʣ�ʹ�� sharedMaterial ���ⴴ����ʵ����Ӱ������
                originalEdgeMaterials[i] = edgeRenderers[i].sharedMaterial;
            }
            else
            {
                //���û��ָ������ӡ������Ϣ����������
                Debug.LogWarning($"TileController on {gameObject.name}: Edge Renderer at index {i} is not assigned!");
            }
        }
    }

    // ��ȡָ������ı�Ե���͡�
    public EdgeType GetEdgeType(HexDirection direction)
    {
        //�ȼ��tiledata�Ƿ���ڣ��Լ����������Ƿ�����Ч�����鷶Χ��
        if (TileData != null && (int)direction >= 0 && (int)direction < TileData.edges.Length)
        {
            //��tiledata��edges�����ַ��ض�Ӧ����ı�Ե����
            return TileData.edges[(int)direction];
        }
        return EdgeType.None; // ������κ����⣬����none
    }

    //��ȡ��ָ�������෴���ı�Ե���͡�
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        //�ֲ����������㷴����������������������֣���Է������� +3
        HexDirection oppositeDir = (HexDirection)(((int)direction + 3) % 6); 
        //�����Լ��� GetEdgeType ��������ȡ�෴����ı�Ե���ͣ�������
        return GetEdgeType(oppositeDir);
    }

    // ����ָ������ı�Ե����״̬��
    public void SetEdgeHighlight(HexDirection direction, bool highlight)
    {
        int index = (int)direction;//������ö��ת��Ϊ��������
        //ȷ��������Ч�Ҷ�Ӧ��renderer�Ѿ���inspector������
        if (index >= 0 && index < edgeRenderers.Length && edgeRenderers[index] != null)
        {
            //�����Ҫ����������Ҹ����Ĳ����Ѿ�����
            if (highlight && highlightMaterial != null)
            {
                // ����Ե�Ĳ�������Ϊ�������ʡ�
                // ע�⣺ʹ�� .material ��Ϊ�ö��󴴽�һ���µĲ���ʵ��
                edgeRenderers[index].material = highlightMaterial;
            }
            else
            {
                // �ָ�ԭʼ����
                if (originalEdgeMaterials[index] != null)
                {
                    edgeRenderers[index].material = originalEdgeMaterials[index];
                }
            }
        }
    }
    //������б�Ե�ĸ���״̬
    public void ClearAllEdgeHighlights()
    {
        for (int i = 0; i < 6; i++)//ѭ�����Σ�����ÿ����Ե
        {
            // ���� SetEdgeHighlight ��������ÿ����Ե�ĸ���״̬������Ϊ false��
            SetEdgeHighlight((HexDirection)i, false);
        }
    }
    /// <summary>
    /// ��ȡ��ָ�������緽���෴�ı�Ե���ͣ��ῼ�ǵؿ��������ת��
    /// </summary>
    /// <param name="worldDirection">�����ⲿ�����緽��</param>
    /// <returns>�õؿ��ڽӴ����϶�Ӧ�ı�Ե����</returns>
    public EdgeType GetOppositeEdgeTypeInWorld(HexDirection worldDirection)
    {
        // 1. ����Ϸ����� Transform �����ȡ��ǰ�ؿ����ת����
        // ���Ǽ���ؿ��Y����ת����60�ȵ�������
        int rotationIndex = Mathf.RoundToInt(transform.rotation.eulerAngles.y / 60) % 6;

        // 2. ��������������еġ��෴������
        // �������������У��෴������������ǵ�ǰ���� + 3
        int oppositeWorldDirIndex = ((int)worldDirection + 3) % 6;

        // 3. ��������෴�����緽��ת��Ϊ�ؿ�����ġ����ر�Ե������
        // �߼��� TileData �еķ���һ��
        int localEdgeIndex = (oppositeWorldDirIndex - rotationIndex + 6) % 6;

        // 4. �ӵؿ������з�����ȷ�ı�Ե����
        return TileData.edges[localEdgeIndex];
    }
}