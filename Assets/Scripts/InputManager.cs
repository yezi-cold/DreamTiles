using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private TilePlacer tilePlacer;

    private void Awake()
    {
        if (tilePlacer == null)
        {
            tilePlacer = FindObjectOfType<TilePlacer>();
            if (tilePlacer == null)
            {
                Debug.LogError("InputManager: TilePlacer not found in scene!");
            }
        }
    }

    void Update()
    {
        // ��Ϸ�Ƿ���ͣ���߼����Լ�������
        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        // ������ -> �������
        if (Input.GetMouseButtonDown(0))
        {
            if (tilePlacer != null)
            {
                tilePlacer.HandlePlacement(Input.mousePosition);
            }
        }

        // �Ҽ���� -> ������ת
        if (Input.GetMouseButtonDown(1))
        {
            if (tilePlacer != null)
            {
                tilePlacer.HandleRotation();
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // 'R'�� -> ������ת
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (tilePlacer != null)
            {
                tilePlacer.HandleRotation();
            }
        }
    }
}