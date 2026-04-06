using Protocol;
using TMPro;
using UnityEngine;

//装备面板
public class EquipmentPanel : MonoBehaviour
{
    [Header("装备格子")]
    [SerializeField] private EquipmentSlotItem _weaponSlotItem;//武器槽
    [SerializeField] private EquipmentSlotItem _headSlotItem;//头部槽
    [SerializeField] private EquipmentSlotItem _armorSlotItem;//衣服槽
    [SerializeField] private EquipmentSlotItem _handSlotItem;//手部槽
    [SerializeField] private EquipmentSlotItem _legSlotItem;//腿部槽
    [SerializeField] private EquipmentSlotItem _accessorySlotItem;//饰品槽

    [Header("最终属性文本")]
    [SerializeField] private TMP_Text _strengthText;
    [SerializeField] private TMP_Text _agilityText;
    [SerializeField] private TMP_Text _intelligenceText;
    [SerializeField] private TMP_Text _critRateText;
    [SerializeField] private TMP_Text _critDamageText;
    [SerializeField] private TMP_Text _defenseText;
    [SerializeField] private TMP_Text _maxHpText;
    [SerializeField] private TMP_Text _maxMpText;

    [Header("装备操作菜单")]
    [SerializeField] private EquipmentItemOperateMenu _equipmentItemOperateMenu;

    private void Awake()
    {
        RegisterSlotEvent(_weaponSlotItem);
        RegisterSlotEvent(_headSlotItem);
        RegisterSlotEvent(_armorSlotItem);
        RegisterSlotEvent(_handSlotItem);
        RegisterSlotEvent(_legSlotItem);
        RegisterSlotEvent(_accessorySlotItem);

        if (_equipmentItemOperateMenu != null)
        {
            _equipmentItemOperateMenu.gameObject.SetActive(false);
        }
    }
    private void OnEnable()
    {
        Init();
    }

    private void OnDestroy()
    {
        UnRegisterSlotEvent(_weaponSlotItem);
        UnRegisterSlotEvent(_headSlotItem);
        UnRegisterSlotEvent(_armorSlotItem);
        UnRegisterSlotEvent(_handSlotItem);
        UnRegisterSlotEvent(_legSlotItem);
        UnRegisterSlotEvent(_accessorySlotItem);
    }

    public void Init()
    {
        RefreshEquipmentSlots();
        RefreshFinalCharacterInfo();
    }

    private void RefreshEquipmentSlots()
    {
        PlayerEquipmentManager playerEquipmentManager = GameApp.Instance.PlayerEquipmentManager;

        _weaponSlotItem.Init(1, playerEquipmentManager.GetEquipmentBySlotType(1));
        _headSlotItem.Init(2, playerEquipmentManager.GetEquipmentBySlotType(2));
        _armorSlotItem.Init(3, playerEquipmentManager.GetEquipmentBySlotType(3));
        _handSlotItem.Init(4, playerEquipmentManager.GetEquipmentBySlotType(4));
        _legSlotItem.Init(5, playerEquipmentManager.GetEquipmentBySlotType(5));
        _accessorySlotItem.Init(6, playerEquipmentManager.GetEquipmentBySlotType(6));
    }

    private void RefreshFinalCharacterInfo()
    {
        Protocol.CharacterInfo characterInfo = GameApp.Instance.PlayerCharacterManager.GetCharacterInfo();
        if (characterInfo == null)
        {
            return;
        }

        if (_strengthText != null) _strengthText.text = "力量：" + characterInfo.Strength;
        if (_agilityText != null) _agilityText.text = "敏捷：" + characterInfo.Agility;
        if (_intelligenceText != null) _intelligenceText.text = "智力：" + characterInfo.Intelligence;
        if (_critRateText != null) _critRateText.text = "暴击率：" + characterInfo.CritRate;
        if (_critDamageText != null) _critDamageText.text = "暴击伤害：" + characterInfo.CritDamage;
        if (_defenseText != null) _defenseText.text = "防御力：" + characterInfo.Defense;
        if (_maxHpText != null) _maxHpText.text = "最大HP：" + characterInfo.MaxHp;
        if (_maxMpText != null) _maxMpText.text = "最大MP：" + characterInfo.MaxMp;
    }

    private void RegisterSlotEvent(EquipmentSlotItem slotItem)
    {
        if (slotItem != null)
        {
            slotItem.OnClickCallback += OnClickEquipmentSlot;
        }
    }

    private void UnRegisterSlotEvent(EquipmentSlotItem slotItem)
    {
        if (slotItem != null)
        {
            slotItem.OnClickCallback -= OnClickEquipmentSlot;
        }
    }

    private void OnClickEquipmentSlot(EquipmentItemInfo equipmentItemInfo)
    {
        if (_equipmentItemOperateMenu == null)
        {
            return;
        }

        if (equipmentItemInfo == null || equipmentItemInfo.ItemId <= 0)
        {
            _equipmentItemOperateMenu.gameObject.SetActive(false);
            return;
        }

        _equipmentItemOperateMenu.Init(equipmentItemInfo);
    }
}