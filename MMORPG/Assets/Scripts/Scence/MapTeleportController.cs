using Protocol;
using UnityEngine;

//地图传送控制器
public class MapTeleportController : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;

    private void OnEnable()
    {
        GameApp.Instance.WorldService.OnTeleportResponse += HandleTeleportResponse;
    }

    private void OnDisable()
    {
        if (GameApp.Instance == null)
        {
            return;
        }

        GameApp.Instance.WorldService.OnTeleportResponse -= HandleTeleportResponse;
    }

    //处理地图传送响应
    private void HandleTeleportResponse(TeleportResponse response)
    {
        if ((ErrorCode)response.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("传送失败：" + response.Message);
            return;
        }

        //关闭当前所有UI
        if (_uiManager != null)
        {
            _uiManager.CloseAllPanel();
        }

        //更新当前角色地图与目标坐标
        GameApp.Instance.PlayerCharacterManager.UpdateMapAndPosition(
            response.TargetMapId,
            response.PosX,
            response.PosY,
            response.PosZ
        );

        //清掉旧地图远端玩家
        GameApp.Instance.RemotePlayerManager.ClearAll();

        //切到目标场景
        GameApp.Instance.SceneLoaderManager.LoadMapSceneByMapId(response.TargetMapId);
    }
}