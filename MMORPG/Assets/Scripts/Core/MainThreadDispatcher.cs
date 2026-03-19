using System;
using System.Collections.Generic;
using UnityEngine;

//主线程调度器，把后台线程消息切回 Unity 主线程
//网络线程收到消息,丢入主线程队列,在 Update() 中执行
public class MainThreadDispatcher : MonoBehaviour
{
    public static MainThreadDispatcher Instance { get; private set; }

    // 存放需要回到主线程执行的任务
    private readonly Queue<Action> _actions = new Queue<Action>();

    private readonly object _lock = new object();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 后台线程把任务丢到这里，等待主线程执行
    /// </summary>
    public void Enqueue(Action action)
    {
        if (action == null) return;

        lock (_lock)
        {
            _actions.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_actions.Count > 0)
            {
                Action action = _actions.Dequeue();
                action?.Invoke();
            }
        }
    }
}