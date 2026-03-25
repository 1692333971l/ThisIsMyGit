using MMOServer.Network;

//当前在线世界里的角色状态
namespace MMOServer.World
{
    public class OnlinePlayer
    {
        public int CharacterId;
        public int UserId;
        public string Name;
        public int Profession;
        public int Level;
        public int Gold;
        public int Hp;
        public int Mp;

        public int MapId;

        public float PosX;
        public float PosY;
        public float PosZ;

        public float RotY;
        public bool IsMoving;
        public bool IsRunning;

        public ClientSession Session;
    }
}