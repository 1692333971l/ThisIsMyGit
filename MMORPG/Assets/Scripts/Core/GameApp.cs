using UnityEngine;

//客户端总入口: 客户端应用启动后，所有核心模块都由它统一创建和管理
public class GameApp : MonoBehaviour
{
    public static GameApp Instance { get; private set; }
    public NetClient NetClient { get; private set; }//客户端网络层
    public ClientMessageDispatcher ClientMessageDispatcher { get; private set; }//客户端消息分发层
    public UserSession UserSession { get; private set; }//持久化数据
    public SceneLoaderManager SceneLoaderManager { get; private set; }//场景切换管理器

    public UserService UserService { get; private set; }//登录注册业务层

    //当前服务器连接配置
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

        RegistrationServices();//注册核心服务
        RegistrationManagers();//注册核心管理器
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
        NetClient = new NetClient();
        ClientMessageDispatcher = new ClientMessageDispatcher();
        UserService = new UserService();
        UserSession = new UserSession();
    }
    private void RegistrationManagers()
    {
        SceneLoaderManager = new SceneLoaderManager();
    }
    //链接服务端
    private void NetClientConnect()
    {
        GameApp.Instance.NetClient.Connect(
           GameApp.Instance.ServerIp,
           GameApp.Instance.ServerPort
        );
    }
}