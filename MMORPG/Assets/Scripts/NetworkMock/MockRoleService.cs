using System.Collections.Generic;

// 模拟角色服务，提供角色列表和创建角色的功能
public class MockRoleService : IRoleService
{
    private readonly Dictionary<int, List<RoleData>> _roleDataDict = new Dictionary<int, List<RoleData>>();

    public MockRoleService()
    {
        _roleDataDict[1001] = new List<RoleData>
        {
            new RoleData
            {
                RoleId = 1,
                RoleName = "Knight_A",
                Level = 12,
                Hp = 320,
                MaxHp = 320,
                Profession = "弓箭手",
                RoleModelId = 0
            },
            new RoleData
            {
                RoleId = 2,
                RoleName = "Mage_B",
                Level = 8,
                Hp = 180,
                MaxHp = 180,
                Profession = "法师",
                RoleModelId = 2
            }
        };
    }

    public RoleListResponse GetRoleList(int accountId)
    {
        if (!_roleDataDict.ContainsKey(accountId))
        {
            _roleDataDict[accountId] = new List<RoleData>();
        }

        return new RoleListResponse
        {
            Success = true,
            Message = "获取角色列表成功",
            Roles = _roleDataDict[accountId]
        };
    }

    public RoleData CreateRole(int accountId, string roleName, string profession, int roleModelId)
    {
        if (!_roleDataDict.ContainsKey(accountId))
        {
            _roleDataDict[accountId] = new List<RoleData>();
        }

        List<RoleData> roleList = _roleDataDict[accountId];
        int newRoleId = roleList.Count + 1;

        RoleData newRole = new RoleData
        {
            RoleId = newRoleId,
            RoleName = roleName,
            Level = 1,
            Hp = 100,
            MaxHp = 100,
            Profession = profession,
            RoleModelId = roleModelId
        };

        roleList.Add(newRole);
        return newRole;
    }
}