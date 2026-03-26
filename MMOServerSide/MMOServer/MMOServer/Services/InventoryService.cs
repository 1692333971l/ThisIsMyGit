using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services
{
    public class InventoryService
    {
        private readonly InventoryRepository _inventoryRepository;

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
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
    }
}