using MMOServer.Core;
using System.Text.Json;

namespace MMOServer.Config
{
    public class ItemConfigManager
    {
        private readonly List<ItemConfig> _itemConfigList = new List<ItemConfig>();
        private readonly Dictionary<int, ItemConfig> _itemConfigDict = new Dictionary<int, ItemConfig>();

        /// <summary>
        /// 加载道具配置表
        /// </summary>
        public ItemConfigManager()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string filePath = Path.Combine(repoRootDir, "Config", "Generated", "ItemConfig.json");

            if (!File.Exists(filePath))
            {
                throw new Exception($"ItemConfig file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            List<ItemConfig>? configList = JsonSerializer.Deserialize<List<ItemConfig>>(json);

            _itemConfigList.Clear();
            _itemConfigDict.Clear();

            if (configList != null)
            {
                _itemConfigList.AddRange(configList);

                foreach (ItemConfig config in _itemConfigList)
                {
                    _itemConfigDict[config.ItemId] = config;
                }
            }

            Logger.Info($"服务端道具配置加载完成, 数量 = {_itemConfigDict.Count}");
        }

        /// <summary>
        /// 根据道具ID获取配置
        /// </summary>
        public ItemConfig GetById(int itemId)
        {
            _itemConfigDict.TryGetValue(itemId, out ItemConfig config);
            return config;
        }

        /// <summary>
        /// 获取全部道具配置
        /// </summary>
        public List<ItemConfig> GetAll()
        {
            return _itemConfigList;
        }

        /// <summary>
        /// 获取全部可装备道具配置
        /// </summary>
        public List<ItemConfig> GetAllEquipItems()
        {
            return _itemConfigList
                .Where(x => x.CanEquip == 1)
                .ToList();
        }

        /// <summary>
        /// 根据装备槽位类型获取可装备道具配置
        /// </summary>
        public List<ItemConfig> GetEquipItemsBySlotType(int equipSlotType)
        {
            return _itemConfigList
                .Where(x => x.CanEquip == 1 && x.EquipSlotType == equipSlotType)
                .ToList();
        }
    }
}