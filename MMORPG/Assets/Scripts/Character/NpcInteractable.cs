using UnityEngine;

// NPC 可交互对象脚本，挂载在 NPC 角色上，实现 IInteractable 接口
public class NpcInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC Config")]
    [SerializeField] private int _npcId = 1;

    [Header("UI References")]
    [SerializeField] private NpcWorldHintView _worldHintView;// NPC 世界提示 UI 引用
    [SerializeField] private NpcDialogPanel _dialogPanel;// NPC 对话面板引用

    private NPCData _npcData;

    private void Start()
    {
        LoadNpcData();
    }
    // 加载 NPC 数据
    private void LoadNpcData()
    {
        NPCDataResponse response = GameApp.Instance.NPCService.GetNpc(_npcId);
        _npcData = response.Data;
    }
    // 获取交互提示文本
    public string GetInteractHint()
    {
        return _npcData.InteractHint;
    }
    // 实现 IInteractable 接口的交互方法，显示 NPC 对话面板
    public void Interact()
    {
        _dialogPanel.Show(_npcData.Name, _npcData.DialogContent);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteractionController interactionController = other.GetComponent<PlayerInteractionController>();
        interactionController.SetCurrentInteractable(this);
        _worldHintView.Show(_npcData.Name, _npcData.InteractHint);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteractionController interactionController = other.GetComponent<PlayerInteractionController>();
        interactionController.ClearCurrentInteractable(this);
        _worldHintView.Hide();
        _dialogPanel.Hide();
    }
}