using MMOServer.Database;
using MMOServer.Network;

namespace MMOServer.Core
{
    public class GameServer
    {
        private NetServer _netServer;

        public void Start()
        {
            Logger.Info("GameServer Start...");
            Logger.Info("Initialize modules...");
            Logger.Info("Server started successfully.");

            _netServer = new NetServer();
            _netServer.Start(8888);

        }
    }
}