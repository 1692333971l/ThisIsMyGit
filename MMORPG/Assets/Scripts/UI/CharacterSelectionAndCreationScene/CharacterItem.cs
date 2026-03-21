using TMPro;
using UnityEngine;
using UnityEngine.UI;

//角色选择列表成员
public class CharacterItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _characterItemInfo;//角色信息显示

    private CharacterSelectionPanel _characterSelectionPanel;//角色一览面板
    private Protocol.CharacterInfo _characterInfo;//角色信息

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
    //初始化角色选择列表成员
    public void Init(Protocol.CharacterInfo characterItemInfo, CharacterSelectionPanel characterSelectionPanel)
    {
        _characterSelectionPanel = characterSelectionPanel;
        _characterInfo = characterItemInfo;
        _characterItemInfo.text = 
            "名字：" + characterItemInfo.Name
            + "，等级：" + characterItemInfo.Level
            + "，职业：" + GameApp.Instance.ProfessionConfigManager.GetById(_characterInfo.Profession).ProfessionName;
    }
    //点击事件，切换模型与保存当前选择角色信息
    private void OnClick()
    {
        _characterSelectionPanel.ShowCharacterModel(_characterInfo.Profession);
        GameApp.Instance.PlayerCharacterManager.SetCharacterInfo(_characterInfo);
    }
}
