using Protocol;
using UnityEngine;

//背包控制器
public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryPanel _inventoryPanel;//背包格子
    [SerializeField] private InventoryItemOperateMenu _inventoryItemOperateMenu;//道具操作菜单

    private void Awake()
    {
        GameApp.Instance.InventoryService.OnGetInventoryResponse += HandleGetInventoryResponse;
        GameApp.Instance.InventoryService.OnUseItemResponse += HandleUseItemResponse;
        _inventoryItemOperateMenu.OnUseClicked += OnClickUseButton;
        _inventoryItemOperateMenu.OnSellClicked += OnClickSellButton;
    }
    private void OnDestroy()
    {
        GameApp.Instance.InventoryService.OnGetInventoryResponse -= HandleGetInventoryResponse;
        GameApp.Instance.InventoryService.OnUseItemResponse -= HandleUseItemResponse;
        _inventoryItemOperateMenu.OnUseClicked -= OnClickUseButton;
        _inventoryItemOperateMenu.OnSellClicked -= OnClickSellButton;
    }
    //获取背包响应事件
    private void HandleGetInventoryResponse()
    {
        _inventoryPanel.Init();
    }
    //使用按钮点击事件
    private void OnClickUseButton(InventoryItemInfo inventoryItemInfo)
    {
        GameApp.Instance.InventoryService.SendUseItemRequest(inventoryItemInfo.SlotIndex);
    }
    //使用物品响应事件
    private void HandleUseItemResponse(UseItemResponse useItemResponse)
    {
        _inventoryPanel.Init();
    }
    //出售按钮点击事件
    private void OnClickSellButton(int count, InventoryItemInfo inventoryItemInfo)
    {

    }
    //出售物品响应事件
    private void HandlSellItemResponse(UseItemResponse useItemResponse)
    {
        _inventoryPanel.Init();
    }
}
