//玩家任务数据模型
public class PlayerTaskData
{
    private int taskId;//任务ID
    private TaskStatus status;

    public int TaskId { get => taskId; set => taskId = value; }
    public TaskStatus Status { get => status; set => status = value; }
}
