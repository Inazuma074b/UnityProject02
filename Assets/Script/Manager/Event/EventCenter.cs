using System;
using System.Collections.Generic;

public class EventCenter<TEventKey, TEventResult> where TEventKey:struct, IConvertible // 限制使用 enum
{
    private static Dictionary<TEventKey, Action<TEventResult>> eventContainer = new Dictionary<TEventKey, Action<TEventResult>>();

    public static void Init()
    {
        if (null == eventContainer)
        {
            eventContainer = new Dictionary<TEventKey, Action<TEventResult>>();
        }
    }

    public static void AddListener(TEventKey key, Action<TEventResult> notified)
    {
        if (!eventContainer.ContainsKey(key))
        {
            eventContainer.Add(key, notified);
        }

        eventContainer[key] -= notified;
        eventContainer[key] += notified;
    }

    public static void RemoveListener(TEventKey key, Action<TEventResult> notified)
    {
        if (eventContainer.ContainsKey(key))
        {
            eventContainer[key] -= notified;
        }
    }

    public static void Clear()
    {
        List<TEventKey> removeKeys = new List<TEventKey>(eventContainer.Keys);

        int count = removeKeys.Count;
        for (int i = 0; i < count; i++)
        {
            RemoveListener(removeKeys[i], eventContainer[removeKeys[i]]);
        }

        eventContainer.Clear();
    }

    public static void Notify(TEventKey key, TEventResult data)
    {
        if (eventContainer.ContainsKey(key) && eventContainer[key] != null)
        {
            eventContainer[key].Invoke(data);
        }
    }
}
