using UnityEngine;

// 玩家交互控制器脚本，挂载在玩家角色上，处理玩家与可交互对象的交互输入
public class PlayerInteractionController : MonoBehaviour
{
    private KeyCode _interactKey = KeyCode.F;//默认交互键为 F

    private IInteractable _currentInteractable;//当前可交互对象
    private NpcDialogPanel _npcDialogPanel;// NPC 对话面板引用

    private void Start()
    {
        _npcDialogPanel = FindObjectOfType<NpcDialogPanel>();
    }

    private void Update()
    {
        HandleInteractionInput();
    }
    // 处理交互输入
    private void HandleInteractionInput()
    {
        if (_currentInteractable == null)//如果当前没有可交互对象，禁止交互输入
        {
            return;
        }
        if (_npcDialogPanel != null && _npcDialogPanel.IsShowing())//如果 NPC 对话面板正在显示，禁止交互输入
        {
            return;
        }

        if (Input.GetKeyDown(_interactKey))//按下交互键时，如果当前有可交互对象，则调用其交互方法
        {
            _currentInteractable.Interact();
        }
    }

    public void SetCurrentInteractable(IInteractable interactable)//设置当前可交互对象
    {
        _currentInteractable = interactable;
    }

    public void ClearCurrentInteractable(IInteractable interactable)//清除当前可交互对象，只有当传入的对象与当前对象相同时才清除
    {
        if (_currentInteractable != interactable)
        {
            return;
        }
        _currentInteractable = null;
    }
}