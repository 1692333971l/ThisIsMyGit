//NPC数据响应数据结构
public class NPCDataResponse
{
    private bool success;//请求是否成功
    private string message;//请求结果消息
    private NPCData data;//NPC数据列表
    public bool Success { get => success; set => success = value; }
    public string Message { get => message; set => message = value; }
    public NPCData Data { get => data; set => data = value; }
}
