using System;

namespace Protocol
{
    [Serializable]
    public class NetMessage
    {
        // 消息号，表示这是哪一种消息
        public int MessageId;

        // 具体业务数据，先用 JSON 字符串承载
        public string BodyJson;
    }
}