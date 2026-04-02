using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using MMOServer.Network;
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
        public NetMessage HandleGetInventory(NetMessage requestMessage, ClientSession session)
        {
            GetInventoryRequest request = JsonHelper.FromJson<GetInventoryRequest>(requestMessage.BodyJson);

            GetInventoryResponse response = new GetInventoryResponse();

            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法获取背包";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                return BuildGetInventoryResponse(response);
            }

            try
            {
                int characterId = session.CurrentCharacterId;

                List<InventoryItemEntity> entityList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

                List<InventoryItemInfo> itemInfoList = new List<InventoryItemInfo>();

                foreach (InventoryItemEntity entity in entityList)
                {
                    itemInfoList.Add(ToInventoryItemInfo(entity));
                }
                // 规范化背包
                List<InventoryItemInfo> normalizedItemList = NormalizeInventory(characterId);

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "获取背包成功";
                response.CharacterId = characterId;
                response.ItemList = normalizedItemList;

                return BuildGetInventoryResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleGetInventory failed: SessionCharacterId={session?.CurrentCharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "获取背包失败";
                response.CharacterId = session?.CurrentCharacterId ?? 0;
                response.ItemList = new List<InventoryItemInfo>();

                return BuildGetInventoryResponse(response);
            }
        }

        /// <summary>
        /// 处理使用道具请求
        /// </summary>
        public NetMessage HandleUseItem(NetMessage requestMessage, ClientSession session)
        {
            UseItemRequest request = JsonHelper.FromJson<UseItemRequest>(requestMessage.BodyJson);

            UseItemResponse response = new UseItemResponse();

            if (request == null || request.SlotIndex < 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "使用道具请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.CharacterInfo = null;
                return BuildUseItemResponse(response);
            }

            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法使用道具";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.CharacterInfo = null;
                return BuildUseItemResponse(response);
            }

            try
            {
                int characterId = session.CurrentCharacterId;

                // 1. 先查角色
                CharacterEntity character = _characterRepository.GetByCharacterId(characterId);

                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = characterId;
                    response.ItemList = new List<InventoryItemInfo>();
                    response.CharacterInfo = null;
                    return BuildUseItemResponse(response);
                }

                // 2. 查这个格子里有没有道具
                InventoryItemEntity inventoryItem = _inventoryRepository.GetByCharacterIdAndSlotIndex(
                    characterId,
                    request.SlotIndex
                );

                if (inventoryItem == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该格子没有可使用的道具";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildUseItemResponse(response);
                }

                // 3. 查道具配置
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(inventoryItem.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "道具配置不存在";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildUseItemResponse(response);
                }

                // 4. 判定是否可使用
                if (itemConfig.CanUse != 1)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该道具不可使用";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
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
                        response.CharacterId = characterId;
                        response.ItemList = BuildInventoryItemInfoList(characterId);
                        response.CharacterInfo = ToCharacterInfo(character);
                        return BuildUseItemResponse(response);
                }

                // 6. 更新角色 HP / MP
                _characterRepository.UpdateCharacterHpMp(characterId, newHp, newMp);

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

                // 9. 规范化背包
                List<InventoryItemInfo> normalizedItemList = NormalizeInventory(characterId);

                // 10. 返回最新结果
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"使用道具成功：{itemConfig.ItemName}";
                response.CharacterId = characterId;
                response.ItemList = normalizedItemList;
                response.CharacterInfo = ToCharacterInfo(character);

                return BuildUseItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleUseItem failed: CharacterId={session?.CurrentCharacterId}, SlotIndex={request?.SlotIndex}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "使用道具失败";
                response.CharacterId = session?.CurrentCharacterId ?? 0;
                response.ItemList = session != null && session.CurrentCharacterId > 0
                    ? BuildInventoryItemInfoList(session.CurrentCharacterId)
                    : new List<InventoryItemInfo>();
                response.CharacterInfo = session != null && session.CurrentCharacterId > 0
                    ? BuildLatestCharacterInfoSafe(session.CurrentCharacterId)
                    : null;

                return BuildUseItemResponse(response);
            }
        }

        /// <summary>
        /// 处理出售道具请求
        /// </summary>
        public NetMessage HandleSellItem(NetMessage requestMessage, ClientSession session)
        {
            SellItemRequest request = JsonHelper.FromJson<SellItemRequest>(requestMessage.BodyJson);

            SellItemResponse response = new SellItemResponse();

            // 1. 基础参数校验
            if (request == null || request.SlotIndex < 0 || request.Quantity <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "出售道具请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.CharacterInfo = null;
                return BuildSellItemResponse(response);
            }

            // 2. session 校验
            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法出售道具";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.CharacterInfo = null;
                return BuildSellItemResponse(response);
            }

            try
            {
                int characterId = session.CurrentCharacterId;

                // 3. 查角色
                CharacterEntity character = _characterRepository.GetByCharacterId(characterId);
                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = characterId;
                    response.ItemList = new List<InventoryItemInfo>();
                    response.CharacterInfo = null;
                    return BuildSellItemResponse(response);
                }

                // 4. 查背包格子物品
                InventoryItemEntity inventoryItem = _inventoryRepository.GetByCharacterIdAndSlotIndex(
                    characterId,
                    request.SlotIndex
                );

                if (inventoryItem == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该格子没有可出售的道具";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildSellItemResponse(response);
                }

                // 5. 校验出售数量
                if (request.Quantity > inventoryItem.Count)
                {
                    response.ErrorCode = (int)ErrorCode.InvalidParams;
                    response.Message = "出售数量超过背包中实际数量";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildSellItemResponse(response);
                }

                // 6. 查道具配置
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(inventoryItem.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "道具配置不存在";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildSellItemResponse(response);
                }

                // 7. 校验是否可出售
                // 这里先按 SellPrice <= 0 视为不可出售
                if (itemConfig.SellPrice <= 0)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该道具不可出售";
                    response.CharacterId = characterId;
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    response.CharacterInfo = ToCharacterInfo(character);
                    return BuildSellItemResponse(response);
                }

                // 8. 计算本次出售获得金币
                int addGold = itemConfig.SellPrice * request.Quantity;
                int newGold = character.Gold + addGold;

                // 9. 更新角色金币
                _characterRepository.UpdateCharacterGold(characterId, newGold);

                // 10. 扣减背包数量
                int leftCount = inventoryItem.Count - request.Quantity;
                if (leftCount > 0)
                {
                    _inventoryRepository.UpdateItemCount(inventoryItem.Id, leftCount);
                }
                else
                {
                    _inventoryRepository.DeleteById(inventoryItem.Id);
                }

                // 11. 回写最新角色状态
                character.Gold = newGold;

                // 12. 规范化背包
                List<InventoryItemInfo> normalizedItemList = NormalizeInventory(characterId);

                // 13. 返回最新结果
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"出售道具成功：{itemConfig.ItemName} x{request.Quantity}，获得金币 {addGold}";
                response.CharacterId = characterId;
                response.ItemList = normalizedItemList;
                response.CharacterInfo = ToCharacterInfo(character);

                return BuildSellItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleSellItem failed: SlotIndex={request?.SlotIndex}, Quantity={request?.Quantity}, SessionCharacterId={session?.CurrentCharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "出售道具失败";
                response.CharacterId = session?.CurrentCharacterId ?? 0;
                response.ItemList = session != null && session.CurrentCharacterId > 0
                    ? BuildInventoryItemInfoList(session.CurrentCharacterId)
                    : new List<InventoryItemInfo>();
                response.CharacterInfo = session != null && session.CurrentCharacterId > 0
                    ? BuildLatestCharacterInfoSafe(session.CurrentCharacterId)
                    : null;

                return BuildSellItemResponse(response);
            }
        }
        /// <summary>
        /// 规范化当前角色背包
        /// 1. 合并同类物品堆叠
        /// 2. 按最大堆叠数重新拆分
        /// 3. 去掉空洞
        /// 4. 从 0 开始重新连续分配 SlotIndex
        /// </summary>
        private List<InventoryItemInfo> NormalizeInventory(int characterId)
        {
            // 1. 读取当前角色全部背包物品
            List<InventoryItemEntity> itemList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            if (itemList == null || itemList.Count == 0)
            {
                return new List<InventoryItemInfo>();
            }

            // 2. 按 ItemId 分组，并统计总数量
            Dictionary<int, int> totalCountDict = new Dictionary<int, int>();

            foreach (InventoryItemEntity item in itemList)
            {
                if (!totalCountDict.ContainsKey(item.ItemId))
                {
                    totalCountDict[item.ItemId] = 0;
                }

                totalCountDict[item.ItemId] += item.Count;
            }

            // 3. 按“原始最早出现顺序”保留物品种类顺序
            // 这样整理后物品大类顺序更稳定，不会每次都乱跳
            List<int> orderedItemIdList = itemList
                .OrderBy(x => x.SlotIndex)
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

            // 4. 根据总数量 + MaxStackCount 重新构建标准背包数据
            List<InventoryItemEntity> normalizedList = new List<InventoryItemEntity>();
            int nextSlotIndex = 0;

            foreach (int itemId in orderedItemIdList)
            {
                int totalCount = totalCountDict[itemId];

                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(itemId);
                if (itemConfig == null)
                {
                    throw new Exception($"道具配置不存在，ItemId = {itemId}");
                }

                int maxStackCount = itemConfig.MaxStackCount;
                if (maxStackCount <= 0)
                {
                    throw new Exception($"道具最大堆叠数量配置无效，ItemId = {itemId}");
                }

                while (totalCount > 0)
                {
                    int stackCount = Math.Min(totalCount, maxStackCount);

                    normalizedList.Add(new InventoryItemEntity
                    {
                        CharacterId = characterId,
                        SlotIndex = nextSlotIndex,
                        ItemId = itemId,
                        Count = stackCount
                    });

                    totalCount -= stackCount;
                    nextSlotIndex++;
                }
            }

            // 5. 清空旧背包记录
            _inventoryRepository.DeleteAllByCharacterId(characterId);

            // 6. 插入新的规范化背包记录
            foreach (InventoryItemEntity item in normalizedList)
            {
                _inventoryRepository.Insert(item);
            }

            // 7. 返回整理后的最新协议数据
            return normalizedList.Select(ToInventoryItemInfo).ToList();
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

        /// <summary>
        /// 构建出售道具响应消息
        /// </summary>
        private NetMessage BuildSellItemResponse(SellItemResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.SellItemResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }
        /// <summary>
        /// 校验是否有效会话（是否已登录，是否选了角色）
        /// </summary>
        private bool IsSessionValid(ClientSession session)
        {
            if (session == null)
            {
                return false;
            }

            if (session.UserId <= 0)
            {
                return false;
            }

            if (session.CurrentCharacterId <= 0)
            {
                return false;
            }

            return true;
        }
    }
}