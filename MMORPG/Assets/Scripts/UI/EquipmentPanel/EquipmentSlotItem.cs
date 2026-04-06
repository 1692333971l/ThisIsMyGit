using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//装备格子项
public class EquipmentSlotItem : MonoBehaviour
{
    [SerializeField] private Image _icon;//装备图标
    [SerializeField] private TMP_Text _slotNameText;//槽位名称文本
    [SerializeField] private Button _button;//点击按钮

    private EquipmentItemInfo _equipmentItemInfo;

    public event Action<EquipmentItemInfo> OnClickCallback;

    private void Awake()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }
    }

    public void Init(int equipSlotType, EquipmentItemInfo equipmentItemInfo)
    {
        _equipmentItemInfo = equipmentItemInfo;

        if (_slotNameText != null)
        {
            _slotNameText.text = GetSlotName(equipSlotType);
        }

        if (_icon == null)
        {
            return;
        }

        Color color = _icon.color;
        color.a = 0.5f; // 半透明
        // 没有装备
        if (_equipmentItemInfo == null || _equipmentItemInfo.ItemId <= 0)
        {
            _icon.sprite = null;
            _icon.color = color;
            return;
        }

        ItemConfig itemConfig = GameApp.Instance.ItemConfigManager.GetById(_equipmentItemInfo.ItemId);
        if (itemConfig == null)
        {
            _icon.sprite = null;
            _icon.color = color;
            return;
        }

        Sprite iconSprite = Resources.Load<Sprite>(itemConfig.IconPath);
        _icon.sprite = iconSprite;
        _icon.color = Color.white;
    }

    private void OnClick()
    {
        OnClickCallback?.Invoke(_equipmentItemInfo);
    }

    private string GetSlotName(int equipSlotType)
    {
        switch (equipSlotType)
        {
            case 1: return "武器";
            case 2: return "头部";
            case 3: return "衣服";
            case 4: return "手部";
            case 5: return "腿部";
            case 6: return "饰品";
            default: return "未知";
        }
    }
}