using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//注册面板UI
public class RegisterPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _accountInput;
    [SerializeField]
    private TMP_InputField _passwordInput;
    [SerializeField]
    private TMP_InputField _confirmPassword;
    [SerializeField]
    private Button _registerButton;
    [SerializeField]
    private Button _backButton;

    private void Awake()
    {
        _registerButton.onClick.AddListener(OnClickRegisterButton);
    }
    private void Start()
    {
        GameApp.Instance.UserService.OnRegisterResponse += HandleRegisterResponse;
    }
    private void OnDestroy()
    {
        GameApp.Instance.UserService.OnRegisterResponse -= HandleRegisterResponse;
    }
    private void OnClickRegisterButton()
    {
        if (!_passwordInput.text.Equals(_confirmPassword.text))
        {
            MessageHintWindowManger.Instance.ShowMessage("两次密码输入不一致");
            return;
        }
        GameApp.Instance.UserService.SendRegister(_accountInput.text, _passwordInput.text);
    }
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
