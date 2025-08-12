using UnityEngine;
/*������������ű�
  ���幦�ܣ�������Ϸ���������Ϊ��Ŀǰ��Ҫ�Ǵ������������ʵ�־�ͷ���š�*/
public class CameraController : MonoBehaviour
{
    //--�ֶ�--
    [SerializeField] private Camera gameCamera;//˽���ֶΣ����ó����е������

    [Header("Camera Control")]
    [SerializeField] private float zoomSpeed = 5f;//��ͷ�����ٶ�
    [SerializeField] private float minZoom = 2f; // ��ͷ��С��Ұ��Χ��size��󣩡�
    [SerializeField] private float maxZoom = 10f; //��ͷ�����Ұ��Χ��size��С����

    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;// ���û����Inspector��ָ�������Զ����ҳ����б�ǩΪ"MainCamera"���������
        }
    }

    void Update()
    {
        HandleCameraZoom(); // ÿ֡��������������ŵķ�����
    }

    //--˽�з���--
    private void HandleCameraZoom()
    {
        //--�ֲ�����--
        //��ȡ�����ֵĹ���ֵ����ǰΪ�������Ϊ����
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)//��������й���
        {
            //�����µ�������ͼ��С����ȥ����ֵ�����ٶȣ�ʵ����ǰ�����Ŵ�size��С������������С��size���
            float newSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            // Mathf.Clamp ��newSize��ֵ������minZoom��maxZoom֮�䣬��ֹ�������š�
            gameCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}