using MMOServer.Config;
using MMOServer.Network;

namespace MMOServer.Core
{
    public class GameServer
    {
        public static GameServer Instance { get; private set; } = null!;
        private NetServer NetServer;
        public ProfessionConfigManager ProfessionConfigManager { get; private set; }

        public GameServer()
        {
            if (Instance != null)
            {
                throw new Exception("GameServer already created.");
            }
            Instance = this;
            NetServer = new NetServer();
            ProfessionConfigManager = new ProfessionConfigManager();
        }

        public void Start()
        {
            Logger.Info("GameServer Start...");
            Logger.Info("Initialize modules...");
            Logger.Info("Server started successfully.");

            LoadConfigs();
            NetServer.Start(8888);

        }
        /// <summary>
        /// 加载配置表
        /// </summary>
        private void LoadConfigs()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string repoRootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", ".."));
            string configPath = Path.Combine(repoRootDir, "Config", "Generated", "ProfessionConfig.json");
            ProfessionConfigManager.Load(configPath);
        }
    }
}