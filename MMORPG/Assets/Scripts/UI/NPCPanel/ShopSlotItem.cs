using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotItem : MonoBehaviour
{
    [SerializeField] private Image _icon;//图标
    [SerializeField] private TMP_Text _name;//道具名称
    [SerializeField] private TMP_Text _Price;//价格
    [SerializeField] private TMP_Text _count;//剩余数量

    private ShopItemConfig _shopItemConfig;
    public event Action<ShopItemConfig> OnClickCallback;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickShopSlotItem);
    }
    public void Init(ShopItemConfig shopItemConfig)
    {
        _shopItemConfig = shopItemConfig;
        ItemConfig itemConfig =  GameApp.Instance.ItemConfigManager.GetById(_shopItemConfig.ItemId);
        Sprite iconSprite = Resources.Load<Sprite>(itemConfig.IconPath);
        _icon.sprite = iconSprite;
        _name.text = itemConfig.ItemName;
        _Price.text = _shopItemConfig.Price.ToString();
        if (_shopItemConfig.IsLimited != 0)
        {
            _count.text = _shopItemConfig.LimitCount.ToString();
        }
    }
    private void OnClickShopSlotItem()
    {
        OnClickCallback?.Invoke(_shopItemConfig);
    }
}
