using System.Collections.Generic;
using UnityEngine;
//交互管理器，负责管理玩家周围的可交互对象，选择最佳目标并处理交互逻辑
public class InteractionManager
{
    private readonly List<IInteractable> _candidates = new List<IInteractable>();//当前所有可交互对象候选列表

    private IInteractable _currentInteractable;//当前最佳可交互对象
    private Transform _playerTransform;//玩家位置引用

    //初始化玩家引用
    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        RefreshCurrentInteractable();
    }
    //加入候选列表
    public void RegisterInteractable(IInteractable interactable)
    {
        if (interactable == null)//避免添加空对象
        {
            return;
        }
        if (_candidates.Contains(interactable))//避免重复添加
        {
            return;
        }

        _candidates.Add(interactable);
        RefreshCurrentInteractable();
    }
    //移出候选列表
    public void UnregisterInteractable(IInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        if (_candidates.Remove(interactable))
        {
            RefreshCurrentInteractable();
        }
    }
    // 玩家触发交互时调用
    public void InteractCurrent()
    {
        if (_currentInteractable == null)
        {
            return;
        }

        _currentInteractable.Interact();
    }
    //获取当前最佳可交互对象
    public IInteractable GetCurrentInteractable()
    {
        return _currentInteractable;
    }
    //清除所有数据，通常在场景切换时调用
    public void ClearAll()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.OnFocusExit();
            _currentInteractable = null;
        }

        _candidates.Clear();
        _playerTransform = null;
    }
    //重新计算当前最佳目标
    private void RefreshCurrentInteractable()
    {
        IInteractable best = SelectBestInteractable();
        if (_currentInteractable == best)
        {
            return;
        }
        if (_currentInteractable != null)
        {
            _currentInteractable.OnFocusExit();
        }
        _currentInteractable = best;
        if (_currentInteractable != null)
        {
            _currentInteractable.OnFocusEnter();
        }
    }

    //从候选列表中选择最佳目标，优先级高的优先，如果优先级相同则距离近的优先
    private IInteractable SelectBestInteractable()
    {
        // 若玩家引用为空或没有候选对象，则返回 null
        if (_playerTransform == null || _candidates.Count == 0)
        {
            return null;
        }

        IInteractable best = null;
        int priority = int.MinValue;//当前最高优先级
        float bestDistanceSqr = float.MaxValue;//当前最佳对象与玩家的距离平方
        foreach (var item in _candidates)
        {
            if (item.GetPriority() < priority)//优先级较低的对象直接跳过
            {
                continue;
            }
            priority = item.GetPriority();
            
            float distanceSqr = (item.GetInteractPosition() - _playerTransform.position).sqrMagnitude;//使用平方距离避免调用开方运算（性能更好）
            if (distanceSqr > bestDistanceSqr)//距离较远的对象直接跳过
            {
                continue;
            }
            bestDistanceSqr = distanceSqr;
            best = item;
        }
        return best;
    }
}