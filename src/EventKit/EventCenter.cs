using System;
using System.Collections.Generic;

namespace WFramework.CoreDevKit.EventKit
{
    public class EventCenter
    {
        private static readonly Dictionary<string, List<Action>>       EventDict           = new(32);
        private static readonly Dictionary<string, Action<object[]>[]> ParametricEventDict = new(32);
        private static readonly List<string>                           EventCacheDict      = new(16);

        private EventCenter() { }

        #region 延迟分发事件

        public static void PublishCache(string eventName)
        {
            if (!EventCacheDict.Contains(eventName)) return;

            PublishEvent(eventName);
            EventCacheDict.Remove(eventName);
        }

        #endregion

        #region 注册事件

        public static void RegisterEvent(string eventName, Action action)
        {
            if (!EventDict.ContainsKey(eventName)) EventDict[eventName] = new List<Action>();

            EventDict[eventName]
               .Add(action);
        }

        public static void RegisterEvent(string eventName, Action<object[]> action)
        {
            if (!ParametricEventDict.ContainsKey(eventName)) ParametricEventDict[eventName] = new Action<object[]>[32];

            var value = ParametricEventDict[eventName];
            for (var i = 0; i < value.Length; i++)
                if (value[i] == null)
                {
                    value[i] = action;
                    break;
                }
        }

        #endregion

        #region 取消注册事件

        public static void UnregisterEvent(string eventName, Action action)
        {
            if (!EventDict.TryGetValue(eventName, out var value)) return;

            value.Remove(action);
        }

        public static void UnregisterEvent(string eventName, Action<object[]> action)
        {
            if (!ParametricEventDict.TryGetValue(eventName, out var value)) return;

            for (var i = 0; i < value.Length; i++)
                if (value[i] == action)
                {
                    value[i] = null;
                    break;
                }
        }

        public static void UnregisterEvent(string eventName)
        {
            if (!EventDict.TryGetValue(eventName, out var value)) return;

            value.Clear();
        }

        public static void UnregisterAllEvent() { EventDict.Clear(); }

        #endregion

        #region 分发事件

        public static void PublishEvent(string eventName, bool addToCache = false)
        {
            if (addToCache)
            {
                EventCacheDict.Add(eventName);
                return;
            }

            if (!EventDict.TryGetValue(eventName, out var value)) return;

            foreach (var action in value) action();
        }

        public static void PublishEvent(string eventName, object[] args, bool addToCache = false)
        {
            if (addToCache)
            {
                EventCacheDict.Add(eventName);
                return;
            }

            if (!ParametricEventDict.TryGetValue(eventName, out var value)) return;

            foreach (var action in value) action?.Invoke(args);
        }

        #endregion
    }
}