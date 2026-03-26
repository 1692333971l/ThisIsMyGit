using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//背包格子脚本
public class InventorySlotItem : MonoBehaviour
{
    [SerializeField] private int _slotIndex;//格子id
    [SerializeField] private Image _icon;//图标
    [SerializeField] private TMP_Text _name;//道具名称
    [SerializeField] private TMP_Text _count;//数量

    public void Init(InventoryItemInfo inventoryItemInfo)
    {
        _slotIndex = inventoryItemInfo.SlotIndex;
        ItemConfig itemConfig =  GameApp.Instance.ItemConfigManager.GetById(inventoryItemInfo.ItemId);
        Sprite iconSprite = Resources.Load<Sprite>(itemConfig.IconPath);
        _icon.sprite = iconSprite;
        _name.text = itemConfig.ItemName;
        _count.text = "" + inventoryItemInfo.Count;
    }

    public void InitEmpty(int slotIndex)
    {
        _slotIndex = slotIndex;
        _name.text = "";
        _count.text = "";
    }
}
