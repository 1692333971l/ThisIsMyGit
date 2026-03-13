//任务数据模型
public class TaskData
{
    private int taskId;//任务ID
    private int startNpcId;//任务起始NPC ID
    private int finishNpcId;//任务结束NPC ID
    private string taskName;//任务名称
    private string description;//任务描述
    private string startDialog;//任务开始对话
    private string finishDialog;//任务完成对话
    private string completedDialog;//任务已完成对话
    private TaskType taskType;//任务类型（主线、支线、日常等）

    public int TaskId { get => taskId; set => taskId = value; }
    public int StartNpcId { get => startNpcId; set => startNpcId = value; }
    public int FinishNpcId { get => finishNpcId; set => finishNpcId = value; }
    public string TaskName { get => taskName; set => taskName = value; }
    public string Description { get => description; set => description = value; }
    public string StartDialog { get => startDialog; set => startDialog = value; }
    public string FinishDialog { get => finishDialog; set => finishDialog = value; }
    public string CompletedDialog { get => completedDialog; set => completedDialog = value; }
    public TaskType TaskType { get => taskType; set => taskType = value; }

}
