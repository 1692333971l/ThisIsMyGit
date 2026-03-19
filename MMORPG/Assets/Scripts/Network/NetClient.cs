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
        byte[] buffer = new byte[2048];

        while (IsConnected)
        {
            try
            {
                int length = await _stream.ReadAsync(buffer, 0, buffer.Length);

                if (length == 0)
                {
                    Debug.LogWarning("Server disconnected.");

                    MainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        Disconnect();
                    });

                    break;
                }

                string json = Encoding.UTF8.GetString(buffer, 0, length);
                Debug.Log($"Receive raw json: {json}");

                NetMessage message = JsonUtility.FromJson<NetMessage>(json);

                MainThreadDispatcher.Instance.Enqueue(() =>
                {
                    // 交给客户端消息分发器处理，不再直接通知UI
                    GameApp.Instance.ClientMessageDispatcher.HandleMessage(message);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Receive failed: {ex.Message}");

                MainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Disconnect();
                });

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
            byte[] data = Encoding.UTF8.GetBytes(json);

            await _stream.WriteAsync(data, 0, data.Length);

            Debug.Log($"Send message json: {json}");
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
}