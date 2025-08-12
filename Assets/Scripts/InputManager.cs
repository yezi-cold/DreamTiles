using UnityEngine;
/*输入管理器脚本
整体功能: 它的唯一职责就是监听玩家的原始输入（鼠标点击、键盘按键），然后将这些输入转换为“意图”，
并通知TilePlacer去执行相应的动作，如“请求放置”或“请求旋转”。它完全不关心能不能放置、地块长什么样等逻辑*/
public class InputManager : MonoBehaviour
{
    //--字段--
    [SerializeField] private TilePlacer tilePlacer;//私有字段，用于持有对tileplacer脚本实例的引用，这是唯一需要通信的组件

    private void Awake()
    {
        if (tilePlacer == null)//如果在inspector中没有手动拖拽赋值
        {
            //自动在场景中查找tileplacer组件的实例
            tilePlacer = FindObjectOfType<TilePlacer>();
            if (tilePlacer == null)//如果仍然找不到
            {
                Debug.LogError("InputManager: TilePlacer not found in scene!");
            }
        }
    }

    void Update()
    {
        // 未来可以在这里加入游戏是否暂停的判断逻辑。
        HandleMouseInput();//每帧调用处理鼠标输入的方法
        HandleKeyboardInput();//每帧调用处理键盘输入的方法。
    }

    //--私有方法--
    private void HandleMouseInput()
    {
        // 会在鼠标左键被按下的那一帧返回true。
        if (Input.GetMouseButtonDown(0))
        {
            if (tilePlacer != null)//确保tileplacer实例存在。
            {
                //调用tileplacer的handleplacement方法，并将当前鼠标的屏幕坐标作为参数传进去
                tilePlacer.HandlePlacement(Input.mousePosition);
            }
        }

        // 会在鼠标右键按下的那一帧返回true
        if (Input.GetMouseButtonDown(1))
        {
            if (tilePlacer != null)
            {
                //调用方法，请求旋转。
                tilePlacer.HandleRotation();
            }
        }
    }

    private void HandleKeyboardInput()
    {
        // 按下键盘R那一帧返回true。
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (tilePlacer != null)
            {
                //同样调用旋转的方法。
                tilePlacer.HandleRotation();
            }
        }
    }
}