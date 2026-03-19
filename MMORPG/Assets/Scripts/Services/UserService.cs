using Protocol;
using System;
using UnityEngine;

//登录注册业务层
public class UserService
{
    public event Action<LoginResponse> OnLoginResponse;
    public event Action<RegisterResponse> OnRegisterResponse;

    //登录请求
    public void SendLogin(string account, string password)
    {
        LoginRequest loginRequest = new LoginRequest()
        {
            Account = account,
            Password = password
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.LoginRequest,
            BodyJson = JsonUtility.ToJson(loginRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //登录响应
    public void HandleLoginResponse(NetMessage message)
    {
        LoginResponse response = JsonUtility.FromJson<LoginResponse>(message.BodyJson);
        if ((ErrorCode)response.ErrorCode == ErrorCode.Success)
        {
            GameApp.Instance.UserSession.UserId = response.UserId;
            GameApp.Instance.UserSession.Account = response.Account;
        }
        OnLoginResponse?.Invoke(response);
    }
    //注册请求
    public void SendRegister(string account, string password)
    {
        RegisterRequest registerRequest = new RegisterRequest()
        {
            Account = account,
            Password = password
        };
        NetMessage message = new NetMessage()
        {
            MessageId = (int)MessageId.RegisterRequest,
            BodyJson = JsonUtility.ToJson(registerRequest)
        };
        GameApp.Instance.NetClient.SendMessage(message);
    }
    //注册响应
    public void HandleRegisterResponse(NetMessage message)
    {
        RegisterResponse response = JsonUtility.FromJson<RegisterResponse>(message.BodyJson);
        OnRegisterResponse?.Invoke(response);
    }
}
