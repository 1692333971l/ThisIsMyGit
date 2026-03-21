using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//职业配置表管理器
public class ProfessionConfigManager
{
    private readonly Dictionary<int, ProfessionConfig> _configDict = new Dictionary<int, ProfessionConfig>();

    /// <summary>
    /// 加载职业配置
    /// </summary>
    public ProfessionConfigManager()
    {
        _configDict.Clear();

        TextAsset textAsset = Resources.Load<TextAsset>("Config/Generated/ProfessionConfig");
        if (textAsset == null)
        {
            Debug.LogError("ProfessionConfig.json 加载失败，请检查 Resources 路径是否正确。");
            return;
        }

        ProfessionConfig[] configs = JsonArrayHelper.FromJson<ProfessionConfig>(textAsset.text);

        if (configs == null || configs.Length == 0)
        {
            Debug.LogWarning("职业配置为空。");
            return;
        }

        foreach (ProfessionConfig config in configs)
        {
            _configDict[config.ProfessionId] = config;
        }

        Debug.Log($"客户端职业配置加载完成，数量：{_configDict.Count}");
    }

    /// <summary>
    /// 根据职业ID获取配置
    /// </summary>
    public ProfessionConfig GetById(int professionId)
    {
        _configDict.TryGetValue(professionId, out ProfessionConfig config);
        return config;
    }

    /// <summary>
    /// 获取全部职业配置
    /// </summary>
    public List<ProfessionConfig> GetAll()
    {
        return _configDict.Values.ToList();
    }
}