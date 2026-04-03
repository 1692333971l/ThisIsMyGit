using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using MMOServer.Network;
using MMOServer.Services.Common;
using Protocol;

namespace MMOServer.Services
{
    public class ShopService
    {
        private const int MaxSlotCount = 72;
        private readonly CharacterRepository _characterRepository;
        private readonly InventoryRepository _inventoryRepository;
        private readonly InventoryDomainService _inventoryDomainService;

        public ShopService()
        {
            _characterRepository = new CharacterRepository();
            _inventoryRepository = new InventoryRepository();
            _inventoryDomainService = new InventoryDomainService();
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

                // 10. 背包容量校验
                if (!_inventoryDomainService.CanAddItem(characterId, request.ItemId, request.Quantity, itemConfig.MaxStackCount))
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "背包空间不足";
                    response.CharacterId = characterId;
                    response.ShopId = request.ShopId;
                    response.ItemId = request.ItemId;
                    response.BuyQuantity = request.Quantity;
                    response.RemainingLimitCount = shopItemConfig.IsLimited == 1 ? shopItemConfig.LimitCount : -1;
                    response.CharacterInfo = ToCharacterInfo(character);
                    response.ItemList = BuildInventoryItemInfoList(characterId);
                    return BuildBuyShopItemResponse(response);
                }

                // 11. 加物品进背包
                _inventoryDomainService.AddItem(characterId, request.ItemId, request.Quantity, itemConfig.MaxStackCount);

                // 12. 背包规范化
                List<InventoryItemInfo> normalizedItemList = _inventoryDomainService.NormalizeInventory(characterId);

                // 13. 计算剩余限购数量（当前简化版）
                int remainingLimitCount = -1;
                if (shopItemConfig.IsLimited == 1)
                {
                    remainingLimitCount = shopItemConfig.LimitCount - request.Quantity;
                    if (remainingLimitCount < 0)
                    {
                        remainingLimitCount = 0;
                    }
                }

                // 14. 返回最新结果
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