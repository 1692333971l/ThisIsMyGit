using TMPro;
using UnityEngine;
using UnityEngine.UI;

//角色列表项视图，显示角色基本信息，并响应点击事件
public class RoleItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _roleInfoText;
    [SerializeField] private Button _button;

    private RoleData _roleData;
    private RoleSelectPanel _ownerPanel;

    //初始化自身
    public void Init(RoleData roleData, RoleSelectPanel ownerPanel)
    {
        _roleData = roleData;
        _ownerPanel = ownerPanel;

        _roleInfoText.text = $"ID:{roleData.RoleId}  名称:{roleData.RoleName}  职业:{roleData.Profession}  Lv:{roleData.Level}";

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClickItem);
    }
    //点击角色项
    private void OnClickItem()
    {
        _ownerPanel.SelectRole(_roleData);
    }
}