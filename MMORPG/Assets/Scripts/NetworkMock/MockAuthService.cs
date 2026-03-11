//模拟登录接口的实现
using System.Collections.Generic;

public class MockAuthService : IAuthService
{
    private readonly Dictionary<string, Dictionary<string, string>> users = new Dictionary<string, Dictionary<string, string>>();
    public MockAuthService()
    {
        users.Add("123", new Dictionary<string, string>
        {
            { "AccountId", "1001" },
            { "Password", "123" },
            { "Token", "mock_token_1002" }
        });

        users["qwe"] = new Dictionary<string, string>
        {
            { "AccountId", "1002" },
            { "Password", "qwe" },
            { "Token", "mock_token_1002" }
        };
    }
    public LoginResponse Login(string account, string password)//模拟登录接口
    {
        if (!users.ContainsKey(account))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "账号不存在",
                AccountId = 0,
                Token = null
            };
        }
        if (!users[account]["Password"].Equals(password))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "账号或密码错误",
                AccountId = 0,
                Token = null
            };
        }
        return new LoginResponse
        {
            Success = true,
            Message = "登录成功",
            AccountId = int.Parse(users[account]["AccountId"]),
            Token = users[account]["Token"]
        };
    }
    public LoginResponse Register(string account, string password)
    {
        return new LoginResponse
        {
            Success = true,
            Message = "注册成功",
            AccountId = 1002,
            Token = "mock_token_1001"
        };
     }
}
