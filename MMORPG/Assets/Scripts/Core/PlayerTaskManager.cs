using System.Collections.Generic;

//玩家任务管理器，负责管理玩家的任务状态和任务进度
public class PlayerTaskManager
{
    private readonly Dictionary<int, PlayerTaskData> _taskStateMap = new Dictionary<int, PlayerTaskData>();//玩家任务状态字典，key是任务ID，value是玩家在这个任务上的状态数据

    //获取玩家某个任务的状态
    public TaskStatus GetTaskStatus(int taskId)
    {
        if (!_taskStateMap.TryGetValue(taskId, out PlayerTaskData taskData))//如果玩家没有这个任务，那么状态就是未接受
        {
            return TaskStatus.NotAccepted;
        }

        return taskData.Status;
    }
    //判断玩家是否已经接受了某个任务
    public bool HasTask(int taskId)
    {
        return _taskStateMap.ContainsKey(taskId);
    }
    //玩家接受任务，返回是否成功（如果已经接受过了，就不能再接受了）
    public bool AcceptTask(int taskId)
    {
        if (_taskStateMap.ContainsKey(taskId))
        {
            return false;
        }
        _taskStateMap.Add(taskId, new PlayerTaskData//添加一个新的任务状态数据，初始状态是进行中
        {
            TaskId = taskId,
            Status = TaskStatus.InProgress
        });
        return true;
    }
    //标记玩家某个任务可以提交了，返回是否成功（如果没有这个任务，或者状态不是进行中，就不能标记）
    public bool MarkTaskCanSubmit(int taskId)
    {
        if (!_taskStateMap.TryGetValue(taskId, out PlayerTaskData taskData))
        {
            return false;
        }
        if (taskData.Status != TaskStatus.InProgress)
        {
            return false;
        }
        taskData.Status = TaskStatus.CanSubmit;
        return true;
    }
    //玩家提交任务，返回是否成功（如果没有这个任务，或者状态不是可以提交，就不能提交）
    public bool SubmitTask(int taskId)
    {
        if (!_taskStateMap.TryGetValue(taskId, out PlayerTaskData taskData))
        {
            return false;
        }
        if (taskData.Status != TaskStatus.CanSubmit)
        {
            return false;
        }
        taskData.Status = TaskStatus.Completed;
        return true;
    }
    //获取玩家所有任务的状态数据，返回一个字典，key是任务ID，value是玩家在这个任务上的状态数据
    public Dictionary<int, PlayerTaskData> GetAllTaskStates()
    {
        return _taskStateMap;
    }
    // 给 NPC 用：从这个 NPC 的任务列表里找玩家当前最该推进的任务
    public TaskData GetCurrentTaskForNpc(int npcId)
    {
        List<TaskData> npcTasks = GameApp.Instance.TaskService.GetTasksByStartNpcId(npcId);
        if (npcTasks == null || npcTasks.Count == 0)
        {
            return null;
        }
        foreach (TaskData task in npcTasks)
        {
            TaskStatus status = GetTaskStatus(task.TaskId);
            if (status != TaskStatus.Completed)
            {
                return task;
            }
        }
        return null;
    }
}
