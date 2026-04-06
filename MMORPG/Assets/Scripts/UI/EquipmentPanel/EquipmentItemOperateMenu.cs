using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//装备操作菜单
public class EquipmentItemOperateMenu : MonoBehaviour
{
    [SerializeField] private Button _unequipItemButton;//卸下按钮
    [SerializeField] private Button _cancelButton;//取消按钮
    [SerializeField] private TMP_Text _descriptionText;//描述文本

    private EquipmentItemInfo _equipmentItemInfo;

    public event Action<EquipmentItemInfo> OnUnequipClicked;

    private void Awake()
    {
        if (_unequipItemButton != null)
        {
            _unequipItemButton.onClick.AddListener(OnClickUnequipButton);
        }

        if (_cancelButton != null)
        {
            _cancelButton.onClick.AddListener(OnClickCancelButton);
        }
    }

    public void Init(EquipmentItemInfo equipmentItemInfo)
    {
        _equipmentItemInfo = equipmentItemInfo;

        if (_equipmentItemInfo == null || _equipmentItemInfo.ItemId <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        ItemConfig itemConfig = GameApp.Instance.ItemConfigManager.GetById(_equipmentItemInfo.ItemId);
        if (itemConfig == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text =
                itemConfig.ItemName + "\n\n" +
                itemConfig.Description + "\n\n" +
                BuildPropertyText(itemConfig);
        }

        gameObject.SetActive(true);
    }

    private string BuildPropertyText(ItemConfig itemConfig)
    {
        string result = string.Empty;

        if (itemConfig.AddStrength != 0) result += $"力量 +{itemConfig.AddStrength}\n";
        if (itemConfig.AddAgility != 0) result += $"敏捷 +{itemConfig.AddAgility}\n";
        if (itemConfig.AddIntelligence != 0) result += $"智力 +{itemConfig.AddIntelligence}\n";
        if (itemConfig.AddDefense != 0) result += $"防御 +{itemConfig.AddDefense}\n";
        if (itemConfig.AddMaxHp != 0) result += $"最大HP +{itemConfig.AddMaxHp}\n";
        if (itemConfig.AddMaxMp != 0) result += $"最大MP +{itemConfig.AddMaxMp}\n";
        if (itemConfig.AddCritRate != 0) result += $"暴击率 +{itemConfig.AddCritRate}\n";
        if (itemConfig.AddCritDamage != 0) result += $"暴击伤害 +{itemConfig.AddCritDamage}\n";

        return result;
    }

    private void OnClickUnequipButton()
    {
        if (_equipmentItemInfo == null || _equipmentItemInfo.ItemId <= 0)
        {
            return;
        }

        OnUnequipClicked?.Invoke(_equipmentItemInfo);
        gameObject.SetActive(false);
    }

    private void OnClickCancelButton()
    {
        gameObject.SetActive(false);
    }
}