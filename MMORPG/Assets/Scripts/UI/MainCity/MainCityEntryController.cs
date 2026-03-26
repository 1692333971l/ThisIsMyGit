using Protocol;
using UnityEngine;
using UnityEngine.UI;

// 主城入口控制器
// 作用：作为“主城场景中的多人同步总入口”
//
// 它负责：
// 1. 进入主城场景后，向服务端发送 EnterGame 请求
// 2. 收到 EnterGameResponse 后，生成本地玩家和已有的远端玩家
// 3. 收到 PlayerEnterNotify 时，生成新进入的远端玩家
// 4. 收到 PlayerLeaveNotify 时，移除离开的远端玩家
// 5. 收到 PlayerMoveNotify 时，更新远端玩家移动表现
//
// 你可以把它理解成：
// “主城场景里的联机流程总调度器”
public class MainCityEntryController : MonoBehaviour
{
    // 跟随摄像机组件
    [SerializeField] private CameraFollow _cameraFollow;
    
    // 角色呼出面板
    [SerializeField] private Image _characterPanel;

    /// <summary>
    /// Unity 生命周期：OnEnable
    /// 
    /// 作用：
    /// 订阅 WorldService 中的事件。
    /// 这样当服务端返回进入游戏响应、玩家进入通知、离开通知、移动通知时，
    /// 当前主城控制器就能收到并处理。
    /// </summary>
    private void OnEnable()
    {
        // 订阅“进入游戏响应”事件
        GameApp.Instance.WorldService.OnEnterGameResponse += HandleEnterGameResponse;

        // 订阅“玩家进入通知”事件
        GameApp.Instance.WorldService.OnPlayerEnterNotify += HandlePlayerEnterNotify;

        // 订阅“玩家离开通知”事件
        GameApp.Instance.WorldService.OnPlayerLeaveNotify += HandlePlayerLeaveNotify;

        // 订阅“玩家移动通知”事件
        GameApp.Instance.WorldService.OnPlayerMoveNotify += HandlePlayerMoveNotify;
    }

    /// <summary>
    /// Unity 生命周期：OnDisable
    /// 
    /// 作用：
    /// 取消订阅事件，避免：
    /// 1. 对象销毁后仍然收到事件
    /// 2. 重复进入场景时重复订阅，导致回调执行多次
    /// </summary>
    private void OnDisable()
    {
        // 如果 GameApp 已经销毁，则直接返回，避免空引用
        if (GameApp.Instance == null) return;

        // 取消订阅“进入游戏响应”
        GameApp.Instance.WorldService.OnEnterGameResponse -= HandleEnterGameResponse;

        // 取消订阅“玩家进入通知”
        GameApp.Instance.WorldService.OnPlayerEnterNotify -= HandlePlayerEnterNotify;

        // 取消订阅“玩家离开通知”
        GameApp.Instance.WorldService.OnPlayerLeaveNotify -= HandlePlayerLeaveNotify;

        // 取消订阅“玩家移动通知”
        GameApp.Instance.WorldService.OnPlayerMoveNotify -= HandlePlayerMoveNotify;
    }

    /// <summary>
    /// Unity 生命周期：Start
    /// 
    /// 作用：
    /// 当主城场景刚加载完时，执行主城联机初始化流程：
    /// 1. 清掉之前可能残留的远端玩家对象
    /// 2. 向服务端发送 EnterGame 请求
    /// 3. 向服务端发送 GetInventory 请求
    /// 
    /// 为什么先 ClearAll：
    /// 避免切场景、重新进入主城时，旧的远端玩家对象残留在场景中
    /// </summary>
    private void Start()
    {
        // 清空场景中已有的远端玩家对象
        GameApp.Instance.RemotePlayerManager.ClearAll();

        // 向服务端发送“进入游戏/进入主城”请求
        // 服务端收到后会返回：
        // 1. 当前玩家自己的角色信息
        // 2. 当前地图中已经在线的其他玩家列表
        GameApp.Instance.WorldService.SendEnterGame();

        //获取当前玩家背包
        GameApp.Instance.InventoryService.SendGetInventoryRequest();
    }

    /// <summary>
    /// 处理进入主城响应
    /// 
    /// 流程：
    /// 1. 检查进入是否成功
    /// 2. 保存当前本地玩家角色信息
    /// 3. 生成本地玩家对象
    /// 4. 设置摄像机跟随本地玩家
    /// 5. 给本地玩家的移动控制器设置摄像机引用
    /// 6. 生成当前地图中已在线的其他玩家
    /// </summary>
    /// <param name="response">服务端返回的进入主城响应</param>
    private void HandleEnterGameResponse(EnterGameResponse response)
    {
        // 如果进入失败，则打印错误并返回
        if (response.ErrorCode != (int)ErrorCode.Success)
        {
            Debug.LogError("进入主城失败：" + response.Message);
            return;
        }

        // 把服务端返回的“当前玩家角色信息”保存到本地玩家角色管理器中
        // 后续本地玩家生成、网络同步、UI等都会用到
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(response.CharacterInfo);

        // 生成本地玩家对象
        GameObject playerObject = GameApp.Instance.PlayerSpawnManager.SpawnCurrentPlayer();

        // 让摄像机开始跟随本地玩家
        _cameraFollow.SetTarget(playerObject.transform);

        // 获取本地玩家的移动控制器
        PlayerMovementController movementController = playerObject.GetComponent<PlayerMovementController>();

        // 将角色可操作面板传入角色控制器
        playerObject.GetComponent<PlayerInputController>().Init(_characterPanel, _cameraFollow);

        // 如果存在移动控制器，则把摄像机 Transform 传给它
        // 因为你的移动方向是基于摄像机前后左右来计算的
        if (movementController != null)
        {
            movementController.SetCameraTranform(_cameraFollow.transform);
        }

        // 如果服务端返回了“当前地图中其他在线玩家列表”
        if (response.OtherPlayers != null)
        {
            // 遍历这些其他在线玩家
            foreach (var otherPlayer in response.OtherPlayers)
            {
                // 把他们一个个生成成远端玩家对象
                GameApp.Instance.PlayerSpawnManager.SpawnRemotePlayer(otherPlayer);
            }
        }
    }

    /// <summary>
    /// 处理“有新玩家进入主城”的通知
    /// 
    /// 作用：
    /// 当服务端广播说“某个新玩家进入地图”时，
    /// 客户端需要把这个新玩家生成成远端玩家对象。
    /// 
    /// 这里有两个保护：
    /// 1. 如果通知里的角色是自己，则跳过
    /// 2. 如果这个远端玩家已经存在，则跳过，防止重复生成
    /// </summary>
    /// <param name="notify">玩家进入通知</param>
    private void HandlePlayerEnterNotify(PlayerEnterNotify notify)
    {
        // 先取出本地玩家自己的角色信息
        Protocol.CharacterInfo self = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();

        // 如果通知里这个角色ID其实就是自己，则不处理
        // 防止自己把自己又当成远端玩家生成一次
        if (self != null && notify.Player.CharacterId == self.CharacterId)
        {
            return;
        }

        // 如果远端玩家字典里已经有这个角色了，说明已经生成过
        // 为了避免重复生成，这里直接返回
        if (GameApp.Instance.RemotePlayerManager.GetRemotePlayer(notify.Player.CharacterId) != null)
        {
            return;
        }

        // 生成这个新进入的远端玩家
        GameApp.Instance.PlayerSpawnManager.SpawnRemotePlayer(notify.Player);
    }

    /// <summary>
    /// 处理“玩家离开主城”的通知
    /// 
    /// 作用：
    /// 当服务端广播“某个玩家离开了地图”时，
    /// 客户端要根据角色ID，把对应远端玩家对象从场景里删除。
    /// </summary>
    /// <param name="notify">玩家离开通知</param>
    private void HandlePlayerLeaveNotify(PlayerLeaveNotify notify)
    {
        // 从远端玩家管理器中移除这个角色对应的对象
        GameApp.Instance.RemotePlayerManager.RemoveRemotePlayer(notify.CharacterId);
    }

    /// <summary>
    /// 处理“玩家移动同步”通知
    /// 
    /// 作用：
    /// 当服务端广播某个玩家新的位置/朝向/移动状态时，
    /// 客户端需要更新该远端玩家对象的表现。
    /// 
    /// 注意：
    /// 如果广播里的角色其实是自己，则跳过。
    /// 因为自己的移动是本地控制的，不需要再走远端表现逻辑。
    /// </summary>
    /// <param name="notify">玩家移动通知</param>
    private void HandlePlayerMoveNotify(PlayerMoveNotify notify)
    {
        // 获取本地玩家自己的角色信息
        Protocol.CharacterInfo self = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();

        // 如果这条移动通知对应的是自己，则不处理
        // 自己的角色移动由本地输入控制，不走远端同步逻辑
        if (self != null && notify.CharacterId == self.CharacterId)
        {
            return;
        }

        // 交给远端玩家管理器，根据 CharacterId 找到对应远端对象并更新状态
        GameApp.Instance.RemotePlayerManager.UpdateRemotePlayerMove(notify);
    }
}