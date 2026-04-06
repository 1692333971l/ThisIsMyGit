using Protocol;
using UnityEngine;

//客户端总入口: 客户端应用启动后，所有核心模块都由它统一创建和管理
public class GameApp : MonoBehaviour
{
    //全局核心服务
    public static GameApp Instance { get; private set; }//客户端总入口，单例
    public NetClient NetClient { get; private set; }//客户端网络层
    public ClientMessageDispatcher ClientMessageDispatcher { get; private set; }//客户端消息分发层
    public UserSession UserSession { get; private set; }//持久化数据
    public SceneLoaderManager SceneLoaderManager { get; private set; }//场景切换管理器
    //业务层
    public UserService UserService { get; private set; }//登录注册业务层
    public CharacterService CharacterService { get; private set; }//角色选择创建业务层
    public WorldService WorldService { get; private set; }//世界消息业务层
    public InventoryService InventoryService { get; private set; }//背包业务层
    public ShopService ShopService { get; private set; }//商店业务层
    public EquipmentService EquipmentService { get; private set; }//装备业务层
    //当前玩家数据交互服务
    public PlayerCharacterManager PlayerCharacterManager { get; private set; }//玩家当前角色信息管理器
    public PlayerInventoryManager PlayerInventoryManager { get; private set; }//玩家当前角色背包管理器
    public InteractionManager InteractionManager { get; private set; }//玩家交互管理器
    public PlayerEquipmentManager PlayerEquipmentManager { get; private set; }//玩家当前角色装备管理器
    //联机服务
    public PlayerSpawnManager PlayerSpawnManager { get; private set; }//玩家生成管理器
    public RemotePlayerManager RemotePlayerManager { get; private set; }//场景里其他玩家对象的总表
    //配置表
    public ProfessionConfigManager ProfessionConfigManager { get; private set; }//职业配置表管理器
    public ItemConfigManager ItemConfigManager { get; private set; }//道具配置表管理器
    public NpcConfigManager NpcConfigManager { get; private set; }//NPC配置表管理器
    public ShopItemConfigManager ShopItemConfigManager { get; private set; }//商店配置表管理器

    private bool _hasSentExit = false;
    public string ServerIp { get; private set; } = "127.0.0.1";
    public int ServerPort { get; private set; } = 8888;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegistrationServices();//注册服务
        NetClientConnect();//链接服务端

        Debug.Log("GameApp Init");
    }

    private void Start()
    {
        SceneLoaderManager.LoadLoginAndRegistrationScene();
        Debug.Log("Game Started");
    }
    private void RegistrationServices()
    {
        //全局核心服务
        NetClient               = new NetClient();
        ClientMessageDispatcher = new ClientMessageDispatcher();
        UserSession             = new UserSession();
        SceneLoaderManager      = new SceneLoaderManager();
        //业务层
        UserService             = new UserService();
        CharacterService        = new CharacterService();
        WorldService            = new WorldService();
        InventoryService        = new InventoryService();
        ShopService             = new ShopService();
        EquipmentService        = new EquipmentService();
        //当前玩家数据交互服务
        PlayerCharacterManager  = new PlayerCharacterManager();
        PlayerInventoryManager  = new PlayerInventoryManager();
        InteractionManager      = new InteractionManager();
        PlayerEquipmentManager  = new PlayerEquipmentManager();
        //联机服务
        PlayerSpawnManager      = new PlayerSpawnManager();
        RemotePlayerManager     = new RemotePlayerManager();
        //配置表
        ProfessionConfigManager = new ProfessionConfigManager();
        ItemConfigManager       = new ItemConfigManager();
        NpcConfigManager        = new NpcConfigManager();
        ShopItemConfigManager   = new ShopItemConfigManager();
    }
    //链接服务端
    private void NetClientConnect()
    {
        GameApp.Instance.NetClient.Connect(
           GameApp.Instance.ServerIp,
           GameApp.Instance.ServerPort
        );
    }
    private void OnApplicationQuit()
    {
        SendExitRequest();
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SendExitRequest();
        }
    }
    //向服务器发送退出请求
    private void SendExitRequest()
    {
        if (_hasSentExit)
        {
            return;
        }

        if (PlayerCharacterManager == null || !PlayerCharacterManager.HasCharacterInfo())
        {
            return;
        }

        Protocol.CharacterInfo characterInfo = PlayerCharacterManager.GetCharacterInfo();
        if (characterInfo == null || characterInfo.CharacterId <= 0)
        {
            return;
        }

        _hasSentExit = true;

        PlayerExitRequest request = new PlayerExitRequest
        {
            CharacterId = characterInfo.CharacterId
        };

        NetMessage message = new NetMessage
        {
            MessageId = (int)MessageId.PlayerExitRequest,
            BodyJson = JsonUtility.ToJson(request)
        };

        Debug.Log($"SendExitRequest: CharacterId={characterInfo.CharacterId}");
        NetClient.SendMessage(message);
    }
}