using UnityEngine;

//主城控制器
public class MainCityEntryController : MonoBehaviour
{
    [SerializeField] CameraFollow _cameraFollow;//跟随摄像机
    private void Start()
    {
        GameObject playerObject = GameApp.Instance.PlayerSpawnManager.SpawnCurrentPlayer();//生成玩家实体
        _cameraFollow.SetTarget(playerObject.transform);//设置摄像机跟随目标
        playerObject.GetComponent<PlayerMovementController>().SetCameraTranform(_cameraFollow.transform);//设置玩家身上的摄像机
        playerObject.GetComponent<PlayerCharacterView>().SetupAsLocalPlayer();//启用本地模式
    }
}
