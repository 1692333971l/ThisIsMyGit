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
            while (TcpClient.Connected)
            {
                try
                {
                    byte[] lengthBuffer = new byte[4];
                    bool lengthReadSuccess = await ReadExactAsync(_stream, lengthBuffer, 4);

                    if (!lengthReadSuccess)
                    {
                        Logger.Warn($"Client disconnected: {RemoteEndPoint}");
                        break;
                    }

                    int bodyLength = BitConverter.ToInt32(lengthBuffer, 0);

                    if (bodyLength <= 0)
                    {
                        Logger.Warn($"Invalid packet length from {RemoteEndPoint}: {bodyLength}");
                        break;
                    }

                    byte[] bodyBuffer = new byte[bodyLength];
                    bool bodyReadSuccess = await ReadExactAsync(_stream, bodyBuffer, bodyLength);

                    if (!bodyReadSuccess)
                    {
                        Logger.Warn($"Client disconnected while reading body: {RemoteEndPoint}");
                        break;
                    }

                    string json = Encoding.UTF8.GetString(bodyBuffer);
                    Logger.Info($"Receive packet from {RemoteEndPoint}: {json}");

                    NetMessage requestMessage = JsonHelper.FromJson<NetMessage>(json);
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
                byte[] packet = BuildPacket(json);

                await _stream.WriteAsync(packet, 0, packet.Length);

                Logger.Info($"Send packet to {RemoteEndPoint}: {json}");
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
        private byte[] BuildPacket(string json)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
            byte[] lengthBytes = BitConverter.GetBytes(bodyBytes.Length);

            byte[] packet = new byte[4 + bodyBytes.Length];
            Buffer.BlockCopy(lengthBytes, 0, packet, 0, 4);
            Buffer.BlockCopy(bodyBytes, 0, packet, 4, bodyBytes.Length);

            return packet;
        }
        private async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int length)
        {
            int offset = 0;

            while (offset < length)
            {
                int readCount = await stream.ReadAsync(buffer, offset, length - offset);

                if (readCount == 0)
                {
                    return false;
                }

                offset += readCount;
            }

            return true;
        }
    }
}