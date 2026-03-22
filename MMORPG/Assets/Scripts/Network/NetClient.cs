using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Protocol;

//客户端网络层;
//连接服务端,发送消息,接收服务端消息,收到消息后交给ClientMessageDispatcher
public class NetClient
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;

    public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

    public async void Connect(string ip, int port, Action<bool> onConnectResult = null)
    {
        try
        {
            if (IsConnected)
            {
                Debug.LogWarning("Client already connected.");
                onConnectResult?.Invoke(true);
                return;
            }

            _tcpClient = new TcpClient();
            Debug.Log($"Connecting to server: {ip}:{port}");

            await _tcpClient.ConnectAsync(ip, port);

            _stream = _tcpClient.GetStream();

            Debug.Log("Connected to server successfully.");

            StartReceive();

            onConnectResult?.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connect failed: {ex.Message}");
            onConnectResult?.Invoke(false);
        }
    }

    /// <summary>
    /// 启动后台接收循环
    /// </summary>
    private void StartReceive()
    {
        Task.Run(ReceiveLoop);
    }

    /// <summary>
    /// 持续接收服务端消息
    /// </summary>
    private async Task ReceiveLoop()
    {
        while (IsConnected)
        {
            try
            {
                byte[] lengthBuffer = new byte[4];
                bool lengthReadSuccess = await ReadExactAsync(_stream, lengthBuffer, 4);

                if (!lengthReadSuccess)
                {
                    Debug.LogWarning("Server disconnected.");
                    MainThreadDispatcher.Instance.Enqueue(() => { Disconnect(); });
                    break;
                }

                int bodyLength = BitConverter.ToInt32(lengthBuffer, 0);

                if (bodyLength <= 0)
                {
                    Debug.LogError($"Invalid packet length: {bodyLength}");
                    MainThreadDispatcher.Instance.Enqueue(() => { Disconnect(); });
                    break;
                }

                byte[] bodyBuffer = new byte[bodyLength];
                bool bodyReadSuccess = await ReadExactAsync(_stream, bodyBuffer, bodyLength);

                if (!bodyReadSuccess)
                {
                    Debug.LogWarning("Server disconnected while reading packet body.");
                    MainThreadDispatcher.Instance.Enqueue(() => { Disconnect(); });
                    break;
                }

                string json = Encoding.UTF8.GetString(bodyBuffer);
                Debug.Log($"Receive packet json: {json}");

                NetMessage message = JsonUtility.FromJson<NetMessage>(json);

                MainThreadDispatcher.Instance.Enqueue(() =>
                {
                    GameApp.Instance.ClientMessageDispatcher.HandleMessage(message);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Receive failed: {ex.Message}");
                MainThreadDispatcher.Instance.Enqueue(() => { Disconnect(); });
                break;
            }
        }
    }

    /// <summary>
    /// 发送统一网络消息
    /// </summary>
    public async void SendMessage(NetMessage message)
    {
        try
        {
            if (!IsConnected || _stream == null)
            {
                Debug.LogError("Send failed: client is not connected.");
                return;
            }

            string json = JsonUtility.ToJson(message);
            byte[] packet = BuildPacket(json);

            await _stream.WriteAsync(packet, 0, packet.Length);

            Debug.Log($"Send packet json: {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        try
        {
            _stream?.Close();
            _stream = null;

            _tcpClient?.Close();
            _tcpClient = null;
        }
        catch
        {
        }

        Debug.Log("Disconnected from server.");
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