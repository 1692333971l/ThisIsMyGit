using System;

//传送响应
namespace Protocol
{
    [Serializable]
    public class TeleportResponse
    {
        public int ErrorCode;
        public string Message;

        public int CharacterId;
        public int TargetMapId;

        public float PosX;
        public float PosY;
        public float PosZ;
    }
}