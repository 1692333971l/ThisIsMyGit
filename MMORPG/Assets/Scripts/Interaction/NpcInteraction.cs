using TMPro;
using UnityEngine;

//NPC交互组件
public class NpcInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private int _npcId;//NPC配置表ID
    [SerializeField] private TMP_Text _hint;//世界提示文本
    [SerializeField] private UIManager _uiManager;//对话面板

    public Transform GetTransform()
    {
        return this.transform;
    }
    public void Interact()
    {
        NpcConfig npcConfig = GameApp.Instance.NpcConfigManager.GetById(_npcId);
        _uiManager.ShowDialoguePanel(npcConfig);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _hint.gameObject.SetActive(true);
            GameApp.Instance.InteractionManager.AddInteractable(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _hint.gameObject.SetActive(false);
            GameApp.Instance.InteractionManager.RemoveInteractable(this);
        }
    }
}
