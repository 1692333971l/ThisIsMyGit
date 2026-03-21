using System.Text.Json;
using MMOServer.Core;

namespace MMOServer.Config
{
    public class ProfessionConfigManager
    {
        private readonly Dictionary<int, ProfessionConfig> _configDict = new Dictionary<int, ProfessionConfig>();

        /// <summary>
        /// 加载职业配置
        /// </summary>
        public void Load(string configPath)
        {
            _configDict.Clear();

            if (!File.Exists(configPath))
            {
                Logger.Error($"职业配置文件不存在：{configPath}");
                return;
            }

            string json = File.ReadAllText(configPath);

            List<ProfessionConfig> configs = JsonSerializer.Deserialize<List<ProfessionConfig>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("职业配置表反序列化失败");

            foreach (ProfessionConfig config in configs)
            {
                _configDict[config.ProfessionId] = config;
            }

            Logger.Info($"服务端职业配置加载完成，数量：{_configDict.Count}");
        }

        /// <summary>
        /// 根据职业ID获取配置
        /// </summary>
        public ProfessionConfig GetById(int professionId)
        {
            _configDict.TryGetValue(professionId, out ProfessionConfig config);
            return config;
        }
    }
}