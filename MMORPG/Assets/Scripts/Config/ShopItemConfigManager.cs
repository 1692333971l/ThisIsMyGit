using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopItemConfigManager
{
    private List<ShopItemConfig> _shopItemConfigList = new List<ShopItemConfig>();
    private Dictionary<int, List<ShopItemConfig>> _shopItemDict = new Dictionary<int, List<ShopItemConfig>>();

    /// <summary>
    /// 加载商店商品配置表
    /// </summary>
    public ShopItemConfigManager()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Config/Generated/ShopItemConfig");

        if (textAsset == null)
        {
            Debug.LogError("ShopItemConfig.json not found in Resources/Config/Generated/");
            return;
        }

        List<ShopItemConfig> configList = JsonArrayHelper.FromJsonArray<ShopItemConfig>(textAsset.text);

        _shopItemConfigList = configList ?? new List<ShopItemConfig>();
        _shopItemDict.Clear();

        foreach (ShopItemConfig config in _shopItemConfigList)
        {
            if (!_shopItemDict.ContainsKey(config.ShopId))
            {
                _shopItemDict[config.ShopId] = new List<ShopItemConfig>();
            }

            _shopItemDict[config.ShopId].Add(config);
        }

        // 每个商店内部按 Sort 排序
        foreach (var pair in _shopItemDict)
        {
            pair.Value.Sort((a, b) => a.Sort.CompareTo(b.Sort));
        }

        Debug.Log($"客户端商店配置加载完成, 数量 = {_shopItemConfigList.Count}");
    }

    /// <summary>
    /// 根据 ShopId 获取该商店的商品列表
    /// </summary>
    public List<ShopItemConfig> GetByShopId(int shopId)
    {
        if (_shopItemDict.TryGetValue(shopId, out List<ShopItemConfig> configList))
        {
            return configList;
        }

        return new List<ShopItemConfig>();
    }

    /// <summary>
    /// 获取全部商店商品配置
    /// </summary>
    public List<ShopItemConfig> GetAll()
    {
        return _shopItemConfigList;
    }
}