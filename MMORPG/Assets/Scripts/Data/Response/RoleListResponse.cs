using System.Collections.Generic;

//角色列表响应数据结构
public class RoleListResponse
{
    private bool success;//请求是否成功
    private string message;//请求结果消息
    private List<RoleData> roles;//角色数据列表

    public bool Success { get => success; set => success = value; }
    public string Message { get => message; set => message = value; }
    public List<RoleData> Roles { get => roles; set => roles = value; }
}
