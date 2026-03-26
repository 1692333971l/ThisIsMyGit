using UnityEngine;
using UnityEngine.UI;

//角色输入控制器
public class PlayerInputController : MonoBehaviour
{
    private CameraFollow _cameraFollow;//摄像机跟随组件
    private Image _characterPanel;//角色呼出面板
    private bool _isCharacterPanelPlaying = false;//面板是否正在显示

    public void Init(Image characterPanel, CameraFollow cameraFollow)
    {
        _characterPanel = characterPanel;
        _cameraFollow = cameraFollow;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_isCharacterPanelPlaying)
            {
                _characterPanel.gameObject.SetActive(false);
                _isCharacterPanelPlaying = false;
                _cameraFollow.SetViewpointMovement(true);
            }
            else
            {
                _characterPanel.gameObject.SetActive(true);
                _isCharacterPanelPlaying = true;
                _cameraFollow.SetViewpointMovement(false);
            }
        }
    }
}
