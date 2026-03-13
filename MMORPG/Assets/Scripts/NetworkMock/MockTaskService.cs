using System.Collections.Generic;

// 模拟任务服务，提供根据NPC ID获取任务数据的功能
public class MockTaskService : ITaskService
{
    private readonly Dictionary<int, TaskData> _taskDict = new Dictionary<int, TaskData>();

    public MockTaskService()
    {
        _taskDict[1] = new TaskData
        {
            TaskId = 1,
            StartNpcId = 1,
            FinishNpcId = 1,
            TaskName = "初来乍到",
            Description = "到城市中心转转吧",
            StartDialog = "你刚来还不太熟悉这座城市。",
            FinishDialog = "恭喜你，你已经熟悉了这座城市。",
            CompletedDialog = "你已经完成了这个任务。",
            TaskType = TaskType.Mainline
        };

        _taskDict[2] = new TaskData
        {
            TaskId = 2,
            StartNpcId = 1,
            FinishNpcId = 1,
            TaskName = "小试牛刀",
            Description = "跟旁边的商人聊聊吧",
            StartDialog = "去和商人聊聊，认识一下大家。",
            FinishDialog = "看来你已经和商人熟络起来了。",
            CompletedDialog = "这个任务你已经做完了。",
            TaskType = TaskType.Mainline
        };

        _taskDict[3] = new TaskData
        {
            TaskId = 3,
            StartNpcId = 1,
            FinishNpcId = 1,
            TaskName = "大显身手",
            Description = "去击杀怪物证明自己",
            StartDialog = "城外有很多野兽，去证明你的实力。",
            FinishDialog = "你已经是一名合格的猎人了。",
            CompletedDialog = "你已经完成了狩猎任务。",
            TaskType = TaskType.Mainline
        };
    }
    public TaskDataResponse GetTaskByTaskId(int taskId)
    {
        if (_taskDict.TryGetValue(taskId, out TaskData taskData))
        {
            return new TaskDataResponse
            {
                Success = true,
                Message = "获取任务成功",
                Data = taskData
            };
        }
        return new TaskDataResponse
        {
            Success = false,
            Message = $"未找到任务，taskId = {taskId}",
            Data = null
        };
    }
    public List<TaskData> GetTasksByStartNpcId(int startNpcId)
    {
        List<TaskData> result = new List<TaskData>();

        foreach (TaskData taskData in _taskDict.Values)
        {
            if (taskData.StartNpcId == startNpcId)
            {
                result.Add(taskData);
            }
        }
        result.Sort((a, b) => a.TaskId.CompareTo(b.TaskId));
        return result;
    }
}