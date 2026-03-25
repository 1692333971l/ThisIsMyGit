using MMOServer.Core;
using Protocol;
using System.Net.Sockets;
using System.Text;
using static System.Collections.Specialized.BitVector32;

// 单个客户端会话
// 作用：表示“服务器和某一个客户端之间”的连接与通信上下文
//
// 职责：
// 1. 持有这个客户端的 TcpClient 和 NetworkStream
// 2. 持续接收该客户端发来的数据
// 3. 把数据反序列化成 NetMessage
// 4. 交给 MessageDispatcher 处理
// 5. 把处理结果回发给客户端
// 6. 断线时关闭连接并通知业务层做离线清理
namespace MMOServer.Network
{
    public class ClientSession
    {
        /// <summary>
        /// 当前会话对应的 TCP 客户端连接
        /// 一个 ClientSession 对应一个 TcpClient
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// 是否已经关闭
        /// 0 = 未关闭
        /// 1 = 已关闭
        /// 
        /// 用 Interlocked 保证多线程下 Close() 只会真正执行一次
        /// </summary>
        private int _isClosed = 0;

        /// <summary>
        /// 当前连接对应的网络流
        /// 后续收发数据都通过这个流进行
        /// </summary>
        private NetworkStream _stream;

        /// <summary>
        /// 消息分发器
        /// 用来根据 MessageId 把消息交给正确的业务模块处理
        /// </summary>
        private MessageDispatcher _messageDispatcher;

        /// <summary>
        /// 当前会话对应的用户ID
        /// 一般在登录成功后赋值
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 当前会话正在使用的角色ID
        /// 一般在进入游戏成功后赋值
        /// </summary>
        public int CurrentCharacterId { get; set; }

        /// <summary>
        /// 当前客户端远程地址
        /// 例如：127.0.0.1:53214
        /// 用于日志输出，方便定位是哪一个客户端连接
        /// </summary>
        private readonly string _remoteEndPoint;
        public string RemoteEndPoint => _remoteEndPoint;

        /// <summary>
        /// 构造函数
        /// 
        /// 作用：
        /// 1. 保存 TcpClient
        /// 2. 获取对应的 NetworkStream
        /// 3. 创建消息分发器
        /// </summary>
        /// <param name="tcpClient">已经连接成功的 TCP 客户端</param>
        public ClientSession(TcpClient tcpClient)
        {
            // 保存当前连接对象
            TcpClient = tcpClient;

            // 从 TcpClient 中拿到网络流
            // 以后收发消息都通过这个流进行
            _stream = TcpClient.GetStream();

            // 每个会话都持有一个消息分发器
            // 收到消息后，交给它根据消息号转给对应业务处理
            _messageDispatcher = new MessageDispatcher();

            _remoteEndPoint = TcpClient?.Client?.RemoteEndPoint?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// 启动接收循环
        /// 
        /// 作用：
        /// 为当前客户端启动一个独立任务，持续监听它发来的消息
        /// </summary>
        public void StartReceive()
        {
            // 开一个后台任务，持续执行 ReceiveLoop
            Task.Run(ReceiveLoop);
        }

        /// <summary>
        /// 持续接收客户端消息
        /// 
        /// 这是当前会话最核心的方法。
        /// 流程：
        /// 1. 先读4字节包头（表示消息体长度）
        /// 2. 再按长度读取完整消息体
        /// 3. 把字节流转成字符串
        /// 4. 反序列化成 NetMessage
        /// 5. 交给 MessageDispatcher 处理
        /// 6. 如果有响应消息，再发回客户端
        /// 7. 只要读取失败或异常，就认为客户端断开，关闭会话
        /// </summary>
        private async Task ReceiveLoop()
        {
            // 只要连接还处于 Connected 状态，就持续接收消息
            // 注意：TcpClient.Connected 不是绝对可靠的实时状态，
            // 但你当前阶段这样写可以先接受
            while (TcpClient.Connected)
            {
                try
                {
                    // -------------------------
                    // 第一步：读取消息长度（固定4字节）
                    // -------------------------

                    // 用于保存消息长度的字节数组
                    byte[] lengthBuffer = new byte[4];

                    // 尝试从网络流中精确读取 4 个字节
                    bool lengthReadSuccess = await ReadExactAsync(_stream, lengthBuffer, 4);

                    // 如果没读成功，说明客户端断开或连接异常
                    if (!lengthReadSuccess)
                    {
                        Logger.Warn($"Client disconnected: {RemoteEndPoint}");
                        break;
                    }

                    // 把4字节长度头转成 int，表示消息体长度
                    int bodyLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // 如果长度非法（<=0），说明客户端发了错误数据
                    if (bodyLength <= 0)
                    {
                        Logger.Warn($"Invalid packet length from {RemoteEndPoint}: {bodyLength}");
                        break;
                    }

                    // -------------------------
                    // 第二步：读取消息体
                    // -------------------------

                    // 创建一个刚好等于消息体长度的字节数组
                    byte[] bodyBuffer = new byte[bodyLength];

                    // 从网络流中精确读取 bodyLength 个字节
                    bool bodyReadSuccess = await ReadExactAsync(_stream, bodyBuffer, bodyLength);

                    // 如果消息体没读完整，也认为客户端断开或连接异常
                    if (!bodyReadSuccess)
                    {
                        Logger.Warn($"Client disconnected while reading body: {RemoteEndPoint}");
                        break;
                    }

                    // -------------------------
                    // 第三步：字节流 -> JSON 字符串
                    // -------------------------

                    // 把消息体字节转成 UTF8 字符串
                    string json = Encoding.UTF8.GetString(bodyBuffer);

                    // 输出接收日志，方便调试查看客户端发了什么
                    Logger.Info($"Receive packet from {RemoteEndPoint}: {json}");

                    // -------------------------
                    // 第四步：JSON -> NetMessage
                    // -------------------------

                    // 把 JSON 字符串反序列化成统一网络消息对象
                    NetMessage requestMessage = JsonHelper.FromJson<NetMessage>(json);

                    // -------------------------
                    // 第五步：消息分发处理
                    // -------------------------

                    // 把消息交给消息分发器处理
                    // this 代表当前 session，一起传进去
                    // 因为某些业务（如移动、退出）需要知道是谁发的
                    NetMessage responseMessage = _messageDispatcher.HandleMessage(requestMessage, this);

                    // -------------------------
                    // 第六步：如果有响应，则发回客户端
                    // -------------------------

                    // 有些请求会返回响应消息，例如登录、进入游戏
                    // 有些请求可能只是通知处理，不返回内容，例如某些广播逻辑
                    if (responseMessage != null)
                    {
                        SendMessage(responseMessage);
                    }
                }
                catch (Exception ex)
                {
                    // 收包、解析、处理任意一步出异常，都认为本次接收失败
                    Logger.Error($"Receive failed from {RemoteEndPoint}: {ex.Message}");
                    break;
                }
            }

            // 跳出循环后，说明客户端已断开或出现异常
            // 执行关闭逻辑
            Close();
        }

        /// <summary>
        /// 发送统一网络消息给客户端
        /// 
        /// 流程：
        /// 1. 把 NetMessage 序列化成 JSON
        /// 2. 包装成 “长度头 + 消息体” 的完整数据包
        /// 3. 写入网络流发给客户端
        /// </summary>
        /// <param name="message">要发送的网络消息</param>
        public async void SendMessage(NetMessage message)
        {
            try
            {
                // 如果网络流为空，或者 TCP 已断开，则不能发送
                if (_isClosed == 1 || _stream == null || TcpClient == null || !TcpClient.Connected)
                {
                    Logger.Warn($"Send failed, client disconnected: {RemoteEndPoint}");
                    return;
                }

                // 把消息对象序列化成 JSON 字符串
                string json = JsonHelper.ToJson(message);

                // 构造完整数据包（4字节长度头 + JSON消息体）
                byte[] packet = BuildPacket(json);

                // 把完整数据包写入网络流
                await _stream.WriteAsync(packet, 0, packet.Length);

                // 打印发送日志
                Logger.Info($"Send packet to {RemoteEndPoint}: {json}");
            }
            catch (Exception ex)
            {
                // 发送失败则记录日志
                Logger.Error($"Send failed to {RemoteEndPoint}: {ex.Message}");
            }
        }

        /// <summary>
        /// 关闭当前会话（幂等）
        /// 
        /// 作用：
        /// 1. 保证同一个会话只会真正关闭一次
        /// 2. 通知业务层处理玩家掉线
        /// 3. 释放网络流和 TCP 连接
        /// 4. 输出日志，便于排查问题
        /// </summary>
        public void Close()
        {
            // 如果已经关闭过了，则直接返回
            // CompareExchange 的意思：
            // - 如果当前 _isClosed == 0，就把它改成 1，并返回旧值 0
            // - 如果当前 _isClosed != 0，说明已经关闭过，直接返回旧值
            if (Interlocked.CompareExchange(ref _isClosed, 1, 0) != 0)
            {
                return;
            }

            Logger.Warn($"Session closing: {RemoteEndPoint}, CharacterId={CurrentCharacterId}, UserId={UserId}");

            // 第一步：通知业务层处理掉线
            try
            {
                GameServer.Instance.WorldService.HandlePlayerDisconnect(this);
            }
            catch (Exception ex)
            {
                Logger.Error($"HandlePlayerDisconnect failed: {RemoteEndPoint}, Error={ex.Message}");
                Logger.Error(ex.ToString());
            }

            // 第二步：关闭网络流
            try
            {
                _stream?.Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Close stream failed: {RemoteEndPoint}, Error={ex.Message}");
                Logger.Error(ex.ToString());
            }
            finally
            {
                _stream = null;
            }

            // 第三步：关闭 TCP 连接
            try
            {
                TcpClient?.Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Close tcp client failed: {RemoteEndPoint}, Error={ex.Message}");
                Logger.Error(ex.ToString());
            }
            finally
            {
                TcpClient = null;
            }

            Logger.Warn($"Session closed: {RemoteEndPoint}");
        }

        /// <summary>
        /// 构造完整数据包
        /// 
        /// 数据包格式：
        /// [4字节消息体长度][消息体JSON字节]
        /// 
        /// 作用：
        /// 让接收端知道后面要读多少字节才算一条完整消息
        /// </summary>
        /// <param name="json">要发送的 JSON 字符串</param>
        /// <returns>完整网络数据包字节数组</returns>
        private byte[] BuildPacket(string json)
        {
            // 把 JSON 字符串转成 UTF8 字节数组，作为消息体
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            // 把消息体长度转成4字节数组，作为包头
            byte[] lengthBytes = BitConverter.GetBytes(bodyBytes.Length);

            // 创建完整数据包数组 = 4字节长度头 + 消息体长度
            byte[] packet = new byte[4 + bodyBytes.Length];

            // 把长度头拷贝到 packet 的前4字节
            Buffer.BlockCopy(lengthBytes, 0, packet, 0, 4);

            // 把消息体拷贝到 packet 的后半部分
            Buffer.BlockCopy(bodyBytes, 0, packet, 4, bodyBytes.Length);

            // 返回完整包
            return packet;
        }

        /// <summary>
        /// 从网络流中“精确读取指定长度”的字节
        /// 
        /// 为什么需要它：
        /// 因为一次 ReadAsync 不保证就能读满你想要的长度，
        /// 所以要循环读，直到读满或连接断开。
        /// </summary>
        /// <param name="stream">网络流</param>
        /// <param name="buffer">目标缓冲区</param>
        /// <param name="length">期望读取的总字节数</param>
        /// <returns>
        /// true  = 成功读满指定长度
        /// false = 对端断开，提前读不到了
        /// </returns>
        private async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int length)
        {
            // 当前已经读取到的位置偏移
            int offset = 0;

            // 只要还没读满 length，就继续读
            while (offset < length)
            {
                // 从 stream 继续读取剩余字节
                // offset 表示写入 buffer 的起始位置
                // length - offset 表示这次最多还需要读多少字节
                int readCount = await stream.ReadAsync(buffer, offset, length - offset);

                // 如果读到 0，说明对端已经关闭连接
                if (readCount == 0)
                {
                    return false;
                }

                // 把已读取的字节数累计到 offset
                offset += readCount;
            }

            // 读满了指定长度，返回成功
            return true;
        }
    }
}