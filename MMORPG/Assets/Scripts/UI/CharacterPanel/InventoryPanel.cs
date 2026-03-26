using Protocol;
using System.Collections.Generic;
using UnityEngine;

//背包面板
public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private Transform _inventoryPanelRoot;//背包格子根节点
    [SerializeField] private InventorySlotItem _inventorySlotItem;//背包格子

    private int _maxSlotCount = 72;

    public void OnEnable()
    {
        //清空背包列表
        for (int i = _inventoryPanelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_inventoryPanelRoot.GetChild(i).gameObject);
        }

        Dictionary<int, InventoryItemInfo> inventoryDict = GameApp.Instance.PlayerInventoryManager.GetPlayerInventoryDict();
        if (inventoryDict.Count == 0)
        {
            return;
        }
        //生成列表项
        for (int i = 0; i < _maxSlotCount; i++)
        {
            InventorySlotItem item = Instantiate(_inventorySlotItem, _inventoryPanelRoot);

            InventoryItemInfo inventoryItemInfo = GameApp.Instance.PlayerInventoryManager.GetPlayerInventoryBySlotIndex(i);

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
}
