using MMOServer.Config;
using MMOServer.Network;
using MMOServer.Services;
using MMOServer.World;

namespace MMOServer.Core
{
    public class GameServer
    {
        public static GameServer Instance { get; private set; }
        public NetServer NetServer { get; private set; }
        public ProfessionConfigManager ProfessionConfigManager { get; private set; }
        public ItemConfigManager ItemConfigManager { get; private set; }
        public NpcConfigManager NpcConfigManager { get; private set; }
        public ShopItemConfigManager ShopItemConfigManager { get; private set; }
        public UserService UserService { get; private set; }
        public CharacterService CharacterService { get; private set; }
        public WorldService WorldService { get; private set; }
        public ShopService ShopService { get; private set; }
        public InventoryService InventoryService { get; private set; }
        public OnlinePlayerManager OnlinePlayerManager { get; private set; }

        public GameServer()
        {
            if (Instance != null)
            {
                throw new Exception("GameServer already created.");
            }

            Instance = this;

            NetServer               = new NetServer();
            ProfessionConfigManager = new ProfessionConfigManager();
            ItemConfigManager       = new ItemConfigManager();
            NpcConfigManager        = new NpcConfigManager();
            ShopItemConfigManager   = new ShopItemConfigManager();
            UserService             = new UserService();
            CharacterService        = new CharacterService();
            WorldService            = new WorldService();
            ShopService             = new ShopService();
            InventoryService        = new InventoryService();
            OnlinePlayerManager     = new OnlinePlayerManager();
        }

        public void Start()
        {
            Logger.Info("GameServer Start...");
            Logger.Info("Initialize modules...");
            Logger.Info("Server started successfully.");

            NetServer.Start(8888);
        }
    }
}