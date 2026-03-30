using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//背包格子脚本
public class InventorySlotItem : MonoBehaviour
{
    [SerializeField] private Image _icon;//图标
    [SerializeField] private TMP_Text _name;//道具名称
    [SerializeField] private TMP_Text _count;//数量

    private Sprite _defaultSprite;
    private InventoryItemInfo _inventoryItemInfo;//格子信息

    public event Action<InventoryItemInfo> _onClickCallback;

    private void Awake()
    {
        _defaultSprite = _icon.sprite;
    }
    private void Start()
    {
       gameObject.GetComponent<Button>().onClick.AddListener(OnClickInventorySlotItem);
    }
    public void Init(InventoryItemInfo inventoryItemInfo)
    {
        ItemConfig itemConfig = GameApp.Instance.ItemConfigManager.GetById(inventoryItemInfo.ItemId);
        Sprite iconSprite = Resources.Load<Sprite>(itemConfig.IconPath);
        _icon.sprite = iconSprite;
        _name.text = itemConfig.ItemName;
        _count.text = "" + inventoryItemInfo.Count;

        _inventoryItemInfo = inventoryItemInfo;
    }
    public void InitEmpty(int slotIndex)
    {
        _inventoryItemInfo = null;
        _icon.sprite = _defaultSprite;
        _name.text = "";
        _count.text = "";
    }
    private void OnClickInventorySlotItem()
    {
        _onClickCallback?.Invoke(_inventoryItemInfo);
    }
}
