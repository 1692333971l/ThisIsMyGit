using UnityEngine;

// 游戏主程序，负责初始化游戏核心组件和管理游戏流程
public class GameApp : MonoBehaviour
{
    public static GameApp Instance { get; private set; }//主程序单例
    public SceneLoader SceneLoader { get; private set; }//场景加载器
    public PlayerSession PlayerSession { get; private set; }//玩家数据管理器
    public InteractionManager InteractionManager { get; private set; }//交互管理器
    public PlayerTaskManager PlayerTaskManager { get; private set; }//玩家任务管理器

    public IAuthService AuthService { get; private set; }//认证服务接口
    public IRoleService RoleService { get; private set; }//库存服务接口
    public INPCService NPCService { get; private set; }//NPC服务接口
    public ITaskService TaskService { get; private set; }//NPC任务接口

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitManagers();//初始化管理器
        InitServices();//初始化服务
    }

    private void Start()
    {
        SceneLoader.LoadLoginScene();//启动时加载登录场景
    }

    private void InitManagers()
    {
        SceneLoader = new SceneLoader();
        PlayerSession = new PlayerSession();
        InteractionManager = new InteractionManager();
        PlayerTaskManager = new PlayerTaskManager();
    }

    private void InitServices()
    {
        AuthService = new MockAuthService();
        RoleService = new MockRoleService();
        NPCService  = new MockNPCService();
        TaskService = new MockTaskService();
    }
}
