//玩家数据结构session
public class PlayerSession
{
    public bool IsLogin { get; private set; }//是否登录
    public int AccountId { get; private set; }//账号ID
    public string Token { get; private set; }//登录令牌

    public RoleData CurrentRole { get; private set; }//当前角色信息

    public void SetLoginInfo(int accountId, string token)//设置登录信息
    {
        IsLogin = true;
        AccountId = accountId;
        Token = token;
    }

    public void SetCurrentRole(RoleData roleData)//设置当前角色信息
    {
        CurrentRole = roleData;
    }

    public void ClearLogin()//清除登录信息
    {
        IsLogin = false;
        AccountId = 0;
        Token = string.Empty;
        CurrentRole = null;
    }
}
