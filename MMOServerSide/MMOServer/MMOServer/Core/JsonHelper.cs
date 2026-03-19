using System.Text.Json;

namespace MMOServer.Core
{
    public static class JsonHelper
    {
        // 关键点：Unity 协议类用的是 public 字段，不是属性
        // System.Text.Json 默认不处理字段，所以这里必须开启 IncludeFields
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        /// <summary>
        /// 对象转 JSON
        /// </summary>
        public static string ToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }

        /// <summary>
        /// JSON 转对象
        /// </summary>
        public static T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}