using System;

//进入主城请求
namespace Protocol
{
    [Serializable]
    public class EnterGameRequest
    {
        // 当前用户id
        public int UserId;

        // 当前选择玩家角色id
        public int CharacterId;
    }
}