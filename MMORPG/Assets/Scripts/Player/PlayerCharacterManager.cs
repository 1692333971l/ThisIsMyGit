using Protocol;
using UnityEngine;

//当前本地玩家角色数据管理器
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
    //更新地图和坐标
    public void UpdateMapAndPosition(int mapId, float posX, float posY, float posZ)
    {
        if (_currentCharacterInfo == null)
        {
            return;
        }
        _currentCharacterInfo.MapId = mapId;
        _currentCharacterInfo.PosX = posX;
        _currentCharacterInfo.PosY = posY;
        _currentCharacterInfo.PosZ = posZ;
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
        _currentCharacterObject = null;
    }
}
