using UnityEngine;

//玩家当前控制角色管理器
public class PlayerCharacterManager
{
    private Protocol.CharacterInfo _selectedCharacter;//当前玩家信息
    private int _mapId;//当前地图Id
    private Vector3 _spawnPosition;//出生坐标

    //获取当前玩家信息
    public Protocol.CharacterInfo GetCharacterInfo()
    {
        return _selectedCharacter;
    }

    //设置当前玩家信息
    public void SetCharacterInfo(Protocol.CharacterInfo characterInfo)
    {
        _selectedCharacter = characterInfo;
    }

    //保存进入游戏后的完整数据
    public void SetEnterGameData(Protocol.CharacterInfo characterInfo)
    {
        _selectedCharacter = characterInfo;
        _mapId = characterInfo.MapId;
        _spawnPosition = new Vector3(characterInfo.PosX, characterInfo.PosY, characterInfo.PosZ);
    }

    //获取地图Id
    public int GetMapId()
    {
        return _mapId;
    }

    //获取出生坐标
    public Vector3 GetSpawnPosition()
    {
        return _spawnPosition;
    }

    //清空
    public void Clear()
    {
        _selectedCharacter = null;
        _mapId = 0;
        _spawnPosition = Vector3.zero;
    }
}
