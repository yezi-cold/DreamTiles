using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;

    [Header("Camera Control")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f; // ����: minzoom -> minZoom
    [SerializeField] private float maxZoom = 10f; // ����: maxzoom -> maxZoom

    private void Awake()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleCameraZoom(); // ����: cameraZoom -> HandleCameraZoom
    }

    // ����: ��������Ϊ���շ�
    private void HandleCameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = gameCamera.orthographicSize - scroll * zoomSpeed;
            // ʹ��������ı�����
            gameCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}