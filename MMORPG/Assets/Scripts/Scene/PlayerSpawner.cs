using System.Collections.Generic;
using UnityEngine;
// 玩家生成器，负责在场景中生成玩家角色并初始化相关组件
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> _previewRoles = new List<GameObject>();//角色预览模型列表
    [SerializeField] private GameObject _playerPrefab;//玩家预制体引用
    [SerializeField] private Transform _spawnPoint;//玩家生成点位置引用
    [SerializeField] private CameraFollow _cameraFollow;//摄像机跟随组件引用
    [SerializeField] private NpcDialogPanel _npcDialogPanel;// NPC 对话面板引用

    private void Start()
    {
        _playerPrefab = _previewRoles[GameApp.Instance.PlayerSession.CurrentRole.RoleModelId];
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        RoleData currentRole = GameApp.Instance.PlayerSession.CurrentRole;//获取当前角色数据
        GameObject playerObject = Instantiate(_playerPrefab, _spawnPoint.position, _spawnPoint.rotation);//在生成点位置实例化玩家预制体
        playerObject.GetComponent<PlayerController>().SetCameraTranform(_cameraFollow.gameObject.transform);//设置玩家控制器的摄像机变换

        PlayerEntity playerEntity = playerObject.GetComponent<PlayerEntity>();
        playerEntity.Init(currentRole);//赋予当前角色自身属性

        _cameraFollow.SetTarget(playerObject.transform);//将摄像机跟随目标设置为玩家对象
    }
}