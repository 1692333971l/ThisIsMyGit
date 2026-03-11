using TMPro;
using UnityEngine;

//登录面板，包含账号输入框、密码输入框、登录按钮和消息显示文本
public class LoginPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _accountInput;//账号输入框
    [SerializeField] private TMP_InputField _passwordInput;//密码输入框
    [SerializeField] private TMP_Text _messageText;//消息显示文本

    //登录按钮点击事件
    public void OnClickLogin()
    {
        string account = _accountInput.text.Trim();
        string password = _passwordInput.text.Trim();

        LoginResponse response = GameApp.Instance.AuthService.Login(account, password);//调用登录方法获取响应

        if (!response.Success)//如果登录失败，显示错误消息
        {
            SetMessage(response.Message);
            return;
        }

        GameApp.Instance.PlayerSession.SetLoginInfo(response.AccountId, response.Token);//设置登录信息
        GameApp.Instance.SceneLoader.LoadSelectRoleScene();//加载选择角色场景
    }

    //设置消息文本
    private void SetMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }
}