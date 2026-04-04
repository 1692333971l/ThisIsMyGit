using MMOServer.Core;
using System.Text.Json;

namespace MMOServer.Config
{
    public class MapPortalConfigManager
    {
        private List<MapPortalConfig> _mapPortalConfigList = new List<MapPortalConfig>();
        private Dictionary<int, MapPortalConfig> _mapPortalDict = new Dictionary<int, MapPortalConfig>();

        /// <summary>
        /// 加载地图传送点配置表
        /// </summary>
        public MapPortalConfigManager()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string filePath = Path.Combine(repoRootDir, "Config", "Generated", "MapPortalConfig.json");

            if (!File.Exists(filePath))
            {
                throw new Exception($"MapPortalConfig file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            List<MapPortalConfig>? configList = JsonSerializer.Deserialize<List<MapPortalConfig>>(json);

            _mapPortalConfigList = configList ?? new List<MapPortalConfig>();
            _mapPortalDict.Clear();

            foreach (MapPortalConfig config in _mapPortalConfigList)
            {
                _mapPortalDict[config.PortalId] = config;
            }

            Logger.Info($"服务端地图传送配置加载完成, 数量 = {_mapPortalConfigList.Count}");
        }

        /// <summary>
        /// 根据 PortalId 获取传送点配置
        /// </summary>
        public MapPortalConfig GetById(int portalId)
        {
            _mapPortalDict.TryGetValue(portalId, out MapPortalConfig config);
            return config;
        }

        /// <summary>
        /// 获取全部地图传送点配置
        /// </summary>
        public List<MapPortalConfig> GetAll()
        {
            return _mapPortalConfigList;
        }
    }
}