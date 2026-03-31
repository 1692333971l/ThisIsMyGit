using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly CharacterRepository _characterRepository;

        private const int UseEffectTypeRestoreHp = 1;// 1 = 回复HP
        private const int UseEffectTypeRestoreMp = 2;// 2 = 回复MP

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
            _characterRepository = new CharacterRepository();
        }

        /// <summary>
        /// 处理获取背包请求
        /// </summary>
        public NetMessage HandleGetInventory(NetMessage requestMessage)
        {
            GetInventoryRequest request = JsonHelper.FromJson<GetInventoryRequest>(requestMessage.BodyJson);

            GetInventoryResponse response = new GetInventoryResponse();

            if (request == null || request.CharacterId <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "背包请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();

                return BuildGetInventoryResponse(response);
            }

            try
            {
                List<InventoryItemEntity> entityList = _inventoryRepository.GetInventoryListByCharacterId(request.CharacterId);

                List<InventoryItemInfo> itemInfoList = new List<InventoryItemInfo>();

                foreach (InventoryItemEntity entity in entityList)
                {
                    itemInfoList.Add(ToInventoryItemInfo(entity));
                }

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "获取背包成功";
                response.CharacterId = request.CharacterId;
                response.ItemList = itemInfoList;

                return BuildGetInventoryResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleGetInventory failed: CharacterId={request.CharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "获取背包失败";
                response.CharacterId = request.CharacterId;
                response.ItemList = new List<InventoryItemInfo>();

                return BuildGetInventoryResponse(response);
            }
        }

        /// <summary>
        /// 处理使用道具请求
        /// </summary>
        public NetMessage HandleUseItem(NetMessage requestMessage)
        {
            UseItemRequest request = JsonHelper.FromJson<UseItemRequest>(requestMessage.BodyJson);

            UseItemResponse response = new UseItemResponse();

            if (request == null || request.CharacterId <= 0 || request.SlotIndex < 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "使用道具请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.CharacterInfo = null;
                return BuildUseItemResponse(response);
            }

            try
            {
                // 1. 先查角色
                CharacterEntity character = _characterRepository.GetByCharacterId(request.CharacterId);

                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = request.CharacterId;
                    response.ItemList = new List<InventoryItemInfo>();
                    response.CharacterInfo = null;
                    return BuildUseItemResponse(response);
                }

                // 2. 查这个格子里有没有道具
                InventoryItemEntity inventoryItem = _inventoryRepository.GetByCharacterIdAndSlotIndex(
                    request.CharacterId,
                    request.SlotIndex
                );

                if (inventoryItem == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该格子没有可使用的道具";
                    response.CharacterId = request.CharacterId;
                    response.ItemList = BuildInventoryItemInfoList(request.CharacterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildUseItemResponse(response);
                }

                // 3. 查道具配置
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(inventoryItem.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "道具配置不存在";
                    response.CharacterId = request.CharacterId;
                    response.ItemList = BuildInventoryItemInfoList(request.CharacterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildUseItemResponse(response);
                }

                // 4. 判定是否可使用
                if (itemConfig.CanUse != 1)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该道具不可使用";
                    response.CharacterId = request.CharacterId;
                    response.ItemList = BuildInventoryItemInfoList(request.CharacterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildUseItemResponse(response);
                }

                // 5. 应用使用效果
                int newHp = character.Hp;
                int newMp = character.Mp;

                switch (itemConfig.UseEffectType)
                {
                    case UseEffectTypeRestoreHp:
                        newHp += itemConfig.UseEffectValue;
                        if (newHp > character.MaxHp)
                        {
                            newHp = character.MaxHp;
                        }
                        break;

                    case UseEffectTypeRestoreMp:
                        newMp += itemConfig.UseEffectValue;
                        if (newMp > character.MaxMp)
                        {
                            newMp = character.MaxMp;
                        }
                        break;

                    default:
                        response.ErrorCode = (int)ErrorCode.UnknownError;
                        response.Message = "暂不支持该道具效果类型";
                        response.CharacterId = request.CharacterId;
                        response.ItemList = BuildInventoryItemInfoList(request.CharacterId);
                        response.CharacterInfo = ToCharacterInfo(character);
                        return BuildUseItemResponse(response);
                }

                // 6. 更新角色 HP / MP
                _characterRepository.UpdateCharacterHpMp(request.CharacterId, newHp, newMp);

                // 7. 扣除背包数量
                int leftCount = inventoryItem.Count - 1;
                if (leftCount > 0)
                {
                    _inventoryRepository.UpdateItemCount(inventoryItem.Id, leftCount);
                }
                else
                {
                    _inventoryRepository.DeleteById(inventoryItem.Id);
                }

                // 8. 重新组装最新角色信息
                character.Hp = newHp;
                character.Mp = newMp;

                // 9. 返回最新结果
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"使用道具成功：{itemConfig.ItemName}";
                response.CharacterId = request.CharacterId;
                response.ItemList = BuildInventoryItemInfoList(request.CharacterId);
                response.CharacterInfo = ToCharacterInfo(character);

                return BuildUseItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleUseItem failed: CharacterId={request?.CharacterId}, SlotIndex={request?.SlotIndex}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "使用道具失败";
                response.CharacterId = request?.CharacterId ?? 0;
                response.ItemList = request != null && request.CharacterId > 0
                    ? BuildInventoryItemInfoList(request.CharacterId)
                    : new List<InventoryItemInfo>();
                response.CharacterInfo = request != null && request.CharacterId > 0
                    ? BuildLatestCharacterInfoSafe(request.CharacterId)
                    : null;

                return BuildUseItemResponse(response);
            }
        }

        /// <summary>
        /// 数据库实体 -> 协议对象
        /// </summary>
        private InventoryItemInfo ToInventoryItemInfo(InventoryItemEntity entity)
        {
            return new InventoryItemInfo
            {
                SlotIndex = entity.SlotIndex,
                ItemId = entity.ItemId,
                Count = entity.Count
            };
        }

        /// <summary>
        /// CharacterEntity -> CharacterInfo
        /// </summary>
        private CharacterInfo ToCharacterInfo(CharacterEntity character)
        {
            if (character == null)
            {
                return null;
            }

            return new CharacterInfo
            {
                CharacterId = character.Id,
                UserId = character.UserId,
                Name = character.Name,
                Profession = character.Profession,
                Level = character.Level,
                Exp = character.Exp,
                Gold = character.Gold,
                Strength = character.Strength,
                Agility = character.Agility,
                Intelligence = character.Intelligence,
                CritRate = character.CritRate,
                CritDamage = character.CritDamage,
                Defense = character.Defense,
                Hp = character.Hp,
                Mp = character.Mp,
                MaxHp = character.MaxHp,
                MaxMp = character.MaxMp,
                MapId = character.MapId,
                PosX = character.PosX,
                PosY = character.PosY,
                PosZ = character.PosZ
            };
        }

        /// <summary>
        /// 获取角色最新背包列表（返回给客户端刷新UI）
        /// </summary>
        private List<InventoryItemInfo> BuildInventoryItemInfoList(int characterId)
        {
            List<InventoryItemEntity> entityList = _inventoryRepository.GetInventoryListByCharacterId(characterId);
            List<InventoryItemInfo> itemInfoList = new List<InventoryItemInfo>();

            foreach (InventoryItemEntity entity in entityList)
            {
                itemInfoList.Add(ToInventoryItemInfo(entity));
            }

            return itemInfoList;
        }

        /// <summary>
        /// 安全获取最新角色信息
        /// </summary>
        private CharacterInfo BuildLatestCharacterInfoSafe(int characterId)
        {
            try
            {
                CharacterEntity character = _characterRepository.GetByCharacterId(characterId);
                return ToCharacterInfo(character);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 构建获取背包响应消息
        /// </summary>
        private NetMessage BuildGetInventoryResponse(GetInventoryResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.GetInventoryResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 构建使用道具响应消息
        /// </summary>
        private NetMessage BuildUseItemResponse(UseItemResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.UseItemResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }
    }
}