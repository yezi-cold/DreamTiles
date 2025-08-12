using UnityEngine;
/*����������ű�
���幦��: ����Ψһְ����Ǽ�����ҵ�ԭʼ���루����������̰�������Ȼ����Щ����ת��Ϊ����ͼ����
��֪ͨTilePlacerȥִ����Ӧ�Ķ������硰������á���������ת��������ȫ�������ܲ��ܷ��á��ؿ鳤ʲô�����߼�*/
public class InputManager : MonoBehaviour
{
    //--�ֶ�--
    [SerializeField] private TilePlacer tilePlacer;//˽���ֶΣ����ڳ��ж�tileplacer�ű�ʵ�������ã�����Ψһ��Ҫͨ�ŵ����

    private void Awake()
    {
        if (tilePlacer == null)//�����inspector��û���ֶ���ק��ֵ
        {
            //�Զ��ڳ����в���tileplacer�����ʵ��
            tilePlacer = FindObjectOfType<TilePlacer>();
            if (tilePlacer == null)//�����Ȼ�Ҳ���
            {
                Debug.LogError("InputManager: TilePlacer not found in scene!");
            }
        }
    }

    void Update()
    {
        // δ�����������������Ϸ�Ƿ���ͣ���ж��߼���
        HandleMouseInput();//ÿ֡���ô����������ķ���
        HandleKeyboardInput();//ÿ֡���ô����������ķ�����
    }

    //--˽�з���--
    private void HandleMouseInput()
    {
        // ���������������µ���һ֡����true��
        if (Input.GetMouseButtonDown(0))
        {
            if (tilePlacer != null)//ȷ��tileplacerʵ�����ڡ�
            {
                //����tileplacer��handleplacement������������ǰ������Ļ������Ϊ��������ȥ
                tilePlacer.HandlePlacement(Input.mousePosition);
            }
        }

        // ��������Ҽ����µ���һ֡����true
        if (Input.GetMouseButtonDown(1))
        {
            if (tilePlacer != null)
            {
                //���÷�����������ת��
                tilePlacer.HandleRotation();
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // ���¼���R��һ֡����true��
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (tilePlacer != null)
            {
                //ͬ��������ת�ķ�����
                tilePlacer.HandleRotation();
            }
        }
    }
}