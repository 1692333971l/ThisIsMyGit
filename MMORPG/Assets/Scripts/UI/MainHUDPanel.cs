using TMPro;
using UnityEngine;

//主界面 HUD 面板，显示玩家基本信息
public class MainHUDPanel : MonoBehaviour
{
    [Header("UserInfo")]
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private TextMeshProUGUI _profession;
    [SerializeField] private TextMeshProUGUI _hp;


    void Start()
    {
        RefreshRoleInfo();
    }

    private void RefreshRoleInfo()
    {
        RoleData currentRole = GameApp.Instance.PlayerSession.CurrentRole;

        _level.text = "等级:" + currentRole.Level;
        _profession.text = "职业:" + currentRole.Profession;
        _hp.text = "血量:" + currentRole.MaxHp + " / " + currentRole.Hp;
    }
}
