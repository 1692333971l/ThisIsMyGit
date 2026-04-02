using TMPro;
using UnityEngine;
using UnityEngine.UI;

//对话面板
public class DialoguePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _npcNameText;//NPC名字文本
    [SerializeField] private TMP_Text _dialogueContentText;//对话内容文本
    [SerializeField] private Button _openShopButton;//打开商店按钮
    [SerializeField] private Button _startTaskButton;//开始任务按钮
    [SerializeField] private UIManager _uiManager;//UI管理器

    private int _shopId;//商店ID
    private int _taskId;//任务ID

    private void Start()
    {
        _openShopButton.onClick.AddListener(OnOpenShopButtonClicked);
        _startTaskButton.onClick.AddListener(OnStartTaskButtonClicked);
    }
    public void Show(NpcConfig npcConfig)
    {
        _npcNameText.text = npcConfig.NpcName;
        _dialogueContentText.text = $"你好，我是{npcConfig.NpcName}，有什么可以帮你的吗？";
        _openShopButton.gameObject.SetActive(npcConfig.HasShop == 1);
        _startTaskButton.gameObject.SetActive(npcConfig.HasTask == 1);
        _shopId = npcConfig.ShopId;
        _taskId = npcConfig.TaskId;
        gameObject.SetActive(true);
    }
    private void OnOpenShopButtonClicked()
    {
        gameObject.SetActive(false);
        _uiManager.ShowShopPanel(_shopId);
    }
    private void OnStartTaskButtonClicked()
    {

    }
}
