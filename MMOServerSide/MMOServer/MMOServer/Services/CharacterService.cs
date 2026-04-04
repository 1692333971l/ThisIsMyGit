using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services
{
    /// <summary>
    /// 角色业务服务
    /// 
    /// 主要职责：
    /// 1. 处理角色列表获取
    /// 2. 处理角色创建
    /// </summary>
    public class CharacterService
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
        public CharacterService()
        {
            _characterRepository = new CharacterRepository();
        }

        /// <summary>
        /// 处理“获取角色列表”请求
        /// 
        /// 流程：
        /// 1. 反序列化客户端请求
        /// 2. 校验参数是否合法
        /// 3. 从数据库读取该用户下的所有角色
        /// 4. 转成协议对象 CharacterInfo
        /// 5. 构造响应消息返回给客户端
        /// </summary>
        /// <param name="requestMessage">客户端发来的网络消息</param>
        /// <returns>获取角色列表响应消息</returns>
        public NetMessage HandleGetCharacterList(NetMessage requestMessage)
        {
            // 把客户端发来的 Json 字符串，反序列化成 GetCharacterListRequest 对象
            GetCharacterListRequest request = JsonHelper.FromJson<GetCharacterListRequest>(requestMessage.BodyJson);

            // 创建一个响应对象，后面会把处理结果写进去
            GetCharacterListResponse response = new GetCharacterListResponse();

            // 打日志，方便查看服务端收到的请求参数
            Logger.Info($"HandleGetCharacterList: UserId = {request.UserId}");

            // 先校验用户ID是否合法
            if (request.UserId <= 0)
            {
                // 设置错误码：参数无效
                response.ErrorCode = (int)ErrorCode.InvalidParams;

                // 设置提示信息
                response.Message = "用户ID无效";

                // 角色列表返回空集合，避免客户端空引用
                response.CharacterList = new List<CharacterInfo>();

                // 组装成 NetMessage 并返回
                return BuildGetCharacterListResponse(response);
            }

            try
            {
                // 从数据库中读取当前用户拥有的所有角色实体
                List<CharacterEntity> entityList = _characterRepository.GetCharacterListByUserId(request.UserId);

                // 请求成功，设置成功码
                response.ErrorCode = (int)ErrorCode.Success;

                // 设置成功提示
                response.Message = "获取角色列表成功";

                // 把数据库实体 CharacterEntity 转成协议层 CharacterInfo
                // 因为数据库实体不应该直接发给客户端
                response.CharacterList = entityList.Select(e => new CharacterInfo
                {
                    // 数据库角色主键 -> 协议角色ID
                    CharacterId = e.Id,
                    UserId = e.UserId,// 所属用户ID
                    Name = e.Name,// 角色名称
                    Profession = e.Profession,// 职业ID
                    Level = e.Level,// 等级
                    Gold = e.Gold,// 金币
                    Hp = e.Hp,// 当前生命值
                    Mp = e.Mp,// 当前法力值
                    MapId = e.MapId,// 地图id
                    PosX = e.PosX,// x坐标
                    PosY = e.PosY,// y坐标
                    PosZ = e.PosZ,// z坐标
                }).ToList();

                // 构造并返回获取角色列表响应
                return BuildGetCharacterListResponse(response);
            }
            catch (Exception ex)
            {
                // 出现异常时记录错误日志
                Logger.Error($"HandleGetCharacterList failed: {ex.Message}");

                // 返回统一未知错误码
                response.ErrorCode = (int)ErrorCode.UnknownError;

                // 返回失败提示
                response.Message = "获取角色列表失败";

                // 失败时也返回空列表，避免客户端处理麻烦
                response.CharacterList = new List<CharacterInfo>();

                // 构造响应并返回
                return BuildGetCharacterListResponse(response);
            }
        }

        /// <summary>
        /// 处理“创建角色”请求
        /// 
        /// 流程：
        /// 1. 反序列化请求
        /// 2. 校验参数
        /// 3. 检查该用户角色数量是否达到上限
        /// 4. 根据职业配置构建默认角色属性
        /// 5. 写入数据库
        /// 6. 返回创建结果
        /// </summary>
        /// <param name="requestMessage">客户端发来的网络消息</param>
        /// <returns>创建角色响应消息</returns>
        public NetMessage HandleCreateCharacter(NetMessage requestMessage)
        {
            // 将客户端消息体反序列化为创建角色请求对象
            CreateCharacterRequest request = JsonHelper.FromJson<CreateCharacterRequest>(requestMessage.BodyJson);

            // 创建响应对象
            CreateCharacterResponse response = new CreateCharacterResponse();

            // 打印请求日志
            Logger.Info($"HandleCreateCharacter: UserId = {request.UserId}, Name = {request.Name}, Profession = {request.Profession}");

            // 校验基础参数
            // 1. 用户ID必须有效
            // 2. 名字不能为空或全空格
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Name))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "角色参数无效";
                return BuildCreateCharacterResponse(response);
            }

            try
            {
                // 1. 先检查当前用户已有多少个角色
                int currentCount = _characterRepository.GetCharacterCountByUserId(request.UserId);

                // 如果达到上限，则直接返回失败
                if (currentCount >= 10)
                {
                    response.ErrorCode = (int)ErrorCode.CharacterCountLimitReached;
                    response.Message = "角色数量已达上限";
                    return BuildCreateCharacterResponse(response);
                }

                // 2. 根据职业配置表构建角色默认属性
                // 例如：初始血量、蓝量、属性、出生地图、出生点等
                CharacterEntity newCharacter = BuildDefaultCharacterByProfession(
                    request.UserId,
                    request.Name,
                    request.Profession);

                // 3. 把角色写入数据库，并拿到数据库生成的新角色ID
                int newCharacterId = _characterRepository.Insert(newCharacter);

                // 回填角色ID到实体对象
                newCharacter.Id = newCharacterId;

                // 设置成功响应
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "创建角色成功";

                // 把创建好的角色数据转换成 CharacterInfo 返回给客户端
                response.CharacterInfo = new CharacterInfo
                {
                    CharacterId = newCharacter.Id,
                    UserId = newCharacter.UserId,
                    Name = newCharacter.Name,
                    Profession = newCharacter.Profession,
                    Level = newCharacter.Level,
                    Gold = newCharacter.Gold,
                    Hp = newCharacter.Hp,
                    Mp = newCharacter.Mp,
                    MapId = newCharacter.MapId,
                    PosX = newCharacter.PosX,
                    PosY = newCharacter.PosY,
                    PosZ = newCharacter.PosZ,
                };

                return BuildCreateCharacterResponse(response);
            }
            catch (Exception ex)
            {
                // 出异常则写日志
                Logger.Error($"HandleCreateCharacter failed: {ex.Message}");

                // 返回失败信息
                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "创建角色失败";
                return BuildCreateCharacterResponse(response);
            }
        }

        /// <summary>
        /// 根据职业配置表构建一个“默认角色实体”
        /// 
        /// 作用：
        /// 创建角色时，不是随便 new 一个空角色，
        /// 而是根据职业配置，把初始属性、初始地图、初始位置等都设好。
        /// </summary>
        /// <param name="userId">所属用户ID</param>
        /// <param name="name">角色名称</param>
        /// <param name="profession">职业ID</param>
        /// <returns>初始化完成的角色数据库实体</returns>
        private CharacterEntity BuildDefaultCharacterByProfession(int userId, string name, int profession)
        {
            // 根据职业ID从职业配置表中取出该职业的配置
            ProfessionConfig config = GameServer.Instance.ProfessionConfigManager.GetById(profession);

            // 如果配置表里没有这个职业，说明传入职业ID非法
            if (config == null)
            {
                throw new Exception($"职业配置不存在，ProfessionId = {profession}");
            }

            // 使用职业配置构建默认角色实体
            CharacterEntity entity = new CharacterEntity
            {
                // 基础信息
                UserId = userId,
                Name = name,
                Profession = profession,

                // 默认等级和金币
                Level = 1,
                Gold = 0,

                // 三维基础属性
                Strength = config.Strength,
                Agility = config.Agility,
                Intelligence = config.Intelligence,

                // 战斗相关属性
                CritRate = config.CritRate,
                CritDamage = config.CritDamage,
                Defense = config.Defense,

                // 血蓝和上限
                Hp = config.Hp,
                Mp = config.Mp,
                MaxHp = config.MaxHp,
                MaxMp = config.MaxMp,

                // 初始地图和出生点
                MapId = config.MapId,
                PosX = config.PosX,
                PosY = config.PosY,
                PosZ = config.PosZ
            };

            // 返回构建好的角色实体
            return entity;
        }

        /// <summary>
        /// 构建获取角色列表响应消息
        /// 把协议对象包装成 NetMessage
        /// </summary>
        /// <param name="response">获取角色列表响应对象</param>
        /// <returns>网络消息</returns>
        private NetMessage BuildGetCharacterListResponse(GetCharacterListResponse response)
        {
            return new NetMessage
            {
                // 设置消息号：获取角色列表响应
                MessageId = (int)MessageId.GetCharacterListResponse,

                // 把响应对象序列化成 Json 字符串
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 构建创建角色响应消息
        /// </summary>
        /// <param name="response">创建角色响应对象</param>
        /// <returns>网络消息</returns>
        private NetMessage BuildCreateCharacterResponse(CreateCharacterResponse response)
        {
            return new NetMessage
            {
                // 设置消息号：创建角色响应
                MessageId = (int)MessageId.CreateCharacterResponse,

                // 序列化响应体
                BodyJson = JsonHelper.ToJson(response)
            };
        }
    }
}