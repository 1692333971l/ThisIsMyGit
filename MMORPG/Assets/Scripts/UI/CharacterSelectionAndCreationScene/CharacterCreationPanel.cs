using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//角色创建面板
public class CharacterCreationPanel : MonoBehaviour
{
    [SerializeField] private CharacterSelectionPanel _characterSelectionPanel;//角色选择面板
    [SerializeField] private TMP_InputField _nameInput;//名字输入框
    [SerializeField] private TMP_Dropdown _professionlSelection;//职业选择下拉框
    [SerializeField] private Button _characterCreationButton;//创建角色按钮
    [Header("属性面板")]
    [SerializeField] private TMP_Text _strength;//力量
    [SerializeField] private TMP_Text _agility;//敏捷
    [SerializeField] private TMP_Text _intelligence;//智力
    [SerializeField] private TMP_Text _critRate;//暴击率
    [SerializeField] private TMP_Text _critDamage;//暴击伤害
    [SerializeField] private TMP_Text _defense;//防御力
    [SerializeField] private TMP_Text _maxHp;//最大HP
    [SerializeField] private TMP_Text _maxMp;//最大MP
    private void Start()
    {
        GameApp.Instance.CharacterService.OnCreateCharacterResponse += HandleCreateCharacterResponse;
        _characterCreationButton.onClick.AddListener(OnClickCharacterCreationButton);
        _professionlSelection.onValueChanged.AddListener(OnValueChangedCharacterCreationButton);
    }
    private void OnDestroy()
    {
        GameApp.Instance.CharacterService.OnCreateCharacterResponse -= HandleCreateCharacterResponse;
        _characterCreationButton.onClick.RemoveAllListeners();
        _professionlSelection.onValueChanged.RemoveAllListeners();
    }
    //点击创建角色按钮，发送请求
    private void OnClickCharacterCreationButton()
    {
        if ("".Equals(_nameInput.text))
        {
            MessageHintWindowManger.Instance.ShowMessage("请输入名字");
            return;
        }
        if (_professionlSelection.value == 0)
        {
            MessageHintWindowManger.Instance.ShowMessage("请选择职业");
            return;
        }
        GameApp.Instance.CharacterService.SendCreateCharacter(_nameInput.text, _professionlSelection.value);
    }
    //创建角色响应事件触发，判定是否创建成功
    private void HandleCreateCharacterResponse(CreateCharacterResponse createCharacterResponse)
    {
        if ((ErrorCode)createCharacterResponse.ErrorCode == ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("创建成功");
            gameObject.SetActive(false);
            _characterSelectionPanel.gameObject.SetActive(true);
        }
        else
        {
            MessageHintWindowManger.Instance.ShowMessage("角色数量已达最大限制");
        }
    }
    //切换职业下拉框，动态加载职业属性与模型
    private void OnValueChangedCharacterCreationButton(int value)
    {
        ProfessionConfig professionConfig = GameApp.Instance.ProfessionConfigManager.GetById(value);
        if (professionConfig == null)
        {
            return;
        }
        _strength.text = "力量：" + professionConfig.Strength;
        _agility.text = "敏捷：" + professionConfig.Agility;
        _intelligence.text = "智力：" + professionConfig.Intelligence;
        _critRate.text = "暴击率：" + professionConfig.CritRate;
        _critDamage.text = "暴击伤害：" + professionConfig.CritDamage;
        _defense.text = "防御力：" + professionConfig.Defense;
        _maxHp.text = "最大HP：" + professionConfig.MaxHp;
        _maxMp.text = "最大MP：" + professionConfig.MaxMp;
        _characterSelectionPanel.ShowCharacterModel(professionConfig.ProfessionId);
    }
}
