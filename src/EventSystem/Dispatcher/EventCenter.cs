using System;
using System.Collections.Generic;
using Framework.Utility.ClassTemplate.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.MessageModule.EventSystem
{
    public class EventCenter : EagerSingleton<EventCenter>
    {
        [ShowInInspector] private readonly Dictionary<string, List<Action>> eventDict = new(32);
        private readonly List<string> eventCacheDict = new(16);

        private readonly Dictionary<string, Action<object[]>[]> parametricEventDict = new(32);

        #region 注册事件

        public void RegisterEvent(string eventName, Action action)
        {
            if (!eventDict.ContainsKey(eventName))
            {
                eventDict[eventName] = new List<Action>();
            }

            eventDict[eventName].Add(action);
        }

        public void RegisterEvent(string eventName, Action<object[]> action)
        {
            if (!parametricEventDict.ContainsKey(eventName))
            {
                parametricEventDict[eventName] = new Action<object[]>[32];
            }

            var value = parametricEventDict[eventName];
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == null)
                {
                    value[i] = action;
                    break;
                }
            }
        }

        #endregion

        #region 取消注册事件

        public void UnregisterEvent(string eventName, Action action)
        {
            if (!eventDict.TryGetValue(eventName, out var value))
            {
                return;
            }

            value.Remove(action);
        }
        
        public void UnregisterEvent(string eventName, Action<object[]> action)
        {
            if (!parametricEventDict.TryGetValue(eventName, out var value))
            {
                return;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == action)
                {
                    value[i] = null;
                    break;
                }
            }
        }

        public void UnregisterEvent(string eventName)
        {
            if (!eventDict.TryGetValue(eventName, out var value))
            {
                return;
            }

            value.Clear();
        }

        public void UnregisterAllEvent()
        {
            eventDict.Clear();
        }

        #endregion

        #region 分发事件

        public void PublishEvent(string eventName, bool addToCache = false)
        {
            if (addToCache)
            {
                eventCacheDict.Add(eventName);
                return;
            }

            if (!eventDict.TryGetValue(eventName, out var value))
            {
                return;
            }

            foreach (var action in value)
            {
                action();
            }
        }
        
        public void PublishEvent(string eventName, object[] args, bool addToCache = false)
        {
            if (addToCache)
            {
                eventCacheDict.Add(eventName);
                return;
            }

            if (!parametricEventDict.TryGetValue(eventName, out var value))
            {
                return;
            }

            foreach (var action in value)
            {
                action?.Invoke(args);
            }
        }

        #endregion

        #region 延迟分发事件

        public void PublishCache(string eventName)
        {
            if (!eventCacheDict.Contains(eventName))
            {
                return;
            }

            PublishEvent(eventName);
            eventCacheDict.Remove(eventName);
        }

        #endregion

        #region 生命周期

        // public EventCenter()
        // {
        //     InspectorManager.Instance.RegisterInspector("EventCenter", this);
        // }
        //
        // ~EventCenter()
        // {
        //     InspectorManager.Instance.UnRegisterInspector("EventCenter");
        // }

        #endregion
    }
}