using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 任务操作面板，显示任务名称、描述和操作按钮，根据任务状态显示不同的按钮
public class TaskPanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;//面板根对象
    [SerializeField] private TMP_Text _taskNameText;//任务名称文本
    [SerializeField] private TMP_Text _taskDescriptionText;//任务描述文本
    [SerializeField] private TMP_Text _taskStatusText;//任务状态文本

    [SerializeField] private Button _acceptTaskButton;//接受任务按钮
    [SerializeField] private Button _submitTaskButton;//提交任务按钮
    [SerializeField] private Button _closeButton;//关闭按钮

    private TaskData _currentTaskData;//当前显示的任务数据

    private void Awake()
    {
        _acceptTaskButton.onClick.RemoveAllListeners();
        _acceptTaskButton.onClick.AddListener(OnClickAcceptTask);

        _submitTaskButton.onClick.RemoveAllListeners();
        _submitTaskButton.onClick.AddListener(OnClickSubmitTask);
    }
    //显示任务面板
    public void Show(TaskData taskData)
    {
        _currentTaskData = taskData;
        _root.SetActive(true);
        RefreshView();
    }
    //根据当前任务数据和任务状态刷新面板显示内容和按钮状态
    private void RefreshView()
    {
        TaskStatus status = GameApp.Instance.PlayerTaskManager.GetTaskStatus(_currentTaskData.TaskId);

        SetTexts(
            _currentTaskData.TaskName,
            _currentTaskData.Description,
            _currentTaskData.TaskType.ToString(),
            GetStatusText(status));
        switch (status)
        {
            case TaskStatus.NotAccepted:
                SetButtons(true, false);
                break;
            case TaskStatus.InProgress:
                SetButtons(false, false);
                break;
            case TaskStatus.CanSubmit:
                SetButtons(false, true);
                break;
            case TaskStatus.Completed:
                SetButtons(false, false);
                break;
        }
    }
    //根据任务状态返回对应的状态文本，用于在面板上显示
    private string GetStatusText(TaskStatus status)
    {
        switch (status)
        {
            case TaskStatus.NotAccepted:
                return "未接取";
            case TaskStatus.InProgress:
                return "进行中";
            case TaskStatus.CanSubmit:
                return "可提交";
            case TaskStatus.Completed:
                return "已完成";
            default:
                return "-";
        }
    }
    //点击接受任务按钮的处理，根据当前任务数据调用玩家任务管理器的接取任务方法，并刷新面板显示
    private void OnClickAcceptTask()
    {
        bool success = GameApp.Instance.PlayerTaskManager.AcceptTask(_currentTaskData.TaskId);
        if (!success)
        {
            return;
        }
        GameApp.Instance.PlayerTaskManager.MarkTaskCanSubmit(_currentTaskData.TaskId);//第一版临时模拟：接取任务后立刻允许提交
        RefreshView();
        _root.SetActive(false);
    }
    //点击提交任务按钮的处理，根据当前任务数据调用玩家任务管理器的提交任务方法，并刷新面板显示
    private void OnClickSubmitTask()
    {
        if (_currentTaskData == null)
        {
            return;
        }
        GameApp.Instance.PlayerTaskManager.SubmitTask(_currentTaskData.TaskId);
        RefreshView();
        _root.SetActive(false);
        //提交任务后可以在这里触发一些后续逻辑，比如给玩家奖励、解锁新的任务等 TODO
    }
    //设置面板上显示的文本内容，包括任务名称、描述、类型和状态
    private void SetTexts(string taskName, string taskDescription, string taskType, string taskStatus)
    {
        _taskNameText.text = taskName + "(" + taskStatus + ")";
        _taskDescriptionText.text = taskDescription;
    }

    private void SetButtons(bool showAccept, bool showSubmit)
    {
        _acceptTaskButton.gameObject.SetActive(showAccept);
        _submitTaskButton.gameObject.SetActive(showSubmit);
    }
}
