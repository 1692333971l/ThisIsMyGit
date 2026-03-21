using TMPro;
using UnityEngine;
using UnityEngine.UI;

//消息提示窗口管理器
public class MessageHintWindowManger : MonoBehaviour
{
    public static MessageHintWindowManger Instance;
    [SerializeField] private Button _closeButton;//关闭按钮
    [SerializeField] private TMP_Text _message;//显示消息

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _closeButton.onClick.AddListener(OnClickClose);
        gameObject.SetActive(false);
    }
    //打开窗口显示消息
    public void ShowMessage(string message)
    {
        _message.text = message;
        gameObject.SetActive(true);
    }
    //关闭窗口
    private void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
