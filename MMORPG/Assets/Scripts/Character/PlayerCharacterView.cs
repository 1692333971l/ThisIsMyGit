using UnityEngine;

//同一个角色 prefab 的表现模式切换器
[RequireComponent(typeof(PlayerMovementController))]
public class PlayerCharacterView : MonoBehaviour
{
    private Animator _animator;//动画控制器
    private CharacterController _characterController;//角色控制器组件
    private PlayerMovementController _movementController;//角色移动控制脚本

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _movementController = GetComponent<PlayerMovementController>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }
    //设置为角色预览模式
    public void SetupAsPreview()
    {
        if (_characterController != null)
        {
            _characterController.enabled = false;
        }

        if (_movementController != null)
        {
            _movementController.SetCanMove(false);
            _movementController.enabled = false;
        }

        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.Play("Idle", 0, 0f);
        }
    }
    //设置为本地玩家模式
    public void SetupAsLocalPlayer()
    {
        if (_characterController != null)
        {
            _characterController.enabled = true;
        }

        if (_movementController != null)
        {
            _movementController.enabled = true;
            _movementController.SetCanMove(true);
        }

        if (_animator != null)
        {
            _animator.enabled = true;
        }
    }
    //设置为联机玩家模式
    public void SetupAsRemotePlayer()
    {
        if (_characterController != null)
        {
            _characterController.enabled = false;
        }

        if (_movementController != null)
        {
            _movementController.SetCanMove(false);
            _movementController.enabled = false;
        }

        if (_animator != null)
        {
            _animator.enabled = true;
        }

        RemotePlayerSync remoteSync = GetComponent<RemotePlayerSync>();
        if (remoteSync == null)
        {
            gameObject.AddComponent<RemotePlayerSync>();
        }
    }
}