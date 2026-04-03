using MMOServer.Config;
using MMOServer.Core;
using MMOServer.Database;
using MMOServer.Models;
using Protocol;

namespace MMOServer.Services.Common
{
    /// <summary>
    /// 背包领域公共服务
    /// 负责：
    /// 1. 判断是否可放入背包
    /// 2. 添加道具到背包
    /// 3. 规范化背包（合并堆叠、去空洞、重排格子）
    /// 4. 构建协议层背包列表
    /// </summary>
    public class InventoryDomainService
    {
        private const int MaxSlotCount = 72;
        private readonly InventoryRepository _inventoryRepository;

        public InventoryDomainService()
        {
            _inventoryRepository = new InventoryRepository();
        }

        /// <summary>
        /// 判断道具是否可以放入背包
        /// 规则：
        /// 1. 优先堆叠到已有同类格子
        /// 2. 剩余数量需要新增格子
        /// 3. 新增后总格子数不能超过最大格子数
        /// </summary>
        public bool CanAddItem(int characterId, int itemId, int quantity, int maxStackCount)
        {
            if (quantity <= 0)
            {
                return true;
            }

            if (maxStackCount <= 0)
            {
                return false;
            }

            List<InventoryItemEntity> inventoryList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            int usedSlotCount = inventoryList.Count;
            int remainQuantity = quantity;

            // 先尝试堆叠进已有同类格子
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

                int canStack = maxStackCount - inventoryItem.Count;
                remainQuantity -= canStack;

                if (remainQuantity <= 0)
                {
                    return true;
                }
            }

            // 剩余数量需要新开多少格
            int needNewSlotCount = (int)Math.Ceiling((double)remainQuantity / maxStackCount);

            return usedSlotCount + needNewSlotCount <= MaxSlotCount;
        }

        /// <summary>
        /// 添加道具到背包
        /// 1. 优先堆叠到已有同类格子
        /// 2. 剩余数量再找空格创建新格子
        /// </summary>
        public void AddItem(int characterId, int itemId, int quantity, int maxStackCount)
        {
            if (quantity <= 0)
            {
                return;
            }

            if (!CanAddItem(characterId, itemId, quantity, maxStackCount))
            {
                throw new Exception("背包空间不足，无法添加道具");
            }

            List<InventoryItemEntity> inventoryList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            // 1. 优先堆叠已有同类格子
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

            // 2. 再找空格创建新格子
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
        /// 规范化当前角色背包
        /// 1. 合并同类物品堆叠
        /// 2. 按最大堆叠数重新拆分
        /// 3. 去掉空洞
        /// 4. 从 0 开始连续分配 SlotIndex
        /// </summary>
        public List<InventoryItemInfo> NormalizeInventory(int characterId)
        {
            List<InventoryItemEntity> itemList = _inventoryRepository.GetInventoryListByCharacterId(characterId);

            if (itemList == null || itemList.Count == 0)
            {
                return new List<InventoryItemInfo>();
            }

            // 统计每种物品总数
            Dictionary<int, int> totalCountDict = new Dictionary<int, int>();

            foreach (InventoryItemEntity item in itemList)
            {
                if (!totalCountDict.ContainsKey(item.ItemId))
                {
                    totalCountDict[item.ItemId] = 0;
                }

                totalCountDict[item.ItemId] += item.Count;
            }

            // 保留原始物品大类顺序（按最早出现的 SlotIndex）
            List<int> orderedItemIdList = itemList
                .OrderBy(x => x.SlotIndex)
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

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

            // 理论上正常情况下不会超过上限，但这里再加一道保护
            if (normalizedList.Count > MaxSlotCount)
            {
                throw new Exception($"背包规范化后超出最大格子数限制，CharacterId={characterId}");
            }

            // 重建数据库中的背包记录
            _inventoryRepository.DeleteAllByCharacterId(characterId);

            foreach (InventoryItemEntity item in normalizedList)
            {
                _inventoryRepository.Insert(item);
            }

            return normalizedList.Select(ToInventoryItemInfo).ToList();
        }

        /// <summary>
        /// 获取角色当前背包列表（协议对象）
        /// </summary>
        public List<InventoryItemInfo> BuildInventoryItemInfoList(int characterId)
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
        /// 数据库实体 -> 协议对象
        /// </summary>
        public InventoryItemInfo ToInventoryItemInfo(InventoryItemEntity entity)
        {
            return new InventoryItemInfo
            {
                SlotIndex = entity.SlotIndex,
                ItemId = entity.ItemId,
                Count = entity.Count
            };
        }
    }
}