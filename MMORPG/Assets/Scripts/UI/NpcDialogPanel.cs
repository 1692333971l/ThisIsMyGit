using TMPro;
using UnityEngine;
using UnityEngine.UI;

//NPC 对话面板，显示 NPC 名称、对话内容和任务按钮，根据 NPC 是否有任务显示不同的按钮
public class NpcDialogPanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;//面板根对象
    [SerializeField] private TMP_Text _npcNameText;//NPC名称文本
    [SerializeField] private TMP_Text _dialogContentText;//对话内容文本
    [SerializeField] private Button _taskButton;//任务按钮
    [SerializeField] private Button _closeButton;//关闭按钮
    [SerializeField] private TaskPanel _taskPanel;//任务面板

    private NPCData _npcData;//当前显示的NPC数据
    private TaskData _currentTaskData;//当前NPC相关的任务数据

    private void Awake()
    {
        _taskButton.onClick.RemoveAllListeners();
        _taskButton.onClick.AddListener(OpenTaskPanel);
        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Hide();
    }
    //显示NPC对话面板，传入NPC数据，根据NPC数据和任务数据构建对话内容，并刷新任务按钮显示状态
    public void Show(NPCData npcData)
    {
        _npcData = npcData;//获取当前NPC相关的任务数据
        _currentTaskData = GameApp.Instance.PlayerTaskManager.GetCurrentTaskForNpc(npcData.NpcId);//获取当前NPC相关的任务数据

        _root.SetActive(true);//显示面板

        _npcNameText.text = npcData.Name;
        _dialogContentText.text = BuildDialogContent();

        BuildTaskButton();
    }
    //隐藏NPC对话面板
    public void Hide()
    {
        _root.SetActive(false);
    }
    //判断NPC对话面板是否正在显示
    public bool IsShowing()
    {
        return _root != null && _root.activeSelf;
    }
    //根据NPC数据和任务数据构建对话内容
    private string BuildDialogContent()
    {
        if (_currentTaskData == null)
        {
            return _npcData.DialogContent;
        }
        TaskStatus status = GameApp.Instance.PlayerTaskManager.GetTaskStatus(_currentTaskData.TaskId);
        switch (status)
        {
            case TaskStatus.InProgress:
            case TaskStatus.CanSubmit:
                return _currentTaskData.FinishDialog;
            case TaskStatus.Completed:
                return _currentTaskData.CompletedDialog;
            default:
                return _npcData.DialogContent;
        }
    }
    //根据NPC数据和任务数据构建任务按钮文本
    private void BuildTaskButton()
    {
        if (_currentTaskData == null)
        {
            _taskButton.gameObject.SetActive(false);
            return;
        }
        _taskButton.gameObject.SetActive(true);
        TMP_Text buttonText = _taskButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = _currentTaskData.TaskName;
    }
    //打开任务面板
    private void OpenTaskPanel()
    {
        if (_currentTaskData == null || _taskPanel == null)
        {
            return;
        }
        _root.SetActive(false);
        _taskPanel.Show(_currentTaskData);
    }
}