using UnityEngine;
/*��Ϸ�ܹ������ű�
 * ����Э��������ϵͳ֮���ͨ�š����幦��: ʹ�õ���ģʽ�����Լ���Ϊһ��ȫ��Ψһ��Э���ߡ�
 * ����������һ������������GridManager�����¼���Ȼ������һ������������ScoreManager��TileDeckManager������ָ�
 * ��ʹ�ø�����ϵͳ֮�䲻��Ҫ����ֱ�����ã���������϶ȣ�����Ŀ�ṹ������*/
public class GameManager : MonoBehaviour
{
    //--����--
    public static GameManager Instance { get; private set; }

    //--�ֶ�--
    // �������е���ϵͳ/����������Щ����Ҫ��unity inspector���ֶ���ק��ֵ
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private TileDeckManager tileDeckManager;
    [SerializeField] private ScoreManager scoreManager;
    // δ����������� UIManager, AudioManager ��

    private void Awake()
    {
        // ����ģʽ��׼д��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- ������Э������ ---

    // ���ؿ鱻�ɹ����ú���GridManager����
    public void OnTilePlaced(HexCoord coord, TileData tileData, int matchedEdgesCount)
    {
        // Э��1��֪ͨScoreManager�Ʒ�
        scoreManager.ScoreTilePlacement(tileData, matchedEdgesCount);

        // Э��2��֪ͨTileDeckManager����һ����
        tileDeckManager.DrawNewTile();
    }

    // ���ƶѳ��һ�����ƺ���TileDeckManager����
    public void OnNewTileDrawn(TileData newTile)
    {
        // Э����֪ͨTilePlacer�������ϵ���
        tilePlacer.SetCurrentTileToPlace(newTile);
    }
}