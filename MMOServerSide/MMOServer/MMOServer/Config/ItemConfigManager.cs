using System.Text.Json;
using MMOServer.Core;

namespace MMOServer.Config
{
    public class ItemConfigManager
    {
        private readonly Dictionary<int, ItemConfig> _configDict = new Dictionary<int, ItemConfig>();

        /// <summary>
        /// 加载道具配置
        /// </summary>
        public ItemConfigManager()
        {
            _configDict.Clear();

            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string configPath = Path.Combine(repoRootDir, "Config", "Generated", "ItemConfig.json");

            if (!File.Exists(configPath))
            {
                Logger.Error($"道具配置文件不存在：{configPath}");
                return;
            }

            string json = File.ReadAllText(configPath);

            List<ItemConfig> configs = JsonSerializer.Deserialize<List<ItemConfig>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("道具配置表反序列化失败");

            foreach (ItemConfig config in configs)
            {
                _configDict[config.ItemId] = config;
            }

            Logger.Info($"服务端道具配置加载完成，数量：{_configDict.Count}");
        }

        /// <summary>
        /// 根据道具ID获取配置
        /// </summary>
        public ItemConfig GetById(int itemId)
        {
            _configDict.TryGetValue(itemId, out ItemConfig config);
            return config;
        }

        /// <summary>
        /// 获取全部道具配置
        /// </summary>
        public List<ItemConfig> GetAll()
        {
            return _configDict.Values.ToList();
        }
    }
}