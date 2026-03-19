using TMPro;
using UnityEngine;
using UnityEngine.UI;

//消息提示窗口管理器
public class MessageHintWindowManger : MonoBehaviour
{
    public static MessageHintWindowManger Instance;
    [SerializeField]
    private Button _closeButton;
    [SerializeField]
    private TMP_Text _message;

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
    public void ShowMessage(string message)
    {
        _message.text = message;
        gameObject.SetActive(true);
    }

    private void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
