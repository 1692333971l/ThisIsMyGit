//登陆服务接口
public interface IAuthService
{
    LoginResponse Login(string account, string password);//登录接口，接受账号和密码参数，返回登录响应
    LoginResponse Register(string account, string password);//注册接口，接受账号和密码参数，返回注册响应

}
