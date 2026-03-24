using System;

//玩家进入广播
namespace Protocol
{
    [Serializable]
    public class PlayerEnterNotify
    {
        public OnlineCharacterInfo Player;
    }
}