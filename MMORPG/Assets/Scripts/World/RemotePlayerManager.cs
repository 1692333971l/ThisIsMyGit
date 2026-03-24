using System.Collections.Generic;
using Protocol;
using UnityEngine;

// 场景里其他玩家对象的总表
// 作用：统一管理“当前场景中的所有远端玩家对象”
//
// 为什么需要它：
// 1. 收到玩家进入通知时，要生成远端玩家并登记起来
// 2. 收到玩家离开通知时，要根据 CharacterId 找到对应对象并删除
// 3. 收到玩家移动通知时，要根据 CharacterId 找到对应对象并更新状态
//
// 本质上它就是：
// CharacterId -> 远端玩家 GameObject
public class RemotePlayerManager
{
    /// <summary>
    /// 远端玩家字典
    /// key   = 角色ID CharacterId
    /// value = 对应的远端玩家 GameObject
    /// 
    /// 为什么用字典：
    /// 因为后续收到移动/离开通知时，需要快速通过 CharacterId 找到对象
    /// </summary>
    private readonly Dictionary<int, GameObject> _remotePlayerDict = new Dictionary<int, GameObject>();

    /// <summary>
    /// 添加一个远端玩家对象
    /// 
    /// 作用：
    /// 当收到“其他玩家进入场景”通知，或者进入主城时加载已有在线玩家列表后，
    /// 会把生成出来的远端玩家对象登记到这个字典里。
    /// 
    /// 如果这个角色ID已经存在，则先移除旧对象，再登记新对象，
    /// 防止同一个角色重复生成。
    /// </summary>
    /// <param name="characterId">远端玩家的角色ID</param>
    /// <param name="playerObject">远端玩家对应的 GameObject</param>
    public void AddRemotePlayer(int characterId, GameObject playerObject)
    {
        // 如果字典里已经存在这个角色ID
        // 说明这个远端玩家对象可能已经生成过了
        if (_remotePlayerDict.ContainsKey(characterId))
        {
            // 先移除旧对象，避免重复残留
            RemoveRemotePlayer(characterId);
        }

        // 把新的远端玩家对象登记到字典中
        _remotePlayerDict[characterId] = playerObject;
    }

    /// <summary>
    /// 根据角色ID获取远端玩家对象
    /// </summary>
    /// <param name="characterId">角色ID</param>
    /// <returns>
    /// 找到则返回对应的 GameObject，
    /// 找不到则返回 null
    /// </returns>
    public GameObject GetRemotePlayer(int characterId)
    {
        // 尝试从字典中取出指定角色ID对应的远端玩家对象
        _remotePlayerDict.TryGetValue(characterId, out var playerObject);

        // 返回查询结果
        return playerObject;
    }

    /// <summary>
    /// 移除一个远端玩家对象
    /// 
    /// 作用：
    /// 当收到“玩家离开通知”时，
    /// 需要根据 CharacterId 找到场景里的对象并销毁。
    /// </summary>
    /// <param name="characterId">要移除的角色ID</param>
    public void RemoveRemotePlayer(int characterId)
    {
        // 先尝试从字典中找到这个角色对应的远端对象
        if (_remotePlayerDict.TryGetValue(characterId, out var playerObject))
        {
            // 如果对象不为 null，说明它还存在于场景中
            if (playerObject != null)
            {
                // 销毁这个远端玩家对象
                GameObject.Destroy(playerObject);
            }

            // 从字典中移除该角色记录
            _remotePlayerDict.Remove(characterId);
        }
    }

    /// <summary>
    /// 清空所有远端玩家对象
    /// 
    /// 作用：
    /// 比如重新进入主城、切场景、重置在线玩家状态时，
    /// 可以一次性把场景中的所有远端玩家对象都删除。
    /// </summary>
    public void ClearAll()
    {
        // 遍历字典中的所有远端玩家对象
        foreach (var pair in _remotePlayerDict)
        {
            // pair.Value 就是 GameObject
            if (pair.Value != null)
            {
                // 销毁远端玩家对象
                GameObject.Destroy(pair.Value);
            }
        }

        // 最后把字典清空
        _remotePlayerDict.Clear();
    }

    /// <summary>
    /// 根据服务端发来的移动通知，更新某个远端玩家的表现状态
    /// 
    /// 流程：
    /// 1. 根据 CharacterId 找到对应的远端玩家对象
    /// 2. 取到它身上的 RemotePlayerSync 组件
    /// 3. 把新的目标位置、目标朝向、是否在移动传给 RemotePlayerSync
    /// 4. RemotePlayerSync 再负责平滑表现
    /// </summary>
    /// <param name="notify">服务端发来的玩家移动广播</param>
    public void UpdateRemotePlayerMove(PlayerMoveNotify notify)
    {
        // 根据移动通知中的 CharacterId，查找对应的远端玩家对象
        GameObject playerObject = GetRemotePlayer(notify.CharacterId);

        // 如果找不到对象，说明这个远端玩家还没生成，或者已经被删除
        if (playerObject == null)
        {
            return;
        }

        // 获取这个远端玩家对象上的 RemotePlayerSync 组件
        RemotePlayerSync remoteSync = playerObject.GetComponent<RemotePlayerSync>();

        // 如果没有这个组件，就没法进行远端状态同步
        if (remoteSync == null)
        {
            return;
        }

        // 把网络同步过来的状态交给 RemotePlayerSync
        // 它会负责：
        // 1. 更新目标位置
        // 2. 更新目标朝向
        // 3. 更新移动动画状态
        remoteSync.ApplyNetState(
            new Vector3(notify.PosX, notify.PosY, notify.PosZ),
            notify.RotY,
            notify.IsMoving
        );
    }
}