using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//注册面板UI
public class RegisterPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _accountInput;//账号输入框
    [SerializeField] private TMP_InputField _passwordInput;//密码输入框
    [SerializeField] private TMP_InputField _confirmPassword;//密码二次确认输入框
    [SerializeField] private Button _registerButton;//注册按钮
    private void Start()
    {
        _registerButton.onClick.AddListener(OnClickRegisterButton);
        GameApp.Instance.UserService.OnRegisterResponse += HandleRegisterResponse;
    }
    private void OnDestroy()
    {
        GameApp.Instance.UserService.OnRegisterResponse -= HandleRegisterResponse;
    }
    //点击注册按钮，发送注册请求
    private void OnClickRegisterButton()
    {
        if (!_passwordInput.text.Equals(_confirmPassword.text))
        {
            MessageHintWindowManger.Instance.ShowMessage("两次密码输入不一致");
            return;
        }
        GameApp.Instance.UserService.SendRegister(_accountInput.text, _passwordInput.text);
    }
    //请求响应事件触发
    private void HandleRegisterResponse(RegisterResponse registerResponse)
    {
        switch ((ErrorCode)registerResponse.ErrorCode)
        {
            case ErrorCode.Success:
                MessageHintWindowManger.Instance.ShowMessage("注册成功");
                break;
            case ErrorCode.InvalidParams:
                MessageHintWindowManger.Instance.ShowMessage("账号或密码不能为空");
                break;
            case ErrorCode.AccountAlreadyExists:
                MessageHintWindowManger.Instance.ShowMessage("账户已存在");
                break;
            default:
                MessageHintWindowManger.Instance.ShowMessage("未知错误");
                break;
        }
    }
}
