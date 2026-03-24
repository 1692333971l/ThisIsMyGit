using System;

//玩家移动请求
namespace Protocol
{
    [Serializable]
    public class PlayerMoveRequest
    {
        public int CharacterId;

        public float PosX;
        public float PosY;
        public float PosZ;

        public float RotY;
        public bool IsMoving;
    }
}