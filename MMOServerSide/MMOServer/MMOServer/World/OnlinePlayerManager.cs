using MMOServer.Network;
using Protocol;

// 服务端在线玩家管理器
// 作用：统一管理“已经进入游戏世界”的在线角色
// 注意：这里管理的是在线角色，不是所有网络连接
namespace MMOServer.World
{
    public class OnlinePlayerManager
    {
        // 以 CharacterId 作为 key，保存当前所有在线玩家
        // key   = 角色ID
        // value = 在线玩家对象
        private readonly Dictionary<int, OnlinePlayer> _playersByCharacterId = new Dictionary<int, OnlinePlayer>();

        /// <summary>
        /// 添加一个在线玩家
        /// 如果这个角色已经在线，则先移除旧数据，再放入新的在线对象
        /// </summary>
        /// <param name="player">要添加的在线玩家对象</param>
        public void AddPlayer(OnlinePlayer player)
        {
            // 如果字典中已经存在这个角色ID
            // 说明这个角色可能重复登录，或者旧连接还残留
            if (_playersByCharacterId.ContainsKey(player.CharacterId))
            {
                // 先把旧的在线数据移除，避免重复
                RemovePlayer(player.CharacterId);
            }

            // 将新的在线玩家对象加入字典
            _playersByCharacterId[player.CharacterId] = player;
        }

        /// <summary>
        /// 根据角色ID移除一个在线玩家
        /// </summary>
        /// <param name="characterId">要移除的角色ID</param>
        public void RemovePlayer(int characterId)
        {
            // 先判断字典中是否存在该角色
            if (_playersByCharacterId.ContainsKey(characterId))
            {
                // 存在则移除
                _playersByCharacterId.Remove(characterId);
            }
        }

        /// <summary>
        /// 根据角色ID获取在线玩家对象
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <returns>
        /// 找到则返回对应的 OnlinePlayer；
        /// 找不到则返回 null
        /// </returns>
        public OnlinePlayer GetPlayer(int characterId)
        {
            // 尝试从字典中取出指定角色ID对应的在线玩家
            _playersByCharacterId.TryGetValue(characterId, out var player);

            // 返回查询结果
            return player;
        }

        /// <summary>
        /// 获取指定地图中的所有在线玩家
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <returns>该地图中的所有在线玩家列表</returns>
        public List<OnlinePlayer> GetPlayersByMapId(int mapId)
        {
            // 从所有在线玩家中筛选出 MapId 等于目标地图ID的玩家
            // Values 表示字典中的所有 OnlinePlayer
            // Where 用来过滤条件
            // ToList 把筛选结果转成 List<OnlinePlayer>
            return _playersByCharacterId.Values
                .Where(p => p.MapId == mapId)
                .ToList();
        }

        /// <summary>
        /// 获取指定地图中的“其他玩家”
        /// 常用于广播时排除自己
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <param name="selfCharacterId">自己的角色ID（需要排除）</param>
        /// <returns>同地图且不是自己的其他在线玩家列表</returns>
        public List<OnlinePlayer> GetOtherPlayersByMapId(int mapId, int selfCharacterId)
        {
            // 从所有在线玩家中筛选：
            // 1. 地图ID相同
            // 2. 角色ID不是自己
            return _playersByCharacterId.Values
                .Where(p => p.MapId == mapId && p.CharacterId != selfCharacterId)
                .ToList();
        }

        /// <summary>
        /// 根据网络会话查找对应的在线玩家
        /// 常用于客户端断线时，根据 session 找到是谁掉线了
        /// </summary>
        /// <param name="session">客户端会话对象</param>
        /// <returns>找到则返回在线玩家，找不到返回 null</returns>
        public OnlinePlayer GetPlayerBySession(ClientSession session)
        {
            // 在所有在线玩家中查找第一个 Session 等于目标 session 的玩家
            // FirstOrDefault:
            // - 找到第一个匹配项就返回
            // - 如果找不到，返回默认值 null
            return _playersByCharacterId.Values.FirstOrDefault(p => p.Session == session);
        }

        /// <summary>
        /// 将服务端内部的 OnlinePlayer 对象，转换成共享协议层的 OnlineCharacterInfo
        /// 这样可以安全地发给客户端
        /// </summary>
        /// <param name="player">服务端在线玩家对象</param>
        /// <returns>可用于网络传输的在线角色信息</returns>
        public OnlineCharacterInfo ToOnlineCharacterInfo(OnlinePlayer player)
        {
            // 创建一个共享协议对象，并把 OnlinePlayer 中的数据拷贝过去
            return new OnlineCharacterInfo
            {
                CharacterId = player.CharacterId,// 角色ID
                UserId = player.UserId,// 所属用户ID
                Name = player.Name,// 角色名字
                Profession = player.Profession,// 职业ID
                Level = player.Level,// 等级
                Gold = player.Gold,// 金币
                Hp = player.Hp,// 当前生命值
                Mp = player.Mp,// 当前法力值
                MapId = player.MapId,// 当前所在地图ID
                PosX = player.PosX,// 当前坐标 X
                PosY = player.PosY,// 当前坐标 Y
                PosZ = player.PosZ,// 当前坐标 Z
                RotY = player.RotY,// 当前朝向（绕Y轴旋转角度）
                IsMoving = player.IsMoving,// 当前是否在移动
                IsRunning = player.IsRunning// 当前是否在跑动
            };
        }
    }
}