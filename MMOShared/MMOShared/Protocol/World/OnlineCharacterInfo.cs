using System;

//世界玩家信息
namespace Protocol
{
    [Serializable]
    public class OnlineCharacterInfo
    {
        public int CharacterId;
        public int UserId;
        public string Name;
        public int Profession;
        public int Level;
        public int Gold;
        public int Hp;
        public int Mp;
        //地图id
        public int MapId;
        //玩家坐标
        public float PosX;
        public float PosY;
        public float PosZ;
        //玩家朝向
        public float RotY;
        //是否移动
        public bool IsMoving;
    }
}