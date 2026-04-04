using System;

//传送请求
namespace Protocol
{
    [Serializable]
    public class TeleportRequest
    {
        public int CharacterId;//角色id
        public int TargetPortalId;//目标传送点id
    }
}