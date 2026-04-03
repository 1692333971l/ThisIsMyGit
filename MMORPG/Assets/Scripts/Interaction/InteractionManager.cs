//玩家交互管理器
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager
{
    private List<IInteractable> _interactables = new List<IInteractable>();//当前所有可交互对象列表
    private IInteractable _iInteractable;//最佳交互对象

    //新增交互对象
    public void AddInteractable(IInteractable interactable)
    {
        if (!_interactables.Contains(interactable))
        {
            _interactables.Add(interactable);
        }
    }
    //移除交互对象
    public void RemoveInteractable(IInteractable interactable)
    {
        if (_interactables.Contains(interactable))
        {
            _interactables.Remove(interactable);
        }
    }
    //获取当前最佳交互对象
    public IInteractable GetBestInteractable(Transform transform)
    {
        CalculateBestInteractable(transform);
        return _iInteractable;
    }
    //交互
    public bool Interact(Transform transform)
    {
        if (_interactables == null || _interactables.Count == 0)
        {
            _iInteractable = null;
            return false;
        }
        CalculateBestInteractable(transform);
        _iInteractable.Interact();
        return true;
    }
    //计算最佳交互对象
    private void CalculateBestInteractable(Transform transform)
    {
        if (_interactables == null || _interactables.Count == 0)
        {
            _iInteractable = null;
            return;
        }

        IInteractable bestInteractable = null;
        float bestDistance = float.MaxValue;

        foreach (var item in _interactables)
        {
            if (item == null || item.GetTransform() == null)
                continue;

            float distance = Vector3.Distance(transform.position, item.GetTransform().position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestInteractable = item;
            }
        }

        _iInteractable = bestInteractable;
    }
}
