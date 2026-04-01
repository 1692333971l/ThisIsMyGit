using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//道具操作菜单
public class InventoryItemOperateMenu : MonoBehaviour
{
    [SerializeField] private Slider _quantity;//出售数量
    [SerializeField] private TMP_Text _quantityText;//出售数量文本
    [SerializeField] private TMP_Text _description;//详细描述
    [SerializeField] private Button _useButton;//使用按钮
    [SerializeField] private Button _sellButton;//出售按钮

    private InventoryItemInfo _inventoryItemInfo;//格子信息

    public event Action<InventoryItemInfo> OnUseClicked;
    public event Action<int, InventoryItemInfo> OnSellClicked;

    private void Start()
    {
        _quantity.onValueChanged.AddListener(OnCountValueChanged);
        _useButton.onClick.AddListener(OnClickUseButton);
        _sellButton.onClick.AddListener(OnClickSellButton);
    }
    public void Init(InventoryItemInfo inventoryItemInfo)
    {
        _quantity.value = 1;
        _quantityText.text = "1";
        _quantity.maxValue = inventoryItemInfo.Count;

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
    private void OnCountValueChanged(float value)
    {
        _quantityText.text = value.ToString();
    }
    private void OnClickUseButton()
    {
        OnUseClicked?.Invoke(_inventoryItemInfo);
    }
    private void OnClickSellButton()
    {
        OnSellClicked?.Invoke((int)_quantity.value, _inventoryItemInfo);
    }
}
