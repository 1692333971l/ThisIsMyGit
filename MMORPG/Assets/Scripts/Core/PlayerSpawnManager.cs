using Protocol;
using UnityEngine;

// 玩家模型实例化管理器
// 作用：负责把“角色数据”转换成“场景中的角色对象”
//
// 当前分成两类生成：
// 1. SpawnCurrentPlayer()  -> 生成本地玩家
// 2. SpawnRemotePlayer()   -> 生成远端玩家
//
// 为什么要单独有这个类：
// 因为“生成角色模型”本身是一项独立职责，
// 不应该直接塞进 MainCityEntryController 或 CharacterService 里。
public class PlayerSpawnManager
{
    /// <summary>
    /// 生成当前本地玩家角色
    /// 
    /// 流程：
    /// 1. 从 PlayerCharacterManager 取当前角色信息
    /// 2. 根据职业ID从职业配置表中查到模型路径
    /// 3. 从 Resources 加载对应的角色 prefab
    /// 4. 在出生点实例化角色对象
    /// 5. 记录到 PlayerCharacterManager 中
    /// 6. 把这个角色设置为“本地玩家模式”
    /// 7. 给它挂上本地网络同步器 LocalPlayerNetworkSync
    /// </summary>
    /// <returns>生成好的本地玩家对象</returns>
    public GameObject SpawnCurrentPlayer()
    {
        // 从当前玩家角色管理器中取出“当前本地玩家”的角色信息
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();

        // 根据角色职业ID，从职业配置表中取出对应职业配置
        // 比如模型路径、初始职业配置等
        ProfessionConfig professionConfig = GameApp.Instance.ProfessionConfigManager.GetById(characterInfo.Profession);

        // 根据职业配置中的模型路径，从 Resources 中加载对应角色 prefab
        GameObject prefab = Resources.Load<GameObject>(professionConfig.ModelPath);

        // 获取当前本地角色应该生成的位置（出生点）
        Vector3 spawnPosition = GameApp.Instance.PlayerCharacterManager.GetSpawnPosition();

        // 在场景中实例化本地玩家对象
        // Quaternion.identity 表示默认朝向（无旋转）
        GameObject playerObject = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);

        // 给实例化出来的对象起一个更容易识别的名字，方便在 Hierarchy 中查看
        playerObject.name = $"Player_{characterInfo.Name}_{characterInfo.CharacterId}";

        // 把当前本地玩家对象保存到 PlayerCharacterManager 中
        // 后续摄像机绑定、本地控制、其他逻辑都可能需要拿到这个对象
        GameApp.Instance.PlayerCharacterManager.SetCharacterObject(playerObject);

        // 尝试获取角色对象上的 PlayerCharacterView 组件
        // 这个组件用来切换角色的表现模式（本地/远端/预览）
        PlayerCharacterView playerView = playerObject.GetComponent<PlayerCharacterView>();

        // 如果有这个组件，就把它初始化为“本地玩家模式”
        if (playerView != null)
        {
            playerView.SetupAsLocalPlayer();
        }

        // 尝试获取本地网络同步器组件
        // 它负责把本地玩家的位置、朝向、移动状态定时上报给服务端
        LocalPlayerNetworkSync localSync = playerObject.GetComponent<LocalPlayerNetworkSync>();

        // 如果 prefab 上没有预先挂这个组件，就运行时动态加一个
        if (localSync == null)
        {
            localSync = playerObject.AddComponent<LocalPlayerNetworkSync>();
        }

        // 初始化本地同步器，传入当前角色信息（主要用于拿 CharacterId）
        localSync.Init(characterInfo);

        // 返回本地玩家对象
        return playerObject;
    }

    /// <summary>
    /// 生成一个远端玩家角色
    /// 
    /// 流程：
    /// 1. 根据远端玩家的职业ID查模型配置
    /// 2. 加载对应 prefab
    /// 3. 根据网络同步来的位置和朝向实例化对象
    /// 4. 把对象设置成“远端玩家模式”
    /// 5. 给对象挂上 RemotePlayerSync 组件
    /// 6. 把当前网络状态应用给这个远端对象
    /// 7. 把它登记到 RemotePlayerManager 中
    /// </summary>
    /// <param name="characterInfo">远端在线角色信息</param>
    /// <returns>生成好的远端玩家对象</returns>
    public GameObject SpawnRemotePlayer(OnlineCharacterInfo characterInfo)
    {
        // 根据远端玩家的职业ID，查职业配置表
        ProfessionConfig professionConfig = GameApp.Instance.ProfessionConfigManager.GetById(characterInfo.Profession);

        // 根据配置中的模型路径加载角色 prefab
        GameObject prefab = Resources.Load<GameObject>(professionConfig.ModelPath);

        // 根据网络同步来的在线角色信息，构造远端玩家生成位置
        Vector3 spawnPosition = new Vector3(characterInfo.PosX, characterInfo.PosY, characterInfo.PosZ);

        // 根据网络同步来的朝向，构造远端玩家初始旋转
        Quaternion rotation = Quaternion.Euler(0f, characterInfo.RotY, 0f);

        // 在场景中实例化远端玩家对象
        GameObject playerObject = GameObject.Instantiate(prefab, spawnPosition, rotation);

        // 给远端玩家对象命名，方便在 Hierarchy 中区分
        playerObject.name = $"RemotePlayer_{characterInfo.Name}_{characterInfo.CharacterId}";

        // 获取角色表现组件
        PlayerCharacterView playerView = playerObject.GetComponent<PlayerCharacterView>();

        // 如果存在，则把它设置成“远端玩家模式”
        // 远端玩家模式会禁用本地输入和本地移动控制
        if (playerView != null)
        {
            playerView.SetupAsRemotePlayer();
        }

        // 获取远端同步器组件
        // 它负责根据服务端同步过来的状态，平滑表现远端角色
        RemotePlayerSync remoteSync = playerObject.GetComponent<RemotePlayerSync>();

        // 如果 prefab 上没挂这个组件，就运行时补上
        if (remoteSync == null)
        {
            remoteSync = playerObject.AddComponent<RemotePlayerSync>();
        }

        // 把当前这份在线状态立刻应用给远端玩家
        // 这样它刚生成出来时，位置、朝向、移动状态就是正确的
        remoteSync.ApplyNetState(
            spawnPosition,
            characterInfo.RotY,
            characterInfo.IsMoving,
            characterInfo.IsRunning
        );

        // 把这个远端玩家对象登记到 RemotePlayerManager 中
        // 后续收到移动通知、离开通知时，需要根据 CharacterId 找到它
        GameApp.Instance.RemotePlayerManager.AddRemotePlayer(characterInfo.CharacterId, playerObject);

        // 返回远端玩家对象
        return playerObject;
    }
}