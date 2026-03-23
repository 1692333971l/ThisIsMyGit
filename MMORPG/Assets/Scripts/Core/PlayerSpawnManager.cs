using UnityEngine;

//玩家生成管理器
public class PlayerSpawnManager
{
    //生成当前玩家角色
    public GameObject SpawnCurrentPlayer()
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();//玩家信息
        ProfessionConfig professionConfig = GameApp.Instance.ProfessionConfigManager.GetById(characterInfo.Profession);//根据职业获取模型
        GameObject prefab = Resources.Load<GameObject>(professionConfig.ModelPath);//查询模型
        Vector3 spawnPosition = GameApp.Instance.PlayerCharacterManager.GetSpawnPosition();//获取出生点
        spawnPosition.y += 1f;
        GameObject playerObject = GameObject.Instantiate(prefab, spawnPosition, Quaternion.identity);//生成玩家对象
        playerObject.name = $"Player_{characterInfo.Name}_{characterInfo.CharacterId}";//设置玩家物体名字（方便查看）
        GameApp.Instance.PlayerCharacterManager.SetCharacterObject(playerObject);//保存玩家实体对象

        return playerObject;
    }
}
