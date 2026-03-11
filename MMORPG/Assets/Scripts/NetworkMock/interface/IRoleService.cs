//角色服务接口，定义了获取角色列表和创建角色的方法
public interface IRoleService
{
    RoleListResponse GetRoleList(int accountId);//获取角色列表接口，接受账号ID参数，返回角色列表响应
    RoleData CreateRole(int accountId, string roleName, string profession, int roleModelId);//创建角色接口，接受账号ID、角色名称和职业参数，返回创建的角色数据
}
