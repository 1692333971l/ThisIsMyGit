using System;

//玩家退出广播
namespace Protocol
{
    [Serializable]
    public class PlayerLeaveNotify
    {
        public int CharacterId;
    }
}