using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//角色选择面板
public class CharacterSelectionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _userInfo;//登录用户信息
    [SerializeField] private Transform _characterListRoot;//角色列表根节点
    [SerializeField] private CharacterItem _characterItem;//角色列表成员
    [SerializeField] private Transform _characterViewPiont;//角色展示框根节点
    [SerializeField] private Button _logoutButton;//登出按钮
    [SerializeField] private Button _startGameButton;//游戏开始按钮

    private GameObject _characterModel;//当前展示角色模型
    private void Start()
    {
        SetUserInfo();
        GameApp.Instance.CharacterService.OnGetCharacterListResponse += HandleGetCharacterListResponse;
        _logoutButton.onClick.AddListener(OnClickLogoutButton);
        _startGameButton.onClick.AddListener(OnClickStartGameButton);
    }
    private void OnDestroy()
    {
        GameApp.Instance.CharacterService.OnGetCharacterListResponse -= HandleGetCharacterListResponse;
        _logoutButton.onClick.RemoveAllListeners();
        _startGameButton.onClick.RemoveAllListeners();
    }
    //每次面板打开时，请求获取角色一览
    private void OnEnable()
    {
        GameApp.Instance.CharacterService.SendGetCharacterList();
    }
    //设置用户信息
    private void SetUserInfo()
    {
        _userInfo.text = "用户名：" + GameApp.Instance.UserSession.GetAccount();
    }
    //获取角色一览响应事件触发，清空当前展示，重新生成一览
    private void HandleGetCharacterListResponse(GetCharacterListResponse characterListResponse)
    {
        if ((ErrorCode)characterListResponse.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("角色列表获取失败");
            return;
        }
        //清空列表
        for (int i = _characterListRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_characterListRoot.GetChild(i).gameObject);
        }
        //生成列表项
        foreach (var characterData in characterListResponse.CharacterList)
        {
            CharacterItem item = Instantiate(_characterItem, _characterListRoot);
            item.Init(characterData, GetComponent<CharacterSelectionPanel>());
        }
    }
    //开始游戏按钮点击事件
    private void OnClickStartGameButton()
    {
        if (GameApp.Instance.PlayerCharacterManager.GetCharacterInfo() == null)
        {
            MessageHintWindowManger.Instance.ShowMessage("请选择角色");
            return;
        }
        GameApp.Instance.SceneLoaderManager.LoadMainCity();
    }
    //登出按钮点击事件
    private void OnClickLogoutButton()
    {
        GameApp.Instance.UserSession.Clear();
        GameApp.Instance.SceneLoaderManager.LoadLoginAndRegistrationScene();
    }
    //根据职业ID展示模型
    public void ShowCharacterModel(int Profession)
    {
        if (_characterModel != null)
        {
            Destroy(_characterModel);
        }
        GameObject characterModel = Resources.Load<GameObject>(GameApp.Instance.ProfessionConfigManager.GetById(Profession).ModelPath);
        _characterModel = Instantiate(characterModel, _characterViewPiont);
    }
}
