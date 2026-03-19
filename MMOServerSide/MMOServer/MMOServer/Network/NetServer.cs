using System.Net;
using System.Net.Sockets;
using MMOServer.Core;

namespace MMOServer.Network
{
    public class NetServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        // 保存所有客户端会话，后面做群发、广播时会用到
        private readonly List<ClientSession> _sessions = new List<ClientSession>();

        public void Start(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _isRunning = true;

            Logger.Info($"NetServer started, listening on port {port}");

            Task.Run(ListenLoop);
        }

        private async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    ClientSession session = new ClientSession(tcpClient);

                    _sessions.Add(session);

                    Logger.Info($"Client connected: {session.RemoteEndPoint}");

                    // 新客户端接入后，启动它自己的接收循环
                    session.StartReceive();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Accept client failed: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;

            foreach (var session in _sessions)
            {
                session.Close();
            }

            _sessions.Clear();
            _listener?.Stop();

            Logger.Info("NetServer stopped.");
        }
    }
}