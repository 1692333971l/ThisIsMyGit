using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//商店道具购买菜单
public class ShopItemOperateMenu : MonoBehaviour
{
    [SerializeField] private Slider _quantity;//购买数量
    [SerializeField] private TMP_Text _quantityText;//购买数量文本
    [SerializeField] private TMP_Text _description;//详细描述
    [SerializeField] private Button _buyButton;//购买按钮

    private ShopItemConfig _shopItemConfig;
    public event Action<int, int, int> OnBuyClicked;

    private void Start()
    {
        _quantity.onValueChanged.AddListener(OnQuantityValueChanged);
        _buyButton.onClick.AddListener(OnBuyButtonClicked);
    }
    public void Init(ShopItemConfig shopItemConfig)
    {
        _quantity.value = 1;
        _quantityText.text = "1";
        _shopItemConfig = shopItemConfig;
        if (_shopItemConfig.IsLimited == 0)
        {
            _quantity.maxValue = 99;
        }
        else
        {
            _quantity.maxValue = _shopItemConfig.LimitCount;
        }
        ItemConfig itemConfig = GameApp.Instance.ItemConfigManager.GetById(_shopItemConfig.ItemId);
        _description.text = itemConfig.ItemName + "\n" + "\n" + itemConfig.Description;
        
    }
    private void OnQuantityValueChanged(float value)
    {
        _quantityText.text = value.ToString();
    }
    private void OnBuyButtonClicked()
    {
        OnBuyClicked?.Invoke(_shopItemConfig.ShopId, _shopItemConfig.ItemId, (int)_quantity.value);
    }
}
