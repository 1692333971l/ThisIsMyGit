using MMOServer.Core;
using System.Text.Json;

namespace MMOServer.Config
{
    public class NpcConfigManager
    {
        private Dictionary<int, NpcConfig> _npcConfigDict = new Dictionary<int, NpcConfig>();

        /// <summary>
        /// 加载 NPC 配置表
        /// </summary>
        public NpcConfigManager()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string filePath = Path.Combine(repoRootDir, "Config", "Generated", "NpcConfig.json");

            if (!File.Exists(filePath))
            {
                throw new Exception($"NpcConfig file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            List<NpcConfig>? configList = JsonSerializer.Deserialize<List<NpcConfig>>(json);

            _npcConfigDict.Clear();

            if (configList != null)
            {
                foreach (NpcConfig config in configList)
                {
                    _npcConfigDict[config.NpcId] = config;
                }
            }

            Logger.Info($"服务端NPC配置加载完成, 数量 = {_npcConfigDict.Count}");
        }

        /// <summary>
        /// 根据 NpcId 获取配置
        /// </summary>
        public NpcConfig GetById(int npcId)
        {
            _npcConfigDict.TryGetValue(npcId, out NpcConfig config);
            return config;
        }

        /// <summary>
        /// 获取全部 NPC 配置
        /// </summary>
        public List<NpcConfig> GetAll()
        {
            return _npcConfigDict.Values.ToList();
        }
    }
}