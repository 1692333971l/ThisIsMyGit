using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using MMOServer.Network;
using MMOServer.World;
using Protocol;

namespace MMOServer.Services
{
    /// <summary>
    /// 角色业务服务
    /// 
    /// 主要职责：
    /// 1. 处理进入游戏/进入主城
    /// 2. 处理玩家移动同步
    /// 3. 处理玩家离线/退出
    /// </summary>
    public class WorldService
    {
        /// <summary>
        /// 角色数据库仓库
        /// 用于访问数据库中的角色数据（查角色、建角色等）
        /// </summary>
        private readonly CharacterRepository _characterRepository;

        /// <summary>
        /// 构造函数
        /// 初始化角色数据库仓库
        /// </summary>
        public WorldService()
        {
            _characterRepository = new CharacterRepository();
        }
        /// <summary>
        /// 处理“进入游戏/进入主城”请求
        /// 
        /// 流程：
        /// 1. 反序列化请求
        /// 2. 校验参数
        /// 3. 从数据库检查该角色是否存在且属于当前用户
        /// 4. 构建服务端在线玩家对象 OnlinePlayer
        /// 5. 获取当前地图中其他在线玩家
        /// 6. 把自己加入在线玩家管理器
        /// 7. 把角色信息 + 其他在线玩家列表返回给当前客户端
        /// 8. 广播“新玩家进入”给同地图其他玩家
        /// </summary>
        /// <param name="requestMessage">客户端发来的网络消息</param>
        /// <param name="session">当前客户端连接会话</param>
        /// <returns>进入游戏响应消息</returns>
        public NetMessage HandleEnterGame(NetMessage requestMessage, ClientSession session)
        {
            // 把消息体反序列化成进入游戏请求对象
            EnterGameRequest request = JsonHelper.FromJson<EnterGameRequest>(requestMessage.BodyJson);

            // 创建响应对象
            EnterGameResponse response = new EnterGameResponse();

            // 打印日志
            Logger.Info($"HandleEnterGame: UserId = {request.UserId}, CharacterId = {request.CharacterId}");

            // 参数校验：用户ID和角色ID都必须有效
            if (request.UserId <= 0 || request.CharacterId <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "进入游戏参数无效";

                // 即使失败也给一个空列表，防止客户端空指针
                response.OtherPlayers = new List<OnlineCharacterInfo>();

                return BuildEnterGameResponse(response);
            }

            try
            {
                // 从数据库查询该角色，并验证它属于当前用户
                CharacterEntity entity = _characterRepository.GetByCharacterIdAndUserId(request.CharacterId, request.UserId);

                // 查不到说明角色不存在，或者角色不属于该用户
                if (entity == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在或不属于当前用户";
                    response.OtherPlayers = new List<OnlineCharacterInfo>();
                    return BuildEnterGameResponse(response);
                }

                // 构建服务端在线玩家对象
                // 这是“在线运行时状态”，和数据库存档角色不是一回事
                var onlinePlayer = new OnlinePlayer
                {
                    // 基础角色信息
                    CharacterId = entity.Id,
                    UserId = entity.UserId,
                    Name = entity.Name,
                    Profession = entity.Profession,
                    Level = entity.Level,
                    Gold = entity.Gold,
                    Hp = entity.Hp,
                    Mp = entity.Mp,

                    // 地图与位置
                    MapId = entity.MapId,
                    PosX = entity.PosX,
                    PosY = entity.PosY,
                    PosZ = entity.PosZ,

                    // 初始在线状态
                    // 刚进入游戏时默认朝向 0
                    RotY = 0f,

                    // 默认先认为没在移动
                    IsMoving = false,

                    // 绑定当前连接会话
                    Session = session
                };

                // 获取“同地图中除了自己以外”的其他在线玩家
                // 这样当前客户端进入后，就能先把别人生成出来
                List<OnlinePlayer> otherPlayers = GameServer.Instance.OnlinePlayerManager
                    .GetOtherPlayersByMapId(entity.MapId, entity.Id);

                // 把当前玩家加入在线玩家管理器
                GameServer.Instance.OnlinePlayerManager.AddPlayer(onlinePlayer);

                // 把当前角色ID和用户ID记录到会话中
                // 这样后面断线/移动时可以根据 session 找回是谁
                session.CurrentCharacterId = entity.Id;
                session.UserId = entity.UserId;

                // 设置成功响应
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "进入游戏成功";

                // 返回当前玩家自己的角色信息
                response.CharacterInfo = new CharacterInfo
                {
                    CharacterId = entity.Id,
                    UserId = entity.UserId,
                    Name = entity.Name,
                    Profession = entity.Profession,
                    Level = entity.Level,
                    Gold = entity.Gold,
                    Hp = entity.Hp,
                    Mp = entity.Mp,
                    MapId = entity.MapId,
                    PosX = entity.PosX,
                    PosY = entity.PosY,
                    PosZ = entity.PosZ
                };

                // 把其他在线玩家列表转换成可传输的协议对象列表
                response.OtherPlayers = otherPlayers
                    .Select(p => GameServer.Instance.OnlinePlayerManager.ToOnlineCharacterInfo(p))
                    .ToList();

                // 通知同地图其他玩家：
                // “有新玩家进入主城了”
                BroadcastPlayerEnter(onlinePlayer);

                // 返回进入游戏响应
                return BuildEnterGameResponse(response);
            }
            catch (Exception ex)
            {
                // 记录异常日志
                Logger.Error($"HandleEnterGame failed: {ex.Message}");
                Logger.Error($"HandleEnterGame failed: {ex}");

                // 构建失败响应
                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "进入游戏失败";
                response.OtherPlayers = new List<OnlineCharacterInfo>();

                return BuildEnterGameResponse(response);
            }
        }

        /// <summary>
        /// 处理玩家移动请求
        /// 
        /// 流程：
        /// 1. 反序列化移动请求
        /// 2. 校验角色ID
        /// 3. 从在线玩家管理器中找到该玩家
        /// 4. 校验这个 session 是否真的属于这个玩家
        /// 5. 更新玩家在线位置/朝向/移动状态
        /// 6. 广播移动通知给同地图其他玩家
        /// </summary>
        /// <param name="requestMessage">客户端发来的移动消息</param>
        /// <param name="session">当前客户端会话</param>
        public void HandlePlayerMove(NetMessage requestMessage, ClientSession session)
        {
            // 把消息体反序列化成移动请求对象
            PlayerMoveRequest request = JsonHelper.FromJson<PlayerMoveRequest>(requestMessage.BodyJson);

            // 角色ID非法则直接忽略
            if (request.CharacterId <= 0)
            {
                return;
            }

            // 根据角色ID找到当前在线玩家
            OnlinePlayer player = GameServer.Instance.OnlinePlayerManager.GetPlayer(request.CharacterId);

            // 如果在线玩家不存在，说明该玩家还没进入世界或者已掉线
            if (player == null)
            {
                return;
            }

            // 安全校验：
            // 防止客户端伪造别人角色ID发移动包
            // 只有当前 session 对应的玩家，才能更新自己的移动状态
            if (player.Session != session)
            {
                return;
            }

            // 更新该玩家在服务端内存中的实时位置
            player.PosX = request.PosX;
            player.PosY = request.PosY;
            player.PosZ = request.PosZ;

            // 更新该玩家的朝向
            player.RotY = request.RotY;

            // 更新该玩家是否正在移动或跑动
            player.IsMoving = request.IsMoving;
            player.IsRunning = request.IsRunning;

            // 构造“玩家移动通知”
            PlayerMoveNotify notify = new PlayerMoveNotify
            {
                CharacterId = player.CharacterId,
                PosX = player.PosX,
                PosY = player.PosY,
                PosZ = player.PosZ,
                RotY = player.RotY,
                IsMoving = player.IsMoving,
                IsRunning = player.IsRunning
            };

            // 再把通知包装成网络消息
            NetMessage netMessage = new NetMessage
            {
                MessageId = (int)MessageId.PlayerMoveNotify,
                BodyJson = JsonHelper.ToJson(notify)
            };

            // 获取同地图中除了自己以外的其他玩家
            List<OnlinePlayer> otherPlayers = GameServer.Instance.OnlinePlayerManager
                .GetOtherPlayersByMapId(player.MapId, player.CharacterId);

            // 把移动通知发给这些其他玩家
            foreach (var other in otherPlayers)
            {
                other.Session.SendMessage(netMessage);
            }
        }

        /// <summary>
        /// 处理玩家掉线（被动断线）
        /// 
        /// 场景：
        /// 1. 客户端异常关闭
        /// 2. 网络断开
        /// 3. 服务器读取失败后触发 Close()
        /// 
        /// 流程：
        /// 1. 根据 session 找到对应在线玩家
        /// 2. 从在线玩家管理器中移除
        /// 3. 广播“玩家离开”给同地图其他玩家
        /// </summary>
        /// <param name="session">掉线玩家的网络会话</param>
        public void HandlePlayerDisconnect(ClientSession session)
        {
            // 根据当前会话查找在线玩家
            OnlinePlayer player = GameServer.Instance.OnlinePlayerManager.GetPlayerBySession(session);

            // 找不到说明这个连接可能还没进入游戏，或者已经被清理过
            if (player == null)
            {
                return;
            }

            // 先把地图ID和角色ID记下来，后面广播要用
            int mapId = player.MapId;
            int characterId = player.CharacterId;

            // 保存玩家位置
            SaveOnlinePlayerState(player);
            // 广播“该玩家离开”给同地图其他玩家
            BroadcastPlayerLeave(player);
            // 从在线玩家管理器中移除这个角色
            GameServer.Instance.OnlinePlayerManager.RemovePlayer(characterId);
        }

        /// <summary>
        /// 处理玩家主动退出请求
        /// 
        /// 场景：
        /// 1. 客户端正常退出
        /// 2. Unity 编辑器停止运行时主动发送退出包
        /// 
        /// 和 HandlePlayerDisconnect 的区别：
        /// - Exit 是主动上报退出
        /// - Disconnect 是服务端被动发现断线
        /// </summary>
        /// <param name="requestMessage">客户端发来的退出请求</param>
        /// <param name="session">当前客户端会话</param>
        public void HandlePlayerExit(NetMessage requestMessage, ClientSession session)
        {
            // 反序列化退出请求
            PlayerExitRequest request = JsonHelper.FromJson<PlayerExitRequest>(requestMessage.BodyJson);

            // 基础校验：请求不能为 null，角色ID必须合法
            if (request == null || request.CharacterId <= 0)
            {
                return;
            }

            // 根据角色ID从在线玩家管理器中查找该玩家
            OnlinePlayer player = GameServer.Instance.OnlinePlayerManager.GetPlayer(request.CharacterId);

            // 玩家不存在则直接返回
            if (player == null)
            {
                return;
            }

            // 安全校验：防止别人伪造退出请求踢掉其他玩家
            if (player.Session != session)
            {
                return;
            }

            // 保存玩家位置
            SaveOnlinePlayerState(player);
            // 广播“该玩家离开”给同地图其他玩家
            BroadcastPlayerLeave(player);
            // 从在线玩家管理器中移除该玩家
            GameServer.Instance.OnlinePlayerManager.RemovePlayer(player.CharacterId);
            // 打一条日志
            Logger.Warn($"HandlePlayerExit success: CharacterId={player.CharacterId}");
        }

        /// <summary>
        /// 构建进入游戏响应消息
        /// </summary>
        /// <param name="response">进入游戏响应对象</param>
        /// <returns>网络消息</returns>
        private NetMessage BuildEnterGameResponse(EnterGameResponse response)
        {
            return new NetMessage
            {
                // 设置消息号：进入游戏响应
                MessageId = (int)MessageId.EnterGameResponse,

                // 序列化响应体
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 广播“玩家进入”通知
        /// 
        /// 作用：
        /// 当前玩家进入主城后，通知同地图其他在线玩家：
        /// “有新人进来了，你们也把他生成出来”
        /// </summary>
        /// <param name="onlinePlayer">刚进入游戏的在线玩家</param>
        private void BroadcastPlayerEnter(OnlinePlayer onlinePlayer)
        {
            // 构造“玩家进入通知”协议对象
            PlayerEnterNotify notify = new PlayerEnterNotify
            {
                // 把服务端在线玩家对象转成可传输的协议对象
                Player = GameServer.Instance.OnlinePlayerManager.ToOnlineCharacterInfo(onlinePlayer)
            };

            // 包装成网络消息
            NetMessage message = new NetMessage
            {
                MessageId = (int)MessageId.PlayerEnterNotify,
                BodyJson = JsonHelper.ToJson(notify)
            };

            // 获取同地图中除自己外的其他玩家
            List<OnlinePlayer> otherPlayers = GameServer.Instance.OnlinePlayerManager
                .GetOtherPlayersByMapId(onlinePlayer.MapId, onlinePlayer.CharacterId);

            // 把进入通知发给这些其他玩家
            foreach (var other in otherPlayers)
            {
                other.Session.SendMessage(message);
            }
        }

        /// <summary>
        /// 广播“玩家离开”通知
        /// 
        /// 作用：
        /// 当玩家主动退出时，通知同地图其他玩家：
        /// “这个角色离开了，把它从场景中删除”
        /// </summary>
        /// <param name="player">离开的在线玩家</param>
        private void BroadcastPlayerLeave(OnlinePlayer player)
        {
            // 构造离开通知协议对象
            PlayerLeaveNotify notify = new PlayerLeaveNotify
            {
                CharacterId = player.CharacterId
            };

            // 包装成网络消息
            NetMessage message = new NetMessage
            {
                MessageId = (int)MessageId.PlayerLeaveNotify,
                BodyJson = JsonHelper.ToJson(notify)
            };

            // 获取同地图中除自己外的其他玩家
            List<OnlinePlayer> others = GameServer.Instance.OnlinePlayerManager
                .GetOtherPlayersByMapId(player.MapId, player.CharacterId);

            // 把离开通知发给这些玩家
            foreach (var other in others)
            {
                other.Session.SendMessage(message);
            }
        }

        /// <summary>
        /// 把在线玩家当前状态保存到数据库
        /// 当前版本先保存地图和坐标
        /// </summary>
        private void SaveOnlinePlayerState(OnlinePlayer player)
        {
            if (player == null)
            {
                return;
            }

            try
            {
                _characterRepository.UpdateCharacterPosition(
                    player.CharacterId,
                    player.MapId,
                    player.PosX,
                    player.PosY,
                    player.PosZ
                );

                Logger.Info($"SaveOnlinePlayerState success: CharacterId={player.CharacterId}, MapId={player.MapId}, Pos=({player.PosX}, {player.PosY}, {player.PosZ})");
            }
            catch (Exception ex)
            {
                Logger.Error($"SaveOnlinePlayerState failed: CharacterId={player.CharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());
            }
        }
    }
}