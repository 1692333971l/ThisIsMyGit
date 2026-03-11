using UnityEngine;

// 玩家控制器脚本，挂载在玩家角色上
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private float _moveSpeed = 2f;//移动速度

    private Transform _cameraTransform;//摄像机变换
    private CharacterController _characterController;//角色控制器组件
    private Animator _animator;//动画组件

    public void SetCameraTranform(Transform cameraTranform)
    {
        _cameraTransform = cameraTranform;
    }
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleMove();
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal");//获取水平输入
        float vertical = Input.GetAxis("Vertical");//获取垂直输入
        Vector3 forward = _cameraTransform.forward;//获取摄像机的前方向
        Vector3 right = _cameraTransform.right;//获取摄像机的右方向

        forward.y = 0;//将前方向的y分量设为0，使角色只能在水平面上移动
        right.y = 0;//将右方向的y分量设为0，使角色只能在水平面上移动
        Vector3 move = forward * vertical + right * horizontal;//根据输入计算移动方向

        if (horizontal != 0 || vertical != 0)
        {
            move.Normalize();//将移动方向向量归一化，使其长度为1
            _animator.SetBool("Walk", true);//设置动画参数，播放行走动画
            if (Input.GetKeyDown(KeyCode.LeftShift))//如果按下左Shift键，切换到跑步状态
            {
                _moveSpeed = 4f;
                _animator.SetBool("Run", true);//设置动画参数，播放跑步动画
            }
        }
        else
        {
            _moveSpeed = 1.5f;
            _animator.SetBool("Walk", false);
            _animator.SetBool("Run", false);
        }

        _characterController.SimpleMove(move * _moveSpeed);//使用角色控制器的SimpleMove方法移动角色，传入移动方向和速度

        if (move != Vector3.zero)
        {
            transform.forward = move;//将角色的前方向设置为移动方向，使角色面向移动的方向
        }
    }
}