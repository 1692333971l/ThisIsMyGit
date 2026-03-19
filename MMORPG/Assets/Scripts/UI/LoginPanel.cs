using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//登录面板UI
public class LoginPanel : MonoBehaviour
{
    [SerializeField]
    private Button _loginButton;
    [SerializeField]
    private TMP_InputField _accountInput;
    [SerializeField]
    private TMP_InputField _passwordInput;

    private void Awake()
    {
        _loginButton.onClick.AddListener(OnClickLoginButton);
    }
    private void Start()
    {
        GameApp.Instance.UserService.OnLoginResponse += HandleLoginResponse;
    }
    private void OnDestroy()
    {
        GameApp.Instance.UserService.OnLoginResponse -= HandleLoginResponse;
    }
    private void OnClickLoginButton()
    {
        GameApp.Instance.UserService.SendLogin(_accountInput.text, _passwordInput.text);
    }
    private void HandleLoginResponse(LoginResponse loginResponse)
    {
        switch ((ErrorCode)loginResponse.ErrorCode)
        {
            case ErrorCode.Success:
                GameApp.Instance.SceneLoaderManager.LoadCharacterSelectionAndCreationScene(); 
                break;
            case ErrorCode.InvalidParams:
                MessageHintWindowManger.Instance.ShowMessage("账号或密码不能为空");
                break;
            case ErrorCode.AccountNotFound:
                MessageHintWindowManger.Instance.ShowMessage("账户未找到");
                break;
            case ErrorCode.PasswordIncorrect:
                MessageHintWindowManger.Instance.ShowMessage("密码错误");
                break;
            default:
                MessageHintWindowManger.Instance.ShowMessage("未知错误");
                break;
        }
    }
}
