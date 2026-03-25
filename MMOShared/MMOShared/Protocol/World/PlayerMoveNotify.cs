using System;

//玩家移动广播
namespace Protocol
{
    [Serializable]
    public class PlayerMoveNotify
    {
        public int CharacterId;

        public float PosX;
        public float PosY;
        public float PosZ;

        public float RotY;
        public bool IsMoving;
        public bool IsRunning;
    }
}