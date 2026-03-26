using UnityEngine;

// 远端角色的网络表现器
// 作用：根据网络同步过来的状态，更新远端角色的表现
//
// 它不负责读取输入，也不负责真正的本地移动逻辑。
// 它只负责：
// 1. 接收服务端广播过来的目标位置
// 2. 接收服务端广播过来的目标朝向
// 3. 接收服务端广播过来的是否在移动
// 4. 通过插值，让远端角色平滑移动到目标状态
public class RemotePlayerSync : MonoBehaviour
{
    /// <summary>
    /// 位置插值速度
    /// 数值越大，远端角色追目标位置越快
    /// 数值越小，远端角色移动越平滑但更“拖”
    /// </summary>
    [SerializeField] private float _positionLerpSpeed = 12f;

    /// <summary>
    /// 旋转插值速度
    /// 数值越大，远端角色转身越快
    /// </summary>
    [SerializeField] private float _rotationLerpSpeed = 12f;

    /// <summary>
    /// 当前角色身上的 Animator
    /// 用于根据网络状态切换 Walk / Run 动画参数
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// 远端角色“应该移动到”的目标位置
    /// 这个值由网络同步包不断更新
    /// </summary>
    private Vector3 _targetPosition;

    /// <summary>
    /// 远端角色“应该转到”的目标朝向（Y轴旋转角）
    /// 这个值由网络同步包不断更新
    /// </summary>
    private float _targetRotY;

    /// <summary>
    /// 当前远端角色是否在移动
    /// 这个值来自网络同步
    /// </summary>
    private bool _isMoving;

    /// <summary>
    /// Unity 生命周期：Awake
    /// 
    /// 作用：
    /// 1. 获取 Animator 组件
    /// 2. 用当前物体的初始位置和初始朝向作为目标状态
    ///    防止刚生成时目标值为空或不一致
    /// </summary>
    private void Awake()
    {
        // 获取当前物体上的 Animator 组件
        _animator = GetComponent<Animator>();

        // 初始化目标位置为当前实际位置
        // 这样刚生成时不会突然被拉向原点
        _targetPosition = transform.position;

        // 初始化目标朝向为当前实际朝向
        _targetRotY = transform.eulerAngles.y;
    }

    /// <summary>
    /// 应用一份新的网络状态
    /// 
    /// 作用：
    /// 当客户端收到服务端广播的远端玩家状态时，
    /// 就调用这个方法，把最新状态更新到本脚本中。
    /// 
    /// 注意：
    /// 这里不是立刻瞬移，而是只更新“目标状态”。
    /// 真正的平滑移动在 Update() 中完成。
    /// </summary>
    /// <param name="position">网络同步过来的目标位置</param>
    /// <param name="rotY">网络同步过来的目标Y轴朝向</param>
    /// <param name="isMoving">网络同步过来的是否在移动</param>
    /// <param name="isRunning">网络同步过来的是否在跑动</param>
    public void ApplyNetState(Vector3 position, float rotY, bool isMoving, bool isRunning)
    {
        // 更新目标位置
        _targetPosition = position;

        // 更新目标朝向
        _targetRotY = rotY;

        // 更新移动状态
        _isMoving = isMoving;

        // 如果当前角色身上有 Animator，就同步动画参数
        if (_animator != null)
        {
            // 如果远端玩家处于移动状态，则根据状态播放 Walk Run 动画
            _animator.SetBool("Walk", isMoving);
            _animator.SetBool("Run", isRunning);
        }
    }

    /// <summary>
    /// Unity 生命周期：每帧更新
    /// 
    /// 作用：
    /// 让远端角色的当前位置和当前朝向，
    /// 平滑地靠近网络同步来的目标状态。
    /// 
    /// 为什么不用直接赋值：
    /// 因为网络包不是每帧都发，
    /// 直接赋值会让远端角色看起来抖动、瞬移、不自然。
    /// 所以要通过插值平滑过渡。
    /// </summary>
    private void Update()
    {
        // 位置插值：
        // 从“当前实际位置”平滑移动到“目标位置”
        // Time.deltaTime * _positionLerpSpeed 用于控制平滑速度
        transform.position = Vector3.Lerp(
            transform.position,
            _targetPosition,
            Time.deltaTime * _positionLerpSpeed
        );

        // 根据目标Y轴角度构造目标旋转
        Quaternion targetRotation = Quaternion.Euler(0f, _targetRotY, 0f);

        // 旋转插值：
        // 从“当前实际旋转”平滑转向“目标旋转”
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * _rotationLerpSpeed
        );
    }
}