using Protocol;
using UnityEngine;

// 本地角色状态上报器
// 作用：定时把“本地玩家当前状态”同步给服务端
//
// 它不负责真正控制移动，移动是 PlayerMovementController 的职责。
// 它只负责：
// 1. 读取本地角色当前位置
// 2. 读取本地角色当前朝向
// 3. 读取本地角色当前是否在移动
// 4. 如果状态发生变化，则发 PlayerMoveRequest 给服务端
public class LocalPlayerNetworkSync : MonoBehaviour
{
    /// <summary>
    /// 发送间隔（秒）
    /// 表示每隔多久检查一次是否需要同步
    /// 例如 0.1f 表示每 0.1 秒检查一次
    /// </summary>
    [SerializeField] private float _sendInterval = 0.1f;

    /// <summary>
    /// 当前角色身上的 Animator 组件
    /// 用于读取 Walk / Run 动画参数，从而判断角色是否在移动
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// 计时器
    /// 用来累计时间，达到 _sendInterval 后才执行一次同步检查
    /// </summary>
    private float _timer;

    /// <summary>
    /// 上一次发送给服务端的位置
    /// 用来做变化比较，避免没变化时也一直发包
    /// </summary>
    private Vector3 _lastPos;

    /// <summary>
    /// 上一次发送给服务端的Y轴朝向
    /// </summary>
    private float _lastRotY;

    /// <summary>
    /// 上一次发送给服务端的“是否在移动”状态
    /// </summary>
    private bool _lastIsMoving;

    /// <summary>
    /// 上一次发送给服务端的“是否在跑动”状态
    /// </summary>
    private bool _lastIsRunning;

    /// <summary>
    /// 当前本地角色的角色信息
    /// 主要是为了拿 CharacterId，发同步包时需要告诉服务端“是谁在移动”
    /// </summary>
    private Protocol.CharacterInfo _characterInfo;

    /// <summary>
    /// Unity 生命周期：Awake
    /// 
    /// 作用：
    /// 在对象初始化时获取 Animator 组件
    /// </summary>
    private void Awake()
    {
        // 获取当前物体上的 Animator
        // 后面用它来读取 Walk / Run 参数
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 初始化本地网络同步器
    /// 
    /// 作用：
    /// 1. 记录当前角色信息（主要是 CharacterId）
    /// 2. 记录当前初始位置、朝向、移动状态
    ///    这样后续 Update 时可以拿来比较变化
    /// </summary>
    /// <param name="characterInfo">当前本地玩家的角色信息</param>
    public void Init(Protocol.CharacterInfo characterInfo)
    {
        // 保存角色信息
        _characterInfo = characterInfo;

        // 把当前位置记为“上一次同步的位置”
        _lastPos = transform.position;

        // 把当前Y轴朝向记为“上一次同步的朝向”
        _lastRotY = transform.eulerAngles.y;

        // 初始化时默认认为未在移动
        _lastIsMoving = false;
    }

    /// <summary>
    /// Unity 生命周期：每帧更新
    /// 
    /// 流程：
    /// 1. 如果当前还没有角色信息，则不做任何同步
    /// 2. 用计时器控制同步频率，不要每帧都发包
    /// 3. 读取当前移动状态、位置、朝向
    /// 4. 和上一次发送的状态做对比
    /// 5. 如果状态有变化，则构造 PlayerMoveRequest 并发送给服务端
    /// 6. 更新“上一次发送的状态”
    /// </summary>
    private void Update()
    {
        // 如果角色信息还没有初始化，说明当前对象还不能进行网络同步
        if (_characterInfo == null)
        {
            return;
        }

        // 每帧累加时间
        _timer += Time.deltaTime;

        // 如果还没达到发送间隔，则先不发送
        if (_timer < _sendInterval)
        {
            return;
        }

        // 达到发送间隔后，重置计时器
        _timer = 0f;

        // -------------------------
        // 第一步：计算当前是否在移动
        // -------------------------

        // 默认先认为当前不在移动
        bool isWalking = false;
        bool isRunning = false;

        // 如果 Animator 存在，则通过动画参数判断是否在移动
        if (_animator != null)
        {
            isWalking = _animator.GetBool("Walk");
            isRunning = _animator.GetBool("Run");
        }

        bool isMoving = isWalking || isRunning;
        // -------------------------
        // 第二步：读取当前位置和朝向
        // -------------------------

        // 当前角色位置
        Vector3 currentPos = transform.position;

        // 当前角色绕Y轴旋转角度
        float currentRotY = transform.eulerAngles.y;

        // -------------------------
        // 第三步：比较状态是否发生变化
        // -------------------------

        // changed 为 true 的条件：
        // 1. 当前位置和上次位置相差超过 0.02
        // 2. 当前朝向和上次朝向差值超过 1 度
        // 3. 当前移动状态和上次不同
        // 3. 当前跑动状态和上次不同
        bool changed =
            Vector3.Distance(currentPos, _lastPos) > 0.02f ||
            Mathf.Abs(currentRotY - _lastRotY) > 1f ||
            isMoving != _lastIsMoving ||
            isRunning != _lastIsRunning;

        // 如果状态没有变化，则不需要发包
        if (!changed)
        {
            return;
        }

        // -------------------------
        // 第四步：构造移动请求
        // -------------------------

        PlayerMoveRequest request = new PlayerMoveRequest
        {
            // 角色ID：告诉服务端是谁在移动
            CharacterId = _characterInfo.CharacterId,

            // 当前坐标
            PosX = currentPos.x,
            PosY = currentPos.y,
            PosZ = currentPos.z,

            // 当前朝向
            RotY = currentRotY,

            // 当前是否在移动
            IsMoving = isMoving,
            IsRunning = isRunning
        };

        // -------------------------
        // 第五步：把请求包装成统一网络消息
        // -------------------------

        NetMessage message = new NetMessage
        {
            // 消息号：玩家移动请求
            MessageId = (int)MessageId.PlayerMoveRequest,

            // 消息体：把请求对象序列化成 JSON
            BodyJson = JsonUtility.ToJson(request)
        };

        // -------------------------
        // 第六步：发送给服务端
        // -------------------------

        GameApp.Instance.NetClient.SendMessage(message);

        // -------------------------
        // 第七步：更新“上一次发送状态”
        // -------------------------

        // 把当前位置记下来，作为下次比较的旧数据
        _lastPos = currentPos;

        // 把当前朝向记下来
        _lastRotY = currentRotY;

        // 把当前移动状态记下来
        _lastIsMoving = isMoving;

        // 把当前跑动状态记下来
        _lastIsRunning = isRunning;
    }
}