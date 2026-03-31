using Protocol;
using System.Collections.Generic;
using UnityEngine;

//背包面板
public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private Transform _inventoryPanelRoot;//背包格子根节点
    [SerializeField] private InventorySlotItem _inventorySlotItem;//背包格子
    [SerializeField] private InventoryItemOperateMenu _inventoryItemOperateMenu;//道具操作菜单

    private int _maxSlotCount = 72;

    private void Awake()
    {
        //清空背包列表
        for (int i = _inventoryPanelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_inventoryPanelRoot.GetChild(i).gameObject);
        }
        //生成列表项
        for (int i = 0; i < _maxSlotCount; i++)
        {
            InventorySlotItem item = Instantiate(_inventorySlotItem, _inventoryPanelRoot);
        }
    }
    private void OnEnable()
    {
        Init();
        GameApp.Instance.InventoryService.OnUseItemResponse += HandleUseItemResponse;
        _inventoryItemOperateMenu._onClickUseButton += OnClickUseButton;
        _inventoryItemOperateMenu._onSellUseButton += OnClickSellButton;
    }
    private void OnDisable()
    {
        GameApp.Instance.InventoryService.OnUseItemResponse -= HandleUseItemResponse;
        _inventoryItemOperateMenu._onClickUseButton -= OnClickUseButton;
        _inventoryItemOperateMenu._onSellUseButton -= OnClickSellButton;
    }
    //初始化数据
    private void Init()
    {
        Dictionary<int, InventoryItemInfo> inventoryDict = GameApp.Instance.PlayerInventoryManager.GetPlayerInventoryDict();
        //刷新数据
        for (int i = 0; i < _maxSlotCount; i++)
        {
            InventoryItemInfo inventoryItemInfo = GameApp.Instance.PlayerInventoryManager.GetPlayerInventoryBySlotIndex(i);
            InventorySlotItem item = _inventoryPanelRoot.GetChild(i).gameObject.GetComponent<InventorySlotItem>();

            item._onClickCallback -= OnClickInventorySlotItem;
            item._onClickCallback += OnClickInventorySlotItem;
            if (inventoryItemInfo != null)
            {
                item.Init(inventoryItemInfo);
            }
            else
            {
                item.InitEmpty(i);
            }
        }
    }
    //格子点击事件
    private void OnClickInventorySlotItem(InventoryItemInfo inventoryItemInfo)
    {
        if (inventoryItemInfo == null)
        {
            _inventoryItemOperateMenu.gameObject.SetActive(false);
            return;
        }
        _inventoryItemOperateMenu.gameObject.SetActive(true);
        _inventoryItemOperateMenu.Init(inventoryItemInfo);
    }
    //使用按钮点击事件
    private void OnClickUseButton(InventoryItemInfo inventoryItemInfo)
    {
        GameApp.Instance.InventoryService.SendUseItemRequest(inventoryItemInfo.SlotIndex);
    }
    //使用物品响应事件
    private void HandleUseItemResponse(UseItemResponse useItemResponse)
    {
        Init();
    }
    //出售按钮点击事件
    private void OnClickSellButton(int count, InventoryItemInfo inventoryItemInfo)
    {
        Init();
    }
}
