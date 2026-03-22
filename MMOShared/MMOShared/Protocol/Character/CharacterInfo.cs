using System;

//角色信息
namespace Protocol
{
    [Serializable]
    public class CharacterInfo
    {
        public int CharacterId;//角色ID
        public int UserId;//归属用户ID
        public string Name;//角色名称
        public int Profession;//职业
        public int Level;//等级
        public int Gold;//金币
        public int Hp;//当前生命值
        public int Mp;//当前法力值
        public int MapId;//地图id

        //出生点
        public float PosX;
        public float PosY;
        public float PosZ;
    }
}