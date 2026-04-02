using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using MMOServer.Network;
using Protocol;

namespace MMOServer.Services
{
    public class ShopService
    {
        private readonly CharacterRepository _characterRepository;
        private readonly InventoryRepository _inventoryRepository;

        public ShopService()
        {
            _characterRepository = new CharacterRepository();
            _inventoryRepository = new InventoryRepository();
        }

        /// <summary>
        /// 处理购买商店道具请求
        /// </summary>
        public NetMessage HandleBuyShopItem(NetMessage requestMessage, ClientSession session)
        {
            BuyShopItemRequest request = JsonHelper.FromJson<BuyShopItemRequest>(requestMessage.BodyJson);

            BuyShopItemResponse response = new BuyShopItemResponse();

            // 1. 基础参数校验
            if (request == null || request.ShopId <= 0 || request.ItemId <= 0 || request.Quantity <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "购买请求参数无效";
                response.CharacterId = 0;
                response.ShopId = request?.ShopId ?? 0;
                response.ItemId = request?.ItemId ?? 0;
                response.BuyQuantity = request?.Quantity ?? 0;
                response.RemainingLimitCount = -1;
                response.CharacterInfo = null;
                response.ItemList = new List<InventoryItemInfo>();
                return BuildBuyShopItemResponse(response);
            }

            // 2. session 校验
            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法购买";
                response.CharacterId = 0;
                response.ShopId = request.ShopId;
                response.ItemId = request.ItemId;
                response.BuyQuantity = request.Quantity;
                response.RemainingLimitCount = -1;
                response.CharacterInfo = null;
                response.ItemList = new List<InventoryItemInfo>();
                return BuildBuyShopItemResponse(response);
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
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = -1;
                    response.CharacterInfo = null;
                    response.ItemList = new List<InventoryItemInfo>();
                    return BuildBuyShopItemResponse(response);
                }

                // 4. 校验该商品是否存在于该商店
                ShopItemConfig shopItemConfig = GetShopItemConfig(request.ShopId, request.ItemId);
                if (shopItemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该商店不存在此商品";
                    response.CharacterId = characterId;
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = -1;
                    response.CharacterInfo = ToCharacterInfo(character);
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    return BuildBuyShopItemResponse(response);
                }

                // 5. 限购校验（当前简化版）
                // 如果不限购，RemainingLimitCount 统一返回 -1
                // 如果限购，这里先按“本次购买不能超过配置上限”做最基础限制
                if (shopItemConfig.IsLimited == 1)
                {
                    if (shopItemConfig.LimitCount <= 0)
                    {
                        response.ErrorCode = (int)ErrorCode.UnknownError;
                        response.Message = "商品限购配置无效";
                        response.CharacterId = characterId;
                        response.ShopId = request.ShopId;
                        response.ItemId = request.ItemId;
                        response.BuyQuantity = request.Quantity;
                        response.RemainingLimitCount = 0;
                        response.CharacterInfo = ToCharacterInfo(character);
                        response.ItemList = BuildInventoryItemInfoList(characterId);
                        return BuildBuyShopItemResponse(response);
                    }

                    if (request.Quantity > shopItemConfig.LimitCount)
                    {
                        response.ErrorCode = (int)ErrorCode.InvalidParams;
                        response.Message = "购买数量超过当前商品限购数量";
                        response.CharacterId = characterId;
                        response.ShopId = request.ShopId;
                        response.ItemId = request.ItemId;
                        response.BuyQuantity = request.Quantity;
                        response.RemainingLimitCount = shopItemConfig.LimitCount;
                        response.CharacterInfo = ToCharacterInfo(character);
                        response.ItemList = BuildInventoryItemInfoList(characterId);
                        return BuildBuyShopItemResponse(response);
                    }
                }

                // 6. 查道具配置
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(request.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "道具配置不存在";
                    response.CharacterId = characterId;
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = shopItemConfig.IsLimited == 1 ? shopItemConfig.LimitCount : -1;
                    response.CharacterInfo = ToCharacterInfo(character);
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    return BuildBuyShopItemResponse(response);
                }

                // 7. 计算总价
                int totalPrice = shopItemConfig.Price * request.Quantity;

                if (totalPrice <= 0)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "商品价格配置无效";
                    response.CharacterId = characterId;
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = shopItemConfig.IsLimited == 1 ? shopItemConfig.LimitCount : -1;
                    response.CharacterInfo = ToCharacterInfo(character);
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    return BuildBuyShopItemResponse(response);
                }

                // 8. 金币是否足够
                if (character.Gold < totalPrice)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "金币不足";
                    response.CharacterId = characterId;
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = shopItemConfig.IsLimited == 1 ? shopItemConfig.LimitCount : -1;
                    response.CharacterInfo = ToCharacterInfo(character);
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    return BuildBuyShopItemResponse(response);
                }

                // 9. 扣金币
                int newGold = character.Gold - totalPrice;
                _characterRepository.UpdateCharacterGold(characterId, newGold);
                character.Gold = newGold;

                // 10. 加物品进背包
                AddItemToInventory(characterId, request.ItemId, request.Quantity, itemConfig.MaxStackCount);

                // 11. 背包规范化（去空洞）
                List<InventoryItemInfo> normalizedItemList = NormalizeInventory(characterId);

                // 12. 计算剩余限购数量（当前简化版）
                int remainingLimitCount = -1;
                if (shopItemConfig.IsLimited == 1)
                {
                    remainingLimitCount = shopItemConfig.LimitCount - request.Quantity;
                    if (remainingLimitCount < 0)
                    {
                        remainingLimitCount = 0;
                    }
                }

                // 13. 返回最新结果
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"购买成功：{itemConfig.ItemName} x{request.Quantity}";
                response.CharacterId = characterId;
                response.ShopId = request.ShopId;
                response.ItemId = request.ItemId;
                response.BuyQuantity = request.Quantity;
                response.RemainingLimitCount = remainingLimitCount;
                response.CharacterInfo = ToCharacterInfo(character);
                response.ItemList = normalizedItemList;

                return BuildBuyShopItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleBuyShopItem failed: ShopId={request?.ShopId}, ItemId={request?.ItemId}, Quantity={request?.Quantity}, SessionCharacterId={session?.CurrentCharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "购买失败";
                response.CharacterId = session?.CurrentCharacterId ?? 0;
                response.ShopId = request?.ShopId ?? 0;
                response.ItemId = request?.ItemId ?? 0;
                response.BuyQuantity = request?.Quantity ?? 0;
                response.RemainingLimitCount = -1;
                response.CharacterInfo = session != null && session.CurrentCharacterId > 0
                    ? BuildLatestCharacterInfoSafe(session.CurrentCharacterId)
                    : null;
                response.ItemList = session != null && session.CurrentCharacterId > 0
                    ? BuildInventoryItemInfoList(session.CurrentCharacterId)
                    : new List<InventoryItemInfo>();

                return BuildBuyShopItemResponse(response);
            }
        }

        /// <summary>
        /// 根据 ShopId + ItemId 获取商店商品配置
        /// </summary>
        private ShopItemConfig GetShopItemConfig(int shopId, int itemId)
        {
            List<ShopItemConfig> shopItemList = GameServer.Instance.ShopItemConfigManager.GetByShopId(shopId);
            return shopItemList.FirstOrDefault(x => x.ItemId == itemId);
        }

        /// <summary>
        /// 往背包添加物品
        /// 1. 优先堆叠到已有同类格子
        /// 2. 剩余数量再找空格创建新格子
        /// </summary>
        private void AddItemToInventory(int characterId, int itemId, int quantity, int maxStackCount)
        {
            if (quantity <= 0)
            {
                return;
            }

            List<InventoryItemEntity> inventoryList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            // 1. 先堆叠已有同类物品
            foreach (InventoryItemEntity inventoryItem in inventoryList)
            {
                if (inventoryItem.ItemId != itemId)
                {
                    continue;
                }

                if (inventoryItem.Count >= maxStackCount)
                {
                    continue;
                }

                int canAdd = maxStackCount - inventoryItem.Count;
                int addCount = Math.Min(canAdd, quantity);

                if (addCount > 0)
                {
                    _inventoryRepository.UpdateItemCount(inventoryItem.Id, inventoryItem.Count + addCount);
                    inventoryItem.Count += addCount;
                    quantity -= addCount;
                }

                if (quantity <= 0)
                {
                    return;
                }
            }

            // 2. 再找空格创建新物品格子
            HashSet<int> usedSlotSet = inventoryList
                .Select(x => x.SlotIndex)
                .ToHashSet();

            int nextSlotIndex = 0;

            while (quantity > 0)
            {
                while (usedSlotSet.Contains(nextSlotIndex))
                {
                    nextSlotIndex++;
                }

                int addCount = Math.Min(maxStackCount, quantity);

                InventoryItemEntity newItem = new InventoryItemEntity
                {
                    CharacterId = characterId,
                    SlotIndex = nextSlotIndex,
                    ItemId = itemId,
                    Count = addCount
                };

                _inventoryRepository.Insert(newItem);

                usedSlotSet.Add(nextSlotIndex);
                quantity -= addCount;
                nextSlotIndex++;
            }
        }

        /// <summary>
        /// 规范化当前角色背包（去空洞）
        /// </summary>
        private List<InventoryItemInfo> NormalizeInventory(int characterId)
        {
            List<InventoryItemEntity> itemList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            if (itemList == null || itemList.Count == 0)
            {
                return new List<InventoryItemInfo>();
            }

            List<InventoryItemEntity> orderedList = itemList
                .OrderBy(item => item.SlotIndex)
                .ToList();

            for (int i = 0; i < orderedList.Count; i++)
            {
                InventoryItemEntity item = orderedList[i];

                if (item.SlotIndex != i)
                {
                    _inventoryRepository.UpdateItemSlotIndex(item.Id, i);
                    item.SlotIndex = i;
                }
            }

            return orderedList.Select(ToInventoryItemInfo).ToList();
        }

        /// <summary>
        /// session 是否有效
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
        /// 获取角色当前背包列表
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
        /// 构建购买商店道具响应消息
        /// </summary>
        private NetMessage BuildBuyShopItemResponse(BuyShopItemResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.BuyShopItemResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }
    }
}