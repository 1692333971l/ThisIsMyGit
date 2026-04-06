using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//道具配置表管理器
public class ItemConfigManager
{
    private readonly Dictionary<int, ItemConfig> _configDict = new Dictionary<int, ItemConfig>();

    public ItemConfigManager()
    {
        _configDict.Clear();

        TextAsset textAsset = Resources.Load<TextAsset>("Config/Generated/ItemConfig");
        if (textAsset == null)
        {
            Debug.LogError("ItemConfig.json 加载失败，请检查 Resources 路径是否正确。");
            return;
        }

        List<ItemConfig> configs = JsonArrayHelper.FromJsonArray<ItemConfig>(textAsset.text);

        if (configs == null || configs.Count == 0)
        {
            Debug.LogWarning("道具配置为空。");
            return;
        }

        foreach (ItemConfig config in configs)
        {
            _configDict[config.ItemId] = config;
        }

        Debug.Log($"客户端道具配置加载完成，数量：{_configDict.Count}");
    }

    /// <summary>
    /// 根据道具ID获取配置
    /// </summary>
    public ItemConfig GetById(int itemId)
    {
        _configDict.TryGetValue(itemId, out ItemConfig config);
        return config;
    }

    /// <summary>
    /// 获取全部道具配置
    /// </summary>
    public List<ItemConfig> GetAll()
    {
        return _configDict.Values.ToList();
    }

    /// <summary>
    /// 获取全部可装备道具配置
    /// </summary>
    public List<ItemConfig> GetAllEquipItems()
    {
        return _configDict.Values
            .Where(x => x.CanEquip == 1)
            .ToList();
    }

    /// <summary>
    /// 根据装备槽位类型获取可装备道具配置
    /// </summary>
    public List<ItemConfig> GetEquipItemsBySlotType(int equipSlotType)
    {
        return _configDict.Values
            .Where(x => x.CanEquip == 1 && x.EquipSlotType == equipSlotType)
            .ToList();
    }
}