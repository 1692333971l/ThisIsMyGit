using UnityEngine;

// 玩家实体脚本，挂载在玩家角色上，保存玩家的角色数据
public class PlayerEntity : MonoBehaviour
{
    public RoleData RoleData { get; private set; }

    public void Init(RoleData roleData)
    {
        RoleData = roleData;
        gameObject.name = $"Player_{roleData.RoleName}";
    }
}