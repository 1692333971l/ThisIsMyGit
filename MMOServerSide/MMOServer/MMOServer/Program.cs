using MMOServer.Core;

//服务端程序入口
namespace MMOServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logger.Info("MMO Server starting...");

            GameServer server = new GameServer();
            server.Start();

            Logger.Info("Press any key to exit.");
            Console.ReadKey();
        }
    }
}