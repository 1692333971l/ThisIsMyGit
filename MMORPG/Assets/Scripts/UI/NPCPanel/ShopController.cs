using Protocol;
using UnityEngine;

//商店控制器
public class ShopController : MonoBehaviour
{
    [SerializeField] private ShopPanel _shopPanel;//商店面板
    [SerializeField] private ShopItemOperateMenu _shopItemOperateMenu;//商店道具购买菜单

    private void Awake()
    {
        GameApp.Instance.ShopService.OnBuyShopItemResponse += HandleGetShopItemsResponse;
        _shopItemOperateMenu.OnBuyClicked += OnBuyClicked;
    }
    private void OnDestroy()
    {
        GameApp.Instance.ShopService.OnBuyShopItemResponse -= HandleGetShopItemsResponse;
        _shopItemOperateMenu.OnBuyClicked -= OnBuyClicked;
    }
    private void HandleGetShopItemsResponse(BuyShopItemResponse shopItemsResponse)
    {
        if ((ErrorCode)shopItemsResponse.ErrorCode != ErrorCode.Success)
        {
            MessageHintWindowManger.Instance.ShowMessage("购买失败，" + shopItemsResponse.Message);
            return;
        }
        _shopPanel.Show(shopItemsResponse.ShopId, shopItemsResponse.CharacterInfo);
        MessageHintWindowManger.Instance.ShowMessage("购买成功");
    }
    private void OnBuyClicked(int shopId, int itemId, int quantity)
    {
        GameApp.Instance.ShopService.SendBuyShopItemRequest(shopId, itemId, quantity);
    }
}
