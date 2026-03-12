using UnityEngine;
// NPC 可交互组件，挂载在 NPC 角色上，实现 IInteractable 接口
public class NpcInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC Config")]
    [SerializeField] private int _npcId = 1;//NPC ID，用于获取 NPC 数据
    [SerializeField] private int _priority = 50;//交互优先级，数值越大优先级越高

    [Header("UI References")]
    [SerializeField] private NpcWorldHintView _worldHintView;//世界提示 UI 引用(NPC自身文字提示)
    [SerializeField] private NpcDialogPanel _dialogPanel;//对话面板 UI 引用

    private NPCData _npcData;

    private void Start()
    {
        LoadNpcData();
    }
    //加载 NPC 数据
    private void LoadNpcData()
    {
        NPCDataResponse response = GameApp.Instance.NPCService.GetNpc(_npcId);
        _npcData = response.Data;
    }
    //IInteractable 接口实现 获取交互提示文本
    public string GetInteractHint()
    {
        return _npcData.InteractHint;
    }
    //IInteractable 接口实现 获取交互位置，这里直接使用 NPC 的位置
    public Vector3 GetInteractPosition()
    {
        return transform.position;
    }
    //IInteractable 接口实现 获取交互优先级
    public int GetPriority()
    {
        return _priority;
    }
    //IInteractable 接口实现 当玩家进入交互范围时调用，显示世界提示 UI
    public void OnFocusEnter()
    {
        _worldHintView.Show(_npcData.Name, _npcData.InteractHint);
    }
    //IInteractable 接口实现 当玩家离开交互范围时调用，隐藏世界提示 UI
    public void OnFocusExit()
    {
        _worldHintView.Hide();
    }
    //IInteractable 接口实现 当玩家按下交互键时调用，显示对话面板 UI
    public void Interact()
    {
        _dialogPanel.Show(_npcData.Name, _npcData.DialogContent);
    }
    //当玩家进入触发器范围时注册为交互候选对象
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }
        GameApp.Instance.InteractionManager.RegisterInteractable(this);
    }
    // 当玩家离开触发器范围时取消注册
    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }
        GameApp.Instance.InteractionManager.UnregisterInteractable(this);
    }
    // 辅助方法：判断进入触发器的对象是否是玩家
    private bool IsPlayer(Collider other)
    {
        return other.GetComponent<PlayerInteractionController>() != null;
    }
}