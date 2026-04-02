using System.Collections.Generic;
using TMPro;
using UnityEngine;

//商店面板
public class ShopPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _gold;//当前金币
    [SerializeField] private Transform _shopRoot;//商店格子根节点
    [SerializeField] private ShopSlotItem _shopSlotItem;//商店物品格子
    [SerializeField] private ShopItemOperateMenu _shopItemOperateMenu;//道具操作菜单

    public void Show(int shopId, Protocol.CharacterInfo characterInfo)
    {
        _gold.text = $"金币：{characterInfo.Gold}";
        List<ShopItemConfig> items =  GameApp.Instance.ShopItemConfigManager.GetByShopId(shopId);
        if (items == null || items.Count == 0)
        {
            return;
        }
        //清空背包列表
        for (int i = _shopRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_shopRoot.GetChild(i).gameObject);
        }
        foreach (ShopItemConfig shopItemConfig in items)
        {
            ShopSlotItem shopSlotItem = Instantiate(_shopSlotItem, _shopRoot);
            shopSlotItem.Init(shopItemConfig);
            shopSlotItem.OnClickCallback -= OnClickShopSlotItem;
            shopSlotItem.OnClickCallback += OnClickShopSlotItem;
        }
        gameObject.SetActive(true);
        OnClickShopSlotItem(null);
    }
    private void OnClickShopSlotItem(ShopItemConfig shopItemConfig)
    {
        if (shopItemConfig == null)
        {
            _shopItemOperateMenu.gameObject.SetActive(false);
            return;
        }
        _shopItemOperateMenu.Init(shopItemConfig);
        _shopItemOperateMenu.gameObject.SetActive(true);
    }
}
