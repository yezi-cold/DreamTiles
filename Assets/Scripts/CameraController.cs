using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;

    [Header("Camera Control")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f; // 修正: minzoom -> minZoom
    [SerializeField] private float maxZoom = 10f; // 修正: maxzoom -> maxZoom

    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleCameraZoom(); // 修正: cameraZoom -> HandleCameraZoom
    }

    // 修正: 方法名改为大驼峰
    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            // 使用修正后的变量名
            gameCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}