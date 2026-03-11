//角色数据结构
public class RoleData
{
    private int roleId;//角色ID
    private string roleName;//角色名称
    private int level;//角色等级
    private int hp;//当前HP
    private int maxHp;//最大HP
    private string profession;//职业
    private int roleModelId;//角色模型ID

    public int RoleId { get => roleId; set => roleId = value; }
    public string RoleName { get => roleName; set => roleName = value; }
    public int Level { get => level; set => level = value; }
    public int Hp { get => hp; set => hp = value; }
    public int MaxHp { get => maxHp; set => maxHp = value; }
    public string Profession { get => profession; set => profession = value; }
    public int RoleModelId { get => roleModelId; set => roleModelId = value; }
}
