using System;
using System.Collections.Generic;
using UnityEngine;

//协助读取配置表
public static class JsonArrayHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }

    public static List<T> FromJsonArray<T>(string json)
    {
        string wrappedJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper != null && wrapper.Items != null ? wrapper.Items : new List<T>();
    }
}