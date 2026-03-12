//可交互对象抽象
using UnityEngine;

public interface IInteractable
{
    string GetInteractHint();//获取交互提示文本
    Vector3 GetInteractPosition();//获取交互位置
    int GetPriority();//获取交互优先级，数值越大优先级越高

    void OnFocusEnter();//当玩家进入交互范围时调用
    void OnFocusExit();//当玩家离开交互范围时调用
    void Interact();//当玩家按下交互键时调用
}
