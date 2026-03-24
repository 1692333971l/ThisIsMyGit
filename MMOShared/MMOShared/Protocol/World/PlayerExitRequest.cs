using System;

//玩家退出请求
namespace Protocol
{
    [Serializable]
    public class PlayerExitRequest
    {
        public int CharacterId;
    }
}