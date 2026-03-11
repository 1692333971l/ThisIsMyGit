using System.Collections.Generic;
using UnityEngine;

// 玩家生成器，负责在场景中生成玩家对象并设置摄像机跟随
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;//玩家出生点  
    [SerializeField] private CameraFollow _cameraFollow;//摄像机跟随脚本
    [SerializeField] private List<GameObject> _previewRoles = new List<GameObject>();//角色预览模型列表
    private GameObject _playerPrefab;//玩家预制体

    private void Start()
    {
        _playerPrefab = _previewRoles[GameApp.Instance.PlayerSession.CurrentRole.RoleModelId];
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject playerObject = Instantiate(_playerPrefab, _spawnPoint.position, _spawnPoint.rotation);//实例化玩家对象
        _cameraFollow.SetTarget(playerObject.transform);//设置摄像机跟随玩家
        playerObject.GetComponent<PlayerController>().SetCameraTranform(_cameraFollow.gameObject.transform);//设置玩家控制器的摄像机变换
    }
}