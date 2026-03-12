using UnityEngine;
// 玩家交互控制器，负责监听玩家输入并触发交互事件
public class PlayerInteractionController : MonoBehaviour
{
    private KeyCode _interactKey = KeyCode.F;//交互按键

    private void Update()
    {
        GameApp.Instance.InteractionManager.Initialize(transform);//初始化交互管理器，传入玩家对象的 Transform

        if (Input.GetKeyDown(_interactKey))
        {
            GameApp.Instance.InteractionManager.InteractCurrent();//调用交互管理器执行当前交互
        }
    }
}