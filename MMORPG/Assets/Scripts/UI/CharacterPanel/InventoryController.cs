using Protocol;
using UnityEngine;

//背包控制器
public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryPanel _inventoryPanel;//背包面板
    [SerializeField] private InventoryItemOperateMenu _inventoryItemOperateMenu;//道具操作菜单

    private void Awake()
    {
        GameApp.Instance.InventoryService.OnGetInventoryResponse += HandleGetInventoryResponse;
        GameApp.Instance.InventoryService.OnUseItemResponse += HandleUseItemResponse;
        GameApp.Instance.InventoryService.OnSellItemResponse += HandlSellItemResponse;

        _inventoryItemOperateMenu.OnUseClicked += OnClickUseButton;
        _inventoryItemOperateMenu.OnSellClicked += OnClickSellButton;
    }
    private void OnDestroy()
    {
        GameApp.Instance.InventoryService.OnGetInventoryResponse -= HandleGetInventoryResponse;
        GameApp.Instance.InventoryService.OnUseItemResponse -= HandleUseItemResponse;
        GameApp.Instance.InventoryService.OnSellItemResponse -= HandlSellItemResponse;

        _inventoryItemOperateMenu.OnUseClicked -= OnClickUseButton;
        _inventoryItemOperateMenu.OnSellClicked -= OnClickSellButton;
    }
    //获取背包响应事件
    private void HandleGetInventoryResponse(GetInventoryResponse getInventoryResponse)
    {
        if ((ErrorCode)getInventoryResponse.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("获取背包失败，" + getInventoryResponse.Message);
            return;
        }
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
        if ((ErrorCode)useItemResponse.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("使用物品失败，" + useItemResponse.Message);
            return;
        }
        _inventoryPanel.Init();
        MessageHintWindowManger.Instance.ShowMessage("使用成功");
    }
    //出售按钮点击事件
    private void OnClickSellButton(int quantity, InventoryItemInfo inventoryItemInfo)
    {
        GameApp.Instance.InventoryService.SendSellItemRequest(quantity, inventoryItemInfo.SlotIndex);
    }
    //出售物品响应事件
    private void HandlSellItemResponse(SellItemResponse sellItemResponse)
    {
        if ((ErrorCode)sellItemResponse.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("出售物品失败，" + sellItemResponse.Message);
            return;
        }
        _inventoryPanel.Init();
        MessageHintWindowManger.Instance.ShowMessage("出售成功");
    }
}
