using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using MMOServer.Network;
using MMOServer.Services.Common;
using Protocol;

namespace MMOServer.Services
{
    public class EquipmentService
    {
        private readonly CharacterRepository _characterRepository;
        private readonly InventoryRepository _inventoryRepository;
        private readonly EquipmentRepository _equipmentRepository;
        private readonly InventoryDomainService _inventoryDomainService;
        private readonly EquipmentDomainService _equipmentDomainService;

        public EquipmentService()
        {
            _characterRepository = new CharacterRepository();
            _inventoryRepository = new InventoryRepository();
            _equipmentRepository = new EquipmentRepository();
            _inventoryDomainService = new InventoryDomainService();
            _equipmentDomainService = new EquipmentDomainService();
        }

        /// <summary>
        /// 获取当前装备栏
        /// </summary>
        public NetMessage HandleGetEquipment(NetMessage requestMessage, ClientSession session)
        {
            GetEquipmentRequest request = JsonHelper.FromJson<GetEquipmentRequest>(requestMessage.BodyJson);

            GetEquipmentResponse response = new GetEquipmentResponse();

            if (request == null || request.CharacterId <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "获取装备请求参数无效";
                response.CharacterId = 0;
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildGetEquipmentResponse(response);
            }

            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法获取装备";
                response.CharacterId = 0;
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildGetEquipmentResponse(response);
            }

            if (request.CharacterId != session.CurrentCharacterId)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "角色与当前会话不匹配";
                response.CharacterId = request.CharacterId;
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildGetEquipmentResponse(response);
            }

            try
            {
                CharacterEntity character = _characterRepository.GetByCharacterId(request.CharacterId);
                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = request.CharacterId;
                    response.EquipmentList = new List<EquipmentItemInfo>();
                    response.CharacterInfo = null;
                    return BuildGetEquipmentResponse(response);
                }

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = "获取装备成功";
                response.CharacterId = request.CharacterId;
                response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(request.CharacterId);
                response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);

                return BuildGetEquipmentResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleGetEquipment failed: CharacterId={request?.CharacterId}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "获取装备失败";
                response.CharacterId = request?.CharacterId ?? 0;
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;

                return BuildGetEquipmentResponse(response);
            }
        }

        /// <summary>
        /// 装备背包中的道具
        /// </summary>
        public NetMessage HandleEquipItem(NetMessage requestMessage, ClientSession session)
        {
            EquipItemRequest request = JsonHelper.FromJson<EquipItemRequest>(requestMessage.BodyJson);

            EquipItemResponse response = new EquipItemResponse();

            if (request == null || request.CharacterId <= 0 || request.SlotIndex < 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "装备请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildEquipItemResponse(response);
            }

            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法装备";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildEquipItemResponse(response);
            }

            if (request.CharacterId != session.CurrentCharacterId)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "角色与当前会话不匹配";
                response.CharacterId = request.CharacterId;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildEquipItemResponse(response);
            }

            try
            {
                int characterId = request.CharacterId;

                CharacterEntity character = _characterRepository.GetByCharacterId(characterId);
                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = null;
                    return BuildEquipItemResponse(response);
                }

                // 1. 查背包格子里的物品
                InventoryItemEntity inventoryItem = _inventoryRepository.GetByCharacterIdAndSlotIndex(characterId, request.SlotIndex);
                if (inventoryItem == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该格子没有可装备的道具";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildEquipItemResponse(response);
                }

                // 2. 查道具配置
                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(inventoryItem.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "道具配置不存在";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildEquipItemResponse(response);
                }

                // 3. 校验是否可装备
                if (itemConfig.CanEquip != 1)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该道具不可装备";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildEquipItemResponse(response);
                }

                // 4. 装备类道具默认必须是单堆叠
                if (inventoryItem.Count <= 0)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "装备数量无效";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildEquipItemResponse(response);
                }

                int equipSlotType = itemConfig.EquipSlotType;

                // 5. 如果该槽位已有装备，则需要先把旧装备放回背包
                CharacterEquipmentEntity oldEquipment = _equipmentRepository.GetByCharacterIdAndSlotType(characterId, equipSlotType);
                if (oldEquipment != null)
                {
                    ItemConfig oldItemConfig = GameServer.Instance.ItemConfigManager.GetById(oldEquipment.ItemId);
                    if (oldItemConfig == null)
                    {
                        response.ErrorCode = (int)ErrorCode.UnknownError;
                        response.Message = "原装备配置不存在";
                        response.CharacterId = characterId;
                        response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                        response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                        response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                        return BuildEquipItemResponse(response);
                    }

                    // 先判断旧装备回背包是否放得下
                    if (!_inventoryDomainService.CanAddItem(characterId, oldEquipment.ItemId, 1, oldItemConfig.MaxStackCount))
                    {
                        response.ErrorCode = (int)ErrorCode.UnknownError;
                        response.Message = "背包空间不足，无法替换装备";
                        response.CharacterId = characterId;
                        response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                        response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                        response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                        return BuildEquipItemResponse(response);
                    }

                    // 把旧装备放回背包
                    _inventoryDomainService.AddItem(characterId, oldEquipment.ItemId, 1, oldItemConfig.MaxStackCount);
                }

                // 6. 从背包扣掉新装备
                int leftCount = inventoryItem.Count - 1;
                if (leftCount > 0)
                {
                    _inventoryRepository.UpdateItemCount(inventoryItem.Id, leftCount);
                }
                else
                {
                    _inventoryRepository.DeleteById(inventoryItem.Id);
                }

                // 7. 写入/更新装备表
                if (oldEquipment == null)
                {
                    _equipmentRepository.Insert(new CharacterEquipmentEntity
                    {
                        CharacterId = characterId,
                        EquipSlotType = equipSlotType,
                        ItemId = inventoryItem.ItemId
                    });
                }
                else
                {
                    _equipmentRepository.UpdateByCharacterIdAndSlotType(characterId, equipSlotType, inventoryItem.ItemId);
                }

                // 8. 规范化背包
                List<InventoryItemInfo> normalizedItemList = _inventoryDomainService.NormalizeInventory(characterId);

                // 9. 构建返回
                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"装备成功：{itemConfig.ItemName}";
                response.CharacterId = characterId;
                response.ItemList = normalizedItemList;
                response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);

                CharacterEntity latestCharacter = _characterRepository.GetByCharacterId(characterId);
                response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(latestCharacter);

                return BuildEquipItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleEquipItem failed: CharacterId={request?.CharacterId}, SlotIndex={request?.SlotIndex}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                int characterId = request?.CharacterId ?? 0;
                CharacterEntity latestCharacter = characterId > 0 ? _characterRepository.GetByCharacterId(characterId) : null;

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "装备失败";
                response.CharacterId = characterId;
                response.ItemList = characterId > 0 ? _inventoryDomainService.BuildInventoryItemInfoList(characterId) : new List<InventoryItemInfo>();
                response.EquipmentList = characterId > 0 ? _equipmentDomainService.BuildEquipmentItemInfoList(characterId) : new List<EquipmentItemInfo>();
                response.CharacterInfo = latestCharacter != null ? _equipmentDomainService.BuildFinalCharacterInfo(latestCharacter) : null;

                return BuildEquipItemResponse(response);
            }
        }

        /// <summary>
        /// 卸下当前装备槽位上的道具
        /// </summary>
        public NetMessage HandleUnequipItem(NetMessage requestMessage, ClientSession session)
        {
            UnequipItemRequest request = JsonHelper.FromJson<UnequipItemRequest>(requestMessage.BodyJson);

            UnequipItemResponse response = new UnequipItemResponse();

            if (request == null || request.CharacterId <= 0 || request.EquipSlotType <= 0)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "卸下装备请求参数无效";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildUnequipItemResponse(response);
            }

            if (!IsSessionValid(session))
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "会话无效，无法卸下装备";
                response.CharacterId = 0;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildUnequipItemResponse(response);
            }

            if (request.CharacterId != session.CurrentCharacterId)
            {
                response.ErrorCode = (int)ErrorCode.InvalidParams;
                response.Message = "角色与当前会话不匹配";
                response.CharacterId = request.CharacterId;
                response.ItemList = new List<InventoryItemInfo>();
                response.EquipmentList = new List<EquipmentItemInfo>();
                response.CharacterInfo = null;
                return BuildUnequipItemResponse(response);
            }

            try
            {
                int characterId = request.CharacterId;

                CharacterEntity character = _characterRepository.GetByCharacterId(characterId);
                if (character == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "角色不存在";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = null;
                    return BuildUnequipItemResponse(response);
                }

                CharacterEquipmentEntity equipment = _equipmentRepository.GetByCharacterIdAndSlotType(characterId, request.EquipSlotType);
                if (equipment == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "该槽位没有装备";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildUnequipItemResponse(response);
                }

                ItemConfig itemConfig = GameServer.Instance.ItemConfigManager.GetById(equipment.ItemId);
                if (itemConfig == null)
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "装备配置不存在";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildUnequipItemResponse(response);
                }

                // 卸下前先判断背包能不能放回去
                if (!_inventoryDomainService.CanAddItem(characterId, equipment.ItemId, 1, itemConfig.MaxStackCount))
                {
                    response.ErrorCode = (int)ErrorCode.UnknownError;
                    response.Message = "背包空间不足，无法卸下装备";
                    response.CharacterId = characterId;
                    response.ItemList = _inventoryDomainService.BuildInventoryItemInfoList(characterId);
                    response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);
                    response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(character);
                    return BuildUnequipItemResponse(response);
                }

                // 放回背包
                _inventoryDomainService.AddItem(characterId, equipment.ItemId, 1, itemConfig.MaxStackCount);

                // 从装备表移除
                _equipmentRepository.DeleteByCharacterIdAndSlotType(characterId, request.EquipSlotType);

                // 规范化背包
                List<InventoryItemInfo> normalizedItemList = _inventoryDomainService.NormalizeInventory(characterId);

                response.ErrorCode = (int)ErrorCode.Success;
                response.Message = $"卸下成功：{itemConfig.ItemName}";
                response.CharacterId = characterId;
                response.ItemList = normalizedItemList;
                response.EquipmentList = _equipmentDomainService.BuildEquipmentItemInfoList(characterId);

                CharacterEntity latestCharacter = _characterRepository.GetByCharacterId(characterId);
                response.CharacterInfo = _equipmentDomainService.BuildFinalCharacterInfo(latestCharacter);

                return BuildUnequipItemResponse(response);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandleUnequipItem failed: CharacterId={request?.CharacterId}, EquipSlotType={request?.EquipSlotType}, Error={ex.Message}");
                Logger.Error(ex.ToString());

                int characterId = request?.CharacterId ?? 0;
                CharacterEntity latestCharacter = characterId > 0 ? _characterRepository.GetByCharacterId(characterId) : null;

                response.ErrorCode = (int)ErrorCode.UnknownError;
                response.Message = "卸下装备失败";
                response.CharacterId = characterId;
                response.ItemList = characterId > 0 ? _inventoryDomainService.BuildInventoryItemInfoList(characterId) : new List<InventoryItemInfo>();
                response.EquipmentList = characterId > 0 ? _equipmentDomainService.BuildEquipmentItemInfoList(characterId) : new List<EquipmentItemInfo>();
                response.CharacterInfo = latestCharacter != null ? _equipmentDomainService.BuildFinalCharacterInfo(latestCharacter) : null;

                return BuildUnequipItemResponse(response);
            }
        }

        /// <summary>
        /// 构建获取装备响应消息
        /// </summary>
        private NetMessage BuildGetEquipmentResponse(GetEquipmentResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.GetEquipmentResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 构建装备响应消息
        /// </summary>
        private NetMessage BuildEquipItemResponse(EquipItemResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.EquipItemResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 构建卸下装备响应消息
        /// </summary>
        private NetMessage BuildUnequipItemResponse(UnequipItemResponse response)
        {
            return new NetMessage
            {
                MessageId = (int)MessageId.UnequipItemResponse,
                BodyJson = JsonHelper.ToJson(response)
            };
        }

        /// <summary>
        /// 校验是否有效会话
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