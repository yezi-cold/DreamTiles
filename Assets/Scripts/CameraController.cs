using UnityEngine;
/*摄像机控制器脚本
  整体功能：控制游戏摄像机的行为，目前主要是处理滚轮输入以实现镜头缩放。*/
public class CameraController : MonoBehaviour
{
    //--字段--
    [SerializeField] private Camera gameCamera;//私有字段，引用场景中的摄像机

    [Header("Camera Control")]
    [SerializeField] private float zoomSpeed = 5f;//镜头缩放速度
    [SerializeField] private float minZoom = 2f; // 镜头最小视野范围（size变大）。
    [SerializeField] private float maxZoom = 10f; //镜头最大视野范围（size变小）。

    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;// 如果没有在Inspector中指定，就自动查找场景中标签为"MainCamera"的摄像机。
        }
    }

    void Update()
    {
        HandleCameraZoom(); // 每帧都调用摄像机缩放的方法。
    }

    //--私有方法--
    private void HandleCameraZoom()
    {
        //--局部变量--
        //获取鼠标滚轮的滚动值（向前为正，向后为负）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)//如果滚轮有滚动
        {
            //计算新的正交视图大小，减去滚动值乘以速度，实现向前滚动放大（size变小），向后滚动缩小（size变大）
            float newSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            // Mathf.Clamp 将newSize的值限制在minZoom和maxZoom之间，防止过度缩放。
            gameCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}