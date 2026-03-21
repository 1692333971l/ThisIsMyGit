using System;
using UnityEngine;

//协助读取配置表
public static class JsonArrayHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    /// <summary>
    /// 把顶层数组 JSON 包装后再交给 JsonUtility 解析
    /// </summary>
    public static T[] FromJson<T>(string json)
    {
        string wrappedJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.Items;
    }
}