using UnityEngine;
using System.Collections.Generic; // ��Ҫ��������ռ�

public class TileController : MonoBehaviour
{
    public HexCoord Coords { get; private set; } // �õؿ������������
    public TileData TileData { get; private set; } // �õؿ�����ݶ���

    // �洢������Ե�� MeshRenderer ���á���Ҫ�� Unity Inspector ���ֶ���ֵ��
    [SerializeField] private MeshRenderer[] edgeRenderers = new MeshRenderer[6];

    // ���ڸ����Ĳ��ʡ���Ҫ�� Unity Inspector �����뷢����ʡ�
    [SerializeField] private Material highlightMaterial;

    // �洢ÿ����Ե��ԭʼ���ʣ��Ա���ȡ������ʱ�ָ�
    private Material[] originalEdgeMaterials = new Material[6];

    // �� Initialize �����д��� GridManager �����ã����� FindObjectOfType ���ܿ���
    public void Initialize(HexCoord coords, TileData tileData, GridManager gridManager)
    {
        Coords = coords;
        TileData = tileData;
        gameObject.name = $"Tile ({coords.Q}, {coords.R}) - {tileData.name}"; // �������

        // ���õؿ������λ�ã�ʹ�ô���� GridManager ����
        transform.position = gridManager.HexToWorld(Coords);

        // �״γ�ʼ��ʱ�������б�Ե��ԭʼ����
        for (int i = 0; i < 6; i++)
        {
            if (edgeRenderers[i] != null)
            {
                // ʹ�� sharedMaterial ���ⴴ����ʵ����Ӱ������
                originalEdgeMaterials[i] = edgeRenderers[i].sharedMaterial;
            }
            else
            {
                Debug.LogWarning($"TileController on {gameObject.name}: Edge Renderer at index {i} is not assigned!");
            }
        }
    }

    /// <summary>
    /// ��ȡָ������ı�Ե���͡�
    /// </summary>
    /// <param name="direction">Ҫ��ȡ�������η���</param>
    /// <returns>�÷���ı�Ե���͡�</returns>
    public EdgeType GetEdgeType(HexDirection direction)
    {
        if (TileData != null && (int)direction >= 0 && (int)direction < TileData.edges.Length)
        {
            return TileData.edges[(int)direction];
        }
        return EdgeType.None; // Ĭ�Ϸ��� None
    }

    /// <summary>
    /// ��ȡ��ָ�������෴�ı�Ե���͡�
    /// </summary>
    /// <param name="direction">��ǰ����</param>
    /// <returns>�෴����ı�Ե���͡�</returns>
    public EdgeType GetOppositeEdgeType(HexDirection direction)
    {
        HexDirection oppositeDir = (HexDirection)(((int)direction + 3) % 6); // ��Է������� +3
        return GetEdgeType(oppositeDir);
    }

    /// <summary>
    /// ����ָ�������Ե�ĸ���״̬��
    /// </summary>
    /// <param name="direction">Ҫ�����ı�Ե����</param>
    /// <param name="highlight">�Ƿ������</param>
    public void SetEdgeHighlight(HexDirection direction, bool highlight)
    {
        int index = (int)direction;
        if (index >= 0 && index < edgeRenderers.Length && edgeRenderers[index] != null)
        {
            if (highlight && highlightMaterial != null)
            {
                // ע�⣺����ʹ�� .material �ᴴ������ʵ������������ Draw Call��
                // ���ڴ������������Կ���ʹ�� MaterialPropertyBlock��
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

    /// <summary>
    /// ������б�Ե�ĸ���״̬���ָ�ԭʼ���ʡ�
    /// </summary>
    public void ClearAllEdgeHighlights()
    {
        for (int i = 0; i < 6; i++)
        {
            SetEdgeHighlight((HexDirection)i, false);
        }
    }
}