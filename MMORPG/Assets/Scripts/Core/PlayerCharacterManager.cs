using UnityEngine;

//玩家当前控制角色管理器
public class PlayerCharacterManager
{
    private Protocol.CharacterInfo _currentCharacterInfo;//当前玩家信息
    private GameObject _currentCharacterObject;//当前玩家实体对象

    //获取当前玩家信息
    public Protocol.CharacterInfo GetCharacterInfo()
    {
        return _currentCharacterInfo;
    }
    //设置当前玩家信息
    public void SetCharacterInfo(Protocol.CharacterInfo characterInfo)
    {
        _currentCharacterInfo = characterInfo;
    }
    //获取当前角色实例对象
    public GameObject GetCharacterObject()
    {
        return _currentCharacterObject;
    }
    //设置当前玩家实例对象
    public void SetCharacterObject(GameObject characterObject)
    {
        _currentCharacterObject = characterObject;
    }
    //保存进入游戏后的完整数据
    public void SetEnterGameData(Protocol.CharacterInfo characterInfo)
    {
        _currentCharacterInfo = characterInfo;
    }
    //获取地图Id
    public int GetMapId()
    {
        return _currentCharacterInfo.MapId;
    }
    //获取出生坐标
    public Vector3 GetSpawnPosition()
    {
        return new Vector3(_currentCharacterInfo.PosX, _currentCharacterInfo.PosY, _currentCharacterInfo.PosZ);
    }
    //当前是否已经有角色数据
    public bool HasCharacterInfo()
    {
        return _currentCharacterInfo != null;
    }
    //清空
    public void Clear()
    {
        _currentCharacterInfo = null;
    }
}
