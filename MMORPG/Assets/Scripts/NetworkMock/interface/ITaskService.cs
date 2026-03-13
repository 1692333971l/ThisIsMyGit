//任务服务接口
using System.Collections.Generic;

public interface ITaskService
{
    TaskDataResponse GetTaskByTaskId(int taskId);
    List<TaskData> GetTasksByStartNpcId(int startNpcId);
}
