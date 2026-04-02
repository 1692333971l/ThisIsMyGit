using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcConfigManager
{
    private Dictionary<int, NpcConfig> _npcConfigDict = new Dictionary<int, NpcConfig>();

    /// <summary>
    /// 加载 NPC 配置表
    /// </summary>
    public NpcConfigManager()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Config/Generated/NpcConfig");

        if (textAsset == null)
        {
            Debug.LogError("NpcConfig.json not found in Resources/Config/Generated/");
            return;
        }

        List<NpcConfig> configList = JsonArrayHelper.FromJsonArray<NpcConfig>(textAsset.text);

        _npcConfigDict.Clear();

        foreach (NpcConfig config in configList)
        {
            _npcConfigDict[config.NpcId] = config;
        }

        Debug.Log($"客户端NPC配置加载完成, 数量 = {_npcConfigDict.Count}");
    }

    /// <summary>
    /// 根据 NpcId 获取配置
    /// </summary>
    public NpcConfig GetById(int npcId)
    {
        _npcConfigDict.TryGetValue(npcId, out NpcConfig config);
        return config;
    }

    /// <summary>
    /// 获取全部 NPC 配置
    /// </summary>
    public List<NpcConfig> GetAll()
    {
        return _npcConfigDict.Values.ToList();
    }
}