using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services
{
    public class CharacterService
    {
        private readonly CharacterRepository _characterRepository;

        public CharacterService()
        {
            _characterRepository = new CharacterRepository();
        }

        /// <summary>
        /// 处理获取角色列表请求
        /// </summary>
        public NetMessage HandleGetCharacterList(NetMessage requestMessage)
        {
            GetCharacterListRequest request = JsonHelper.FromJson<GetCharacterListRequest>(requestMessage.BodyJson);
            GetCharacterListResponse response = new GetCharacterListResponse();

            Logger.Info($"HandleGetCharacterList: UserId = {request.UserId}");

            if (request.UserId <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "用户ID无效";
                response.CharacterList = new List<CharacterInfo>();
                return BuildGetCharacterListResponse(response);
            }

            try
            {
                List<CharacterEntity> entityList = _characterRepository.GetCharacterListByUserId(request.UserId);

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "获取角色列表成功";
                response.CharacterList = entityList.Select(e => new CharacterInfo
                {
                    CharacterId = e.Id,
                    UserId = e.UserId,
                    Name = e.Name,
                    Profession = e.Profession,
                    Level = e.Level,
                    Gold = e.Gold,
                    Hp = e.Hp,
                    Mp = e.Mp
                }).ToList();

                return BuildGetCharacterListResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleGetCharacterList failed: {ex.Message}");

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "获取角色列表失败";
                response.CharacterList = new List<CharacterInfo>();
                return BuildGetCharacterListResponse(response);
            }
        }

        /// <summary>
        /// 处理创建角色请求
        /// </summary>
        public NetMessage HandleCreateCharacter(NetMessage requestMessage)
        {
            CreateCharacterRequest request = JsonHelper.FromJson<CreateCharacterRequest>(requestMessage.BodyJson);
            CreateCharacterResponse response = new CreateCharacterResponse();

            Logger.Info($"HandleCreateCharacter: UserId = {request.UserId}, Name = {request.Name}, Profession = {request.Profession}");

            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.Name))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "角色参数无效";
                return BuildCreateCharacterResponse(response);
            }

            try
            {
                // 1. 角色数量是否超上限
                int currentCount = _characterRepository.GetCharacterCountByUserId(request.UserId);
                if (currentCount >= 10)
                {
                    response.ErrorCode = (int)ErrorCode.CharacterCountLimitReached;
                    response.Message = "角色数量已达上限";
                    return BuildCreateCharacterResponse(response);
                }

                // 2. 通过职业配置初始化角色
                CharacterEntity newCharacter = BuildDefaultCharacterByProfession(
                    request.UserId,
                    request.Name,
                    request.Profession);

                // 3. 写入数据库
                int newCharacterId = _characterRepository.Insert(newCharacter);
                newCharacter.Id = newCharacterId;

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "创建角色成功";
                response.CharacterInfo = new CharacterInfo
                {
                    CharacterId = newCharacter.Id,
                    UserId = newCharacter.UserId,
                    Name = newCharacter.Name,
                    Profession = newCharacter.Profession,
                    Level = newCharacter.Level,
                    Gold = newCharacter.Gold,
                    Hp = newCharacter.Hp,
                    Mp = newCharacter.Mp
                };

                return BuildCreateCharacterResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleCreateCharacter failed: {ex.Message}");

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "创建角色失败";
                return BuildCreateCharacterResponse(response);
            }
        }

        /// <summary>
        /// 根据职业配置构建默认角色数据
        /// </summary>
        private CharacterEntity BuildDefaultCharacterByProfession(int userId, string name, int profession)
        {
            ProfessionConfig config = GameServer.Instance.ProfessionConfigManager.GetById(profession);
            if (config == null)
            {
                throw new Exception($"职业配置不存在，ProfessionId = {profession}");
            }

            CharacterEntity entity = new CharacterEntity
            {
                UserId = userId,
                Name = name,
                Profession = profession,

                Level = 1,
                Gold = 0,

                Strength = config.Strength,
                Agility = config.Agility,
                Intelligence = config.Intelligence,

                CritRate = config.CritRate,
                CritDamage = config.CritDamage,
                Defense = config.Defense,

                Hp = config.Hp,
                Mp = config.Mp,
                MaxHp = config.MaxHp,
                MaxMp = config.MaxMp,

                MapId = config.MapId,
                PosX = config.PosX,
                PosY = config.PosY,
                PosZ = config.PosZ
            };

            return entity;
        }

        private NetMessage BuildGetCharacterListResponse(GetCharacterListResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.GetCharacterListResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        private NetMessage BuildCreateCharacterResponse(CreateCharacterResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.CreateCharacterResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }
    }
}