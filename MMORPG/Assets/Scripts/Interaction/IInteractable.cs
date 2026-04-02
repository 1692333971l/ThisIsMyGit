// 交互对象接口
using UnityEngine;

public interface IInteractable
{
    Transform GetTransform(); // 获取交互对象的Transform组件
    void Interact(); // 定义交互行为的方法
}
