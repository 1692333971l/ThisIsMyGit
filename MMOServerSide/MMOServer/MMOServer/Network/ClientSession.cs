using System.Net.Sockets;
using System.Text;
using MMOServer.Core;
using Protocol;

//单个客户端会话
//接收这个客户端发来的数据,反序列化成 NetMessage交给 MessageDispatcher
//把处理结果回发给客户端
namespace MMOServer.Network
{
    public class ClientSession
    {
        public TcpClient TcpClient { get; private set; }

        private NetworkStream _stream;
        private MessageDispatcher _messageDispatcher;

        public string RemoteEndPoint => TcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        public ClientSession(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            _stream = TcpClient.GetStream();

            // 每个会话都持有一个消息分发器
            _messageDispatcher = new MessageDispatcher();
        }

        /// <summary>
        /// 启动接收循环
        /// </summary>
        public void StartReceive()
        {
            Task.Run(ReceiveLoop);
        }

        /// <summary>
        /// 持续接收客户端消息
        /// </summary>
        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[2048];

            while (TcpClient.Connected)
            {
                try
                {
                    int length = await _stream.ReadAsync(buffer, 0, buffer.Length);

                    if (length == 0)
                    {
                        Logger.Warn($"Client disconnected: {RemoteEndPoint}");
                        break;
                    }

                    string json = Encoding.UTF8.GetString(buffer, 0, length);
                    Logger.Info($"Receive raw json from {RemoteEndPoint}: {json}");

                    // 先把统一网络消息反序列化出来
                    NetMessage requestMessage = JsonHelper.FromJson<NetMessage>(json);

                    // 交给消息分发器处理
                    NetMessage responseMessage = _messageDispatcher.HandleMessage(requestMessage);

                    if (responseMessage != null)
                    {
                        SendMessage(responseMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Receive failed from {RemoteEndPoint}: {ex.Message}");
                    break;
                }
            }

            Close();
        }

        /// <summary>
        /// 发送统一网络消息给客户端
        /// </summary>
        public async void SendMessage(NetMessage message)
        {
            try
            {
                if (_stream == null || !TcpClient.Connected)
                {
                    Logger.Warn($"Send failed, client disconnected: {RemoteEndPoint}");
                    return;
                }

                string json = JsonHelper.ToJson(message);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await _stream.WriteAsync(data, 0, data.Length);

                Logger.Info($"Send json to {RemoteEndPoint}: {json}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Send failed to {RemoteEndPoint}: {ex.Message}");
            }
        }

        public void Close()
        {
            try
            {
                _stream?.Close();
                TcpClient?.Close();
            }
            catch
            {
            }
        }
    }
}