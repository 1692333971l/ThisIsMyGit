using MMOServer.Core;
using System.Text.Json;

namespace MMOServer.Config
{
    public class ShopItemConfigManager
    {
        private List<ShopItemConfig> _shopItemConfigList = new List<ShopItemConfig>();
        private Dictionary<int, List<ShopItemConfig>> _shopItemDict = new Dictionary<int, List<ShopItemConfig>>();

        /// <summary>
        /// 加载商店商品配置表
        /// </summary>
        public ShopItemConfigManager()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string filePath = Path.Combine(repoRootDir, "Config", "Generated", "ShopItemConfig.json");

            if (!File.Exists(filePath))
            {
                throw new Exception($"ShopItemConfig file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            List<ShopItemConfig>? configList = JsonSerializer.Deserialize<List<ShopItemConfig>>(json);

            _shopItemConfigList = configList ?? new List<ShopItemConfig>();
            _shopItemDict.Clear();

            foreach (ShopItemConfig config in _shopItemConfigList)
            {
                if (!_shopItemDict.ContainsKey(config.ShopId))
                {
                    _shopItemDict[config.ShopId] = new List<ShopItemConfig>();
                }

                _shopItemDict[config.ShopId].Add(config);
            }

            foreach (var pair in _shopItemDict)
            {
                pair.Value.Sort((a, b) => a.Sort.CompareTo(b.Sort));
            }

            Logger.Info($"服务端商店配置加载完成, 数量 = {_shopItemConfigList.Count}");
        }

        /// <summary>
        /// 根据 ShopId 获取商店商品列表
        /// </summary>
        public List<ShopItemConfig> GetByShopId(int shopId)
        {
            if (_shopItemDict.TryGetValue(shopId, out List<ShopItemConfig> configList))
            {
                return configList;
            }

            return new List<ShopItemConfig>();
        }

        /// <summary>
        /// 获取全部商店商品配置
        /// </summary>
        public List<ShopItemConfig> GetAll()
        {
            return _shopItemConfigList;
        }
    }
}