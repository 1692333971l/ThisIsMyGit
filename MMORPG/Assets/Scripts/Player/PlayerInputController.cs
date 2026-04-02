using UnityEngine;

//角色输入控制器
public class PlayerInputController : MonoBehaviour
{
    private PlayerMovementController _movementController;//玩家移动控制器组件
    private CameraFollow _cameraFollow;//摄像机跟随组件
    private bool _isCharacterPanelPlaying = false;//面板是否正在显示

    private UIManager _uiManager;//UI面板管理器

    public void Init(UIManager uiManager, CameraFollow cameraFollow, PlayerMovementController movementController)
    {
        _uiManager = uiManager;
        _movementController = movementController;
        _cameraFollow = cameraFollow;
    }

    private void Update()
    {
        //呼出角色面板
        if (Input.GetKeyDown(KeyCode.Tab) && !_isCharacterPanelPlaying)
        {
            _uiManager.SetCharacterPanelActive(true);
            _uiManager.SetPlayerInfoPanelActive(false);
            SetPlayerController(true, false, false);
        }
        //交互
        if (Input.GetKeyDown(KeyCode.F) && !_isCharacterPanelPlaying)
        {
            _uiManager.SetPlayerInfoPanelActive(false);
            GameApp.Instance.InteractionManager.Interact(transform);
            SetPlayerController(true, false, false);
        }
        //关闭面板
        if (Input.GetKeyDown(KeyCode.Escape) && _isCharacterPanelPlaying)
        {
            _uiManager.CloseAllPanel();
            SetPlayerController(false, true, true);
        }
    }

    private void SetPlayerController(bool isCharacterPanelPlaying, bool viewpointMovement, bool canMove)
    {
        _isCharacterPanelPlaying = isCharacterPanelPlaying;
        _cameraFollow.SetViewpointMovement(viewpointMovement);
        _movementController.SetCanMove(canMove);
    }
}
