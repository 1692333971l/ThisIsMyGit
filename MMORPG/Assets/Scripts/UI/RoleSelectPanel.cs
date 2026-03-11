using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 角色选择面板，包含创建角色和选择角色功能，显示登录信息和消息提示
public class RoleSelectPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private Image _createPolePanel;//创建角色面板
    [SerializeField] private Image _selectPolePanel;//选择角色面板

    [Header("Preview")]
    [SerializeField] private List<GameObject> _previewRoles = new List<GameObject>();//角色预览模型列表
    [SerializeField] private Transform _previewSpawnPoint;//角色预览生成点
    [SerializeField] private TMP_Dropdown _modelsDropdown;//模型选择下拉框

    [Header("Top Info")]
    [SerializeField] private TextMeshProUGUI _loginIfo;//登录账号信息显示文本
    [SerializeField] private TMP_Text _messageText;//消息显示文本
    [SerializeField] private Button _logoutNutton;//登出登录按钮

    [Header("Create Role")]
    [SerializeField] private TMP_InputField _createNameInput;//创建角色名称输入框
    [SerializeField] private TMP_Dropdown _professionDropdown;//职业选择下拉框

    [Header("Role List")]
    [SerializeField] private Transform _roleListRoot;//角色列表根节点
    [SerializeField] private RoleItemView _roleItemPrefab;//角色列表项预制体

    private RoleData _selectedRole;//当前选中的角色数据
    private GameObject _currentPreview;//当前预览的角色模型
    private List<RoleItemView> _roleItemViews = new List<RoleItemView>();//当前显示的角色列表项
    void Start()
    {
        _loginIfo.text = "登录zhangh：" + GameApp.Instance.PlayerSession.AccountId;//显示登录账号信息 
        RefreshRoleList();
    }
    //登出按钮点击事件
    public void OnClickLogout()
    {
        GameApp.Instance.PlayerSession.ClearLogin();//清除登录信息
        GameApp.Instance.SceneLoader.LoadLoginScene();//加载登录场景
    }
    //创建角色按钮点击事件
    public void OnclickCreateButton()
    {
        string roleName = _createNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            SetMessage("角色名不能为空");
            return;
        }
        GameApp.Instance.RoleService.CreateRole(GameApp.Instance.PlayerSession.AccountId, roleName, _professionDropdown.options[_professionDropdown.value].text, _modelsDropdown.value);//创建角色
        SetMessage("创建角色成功");
        RefreshRoleList();
        _createPolePanel.gameObject.SetActive(false);
        _selectPolePanel.gameObject.SetActive(true);
    }
    //确定当前选择角色
    public void SelectRole(RoleData roleData)
    {
        _selectedRole = roleData;
        PreviewRole(roleData.RoleModelId);
        SetMessage($"已选中角色：{roleData.RoleName}");
    }
    //预览角色事件
    public void PreviewRole(int roleModelId)
    {
        if (_currentPreview != null)
        {
            Destroy(_currentPreview);
        }
        GameObject previewPrefab = _previewRoles[roleModelId];
        _currentPreview = Instantiate(previewPrefab, _previewSpawnPoint.position, _previewSpawnPoint.rotation);
    }
    //进入游戏按钮点击事件
    public void OnClickEnterGame()
    {
        if (_selectedRole == null)
        {
            SetMessage("请先选择一个角色");
            return;
        }

        GameApp.Instance.PlayerSession.SetCurrentRole(_selectedRole);
        GameApp.Instance.SceneLoader.LoadMainCityScene();
    }
    //刷新角色列表显示
    private void RefreshRoleList()
    {
        ClearRoleList();
        int accountId = GameApp.Instance.PlayerSession.AccountId;
        RoleListResponse response = GameApp.Instance.RoleService.GetRoleList(accountId);
        foreach (RoleData roleData in response.Roles)
        {
            RoleItemView item = Instantiate(_roleItemPrefab, _roleListRoot);
            item.Init(roleData, this);
            _roleItemViews.Add(item);
        }
    }
    //清除角色列表并销毁无用组件显示
    private void ClearRoleList()
    {
        foreach (RoleItemView item in _roleItemViews)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        _roleItemViews.Clear();
    }
    //显示消息
    private void SetMessage(string message)
    {
        _messageText.text = message;
    }
}
