using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemOperateMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _description;//详细描述
    [SerializeField] private Button _useButton;//使用按钮
    [SerializeField] private Button _sellButton;//出售按钮

    private InventoryItemInfo _inventoryItemInfo;//格子信息

    public event Action<InventoryItemInfo> _onClickUseButton;
    public event Action<int, InventoryItemInfo> _onSellUseButton;

    private void Start()
    {
        _useButton.onClick.AddListener(OnClickUseButton);
        _sellButton.onClick.AddListener(OnSellUseButton);
    }
    public void Init(InventoryItemInfo inventoryItemInfo)
    {
        ItemConfig itemConfig = GameApp.Instance.ItemConfigManager.GetById(inventoryItemInfo.ItemId);
        if (itemConfig.CanUse == 1)
        {
            _useButton.interactable = true;
        }
        else
        {
            _useButton.interactable = false;
        }
        _description.text = itemConfig.ItemName + "\n" + "\n" + itemConfig.Description;

        _inventoryItemInfo = inventoryItemInfo;
    }
    private void OnClickUseButton()
    {
        _onClickUseButton?.Invoke(_inventoryItemInfo);
    }
    private void OnSellUseButton()
    {
        _onSellUseButton?.Invoke(1, _inventoryItemInfo);
    }
}
