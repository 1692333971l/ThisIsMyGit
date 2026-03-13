// 任务数据响应类，包含请求结果和任务数据
public class TaskDataResponse
{
    private bool success;//请求是否成功
    private string message;//请求结果消息
    private TaskData data;//任务数据

    public bool Success { get => success; set => success = value; }
    public string Message { get => message; set => message = value; }
    public TaskData Data { get => data; set => data = value; }
}
