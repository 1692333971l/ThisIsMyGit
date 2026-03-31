using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//道具操作菜单
public class InventoryItemOperateMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _description;//详细描述
    [SerializeField] private Button _useButton;//使用按钮
    [SerializeField] private Button _sellButton;//出售按钮

    private InventoryItemInfo _inventoryItemInfo;//格子信息

    public event Action<InventoryItemInfo> OnUseClicked;
    public event Action<int, InventoryItemInfo> OnSellClicked;

    private void Start()
    {
        _useButton.onClick.AddListener(OnClickUseButton);
        _sellButton.onClick.AddListener(OnClickSellButton);
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
        OnUseClicked?.Invoke(_inventoryItemInfo);
    }
    private void OnClickSellButton()
    {
        OnSellClicked?.Invoke(1, _inventoryItemInfo);
    }
}
