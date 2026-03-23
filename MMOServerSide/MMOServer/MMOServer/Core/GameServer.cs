using MMOServer.Config;
using MMOServer.Network;
using MMOServer.Services;

namespace MMOServer.Core
{
    public class GameServer
    {
        public static GameServer Instance { get; private set; }
        public NetServer NetServer { get; private set; }
        public ProfessionConfigManager ProfessionConfigManager { get; private set; }
        public UserService UserService { get; private set; }
        public CharacterService CharacterService { get; private set; }

        public GameServer()
        {
            if (Instance != null)
            {
                throw new Exception("GameServer already created.");
            }

            Instance = this;

            NetServer               = new NetServer();
            ProfessionConfigManager = new ProfessionConfigManager();
            UserService             = new UserService();
            CharacterService        = new CharacterService();
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