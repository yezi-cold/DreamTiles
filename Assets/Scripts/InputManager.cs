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
        // 游戏是否暂停等逻辑可以加在这里
        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        // 左键点击 -> 请求放置
        if (Input.GetMouseButtonDown(0))
        {
            if (tilePlacer != null)
            {
                tilePlacer.HandlePlacement(Input.mousePosition);
            }
        }

        // 右键点击 -> 请求旋转
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
        // 'R'键 -> 请求旋转
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (tilePlacer != null)
            {
                tilePlacer.HandleRotation();
            }
        }
    }
}