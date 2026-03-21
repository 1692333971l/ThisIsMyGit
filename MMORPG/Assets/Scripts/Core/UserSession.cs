//持久化数据
public class UserSession
{
    private int UserId;//用户Id
    private string Account;//账号名

    public void SetUserId(int userId)
    {
        UserId =  userId;
    }
    public void SetAccount(string account)
    {
        Account = account;
    }
    public int GetUserId()
    {
        return UserId; 
    }
    public string GetAccount()
    {
        return Account;
    }
    public void Clear()
    {
        UserId = 0;
        Account = null;
    }
}
