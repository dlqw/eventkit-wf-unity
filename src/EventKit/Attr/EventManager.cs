using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WFramework.CoreDevKit.LambdaKit;
using WFramework.CoreDevKit.TemplateKit.Singleton;

namespace WFramework.CoreDevKit.EventKit.Attr
{
    public class EventManager : EagerSingleton<EventManager>
    {
        private readonly Dictionary<string, List<Action>> actionDict = new();

        private readonly Dictionary<string, List<Action<object[]>>> parametricActionDict = new();


        private void HandleEventOnRegister(object obj)
        {
            var classType = obj.GetType();
            var methods   = classType.GetMethods();
            foreach (var method in methods)
            {
                var eventAttributes = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (eventAttributes == null) continue;

                var eventAttribute = (EventAttribute)eventAttributes;
                var parms          = method.GetParameters();
                foreach (var info in parms) Debug.Log($"存在参数{info.Name}，类型为{info.ParameterType}");

                if (parms.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (!actionDict.TryGetValue(eventAttribute.EventName, out var value))
                        actionDict[eventAttribute.EventName] = new List<Action>();
                    else
                        value.Add(action);

                    var eventName = eventAttribute.EventName;

                    EventCenter.RegisterEvent(eventName, action);
                }
                else
                {
                    var action = method.ToAction(obj);

                    if (parametricActionDict.TryGetValue(eventAttribute.EventName, out var value))
                        value.Add(action);
                    else
                        parametricActionDict[eventAttribute.EventName] = new List<Action<object[]>> { action };

                    EventCenter.RegisterEvent(eventAttribute.EventName, action);
                }
            }
        }


        private void HandleEventOnUnregister(object obj)
        {
            var classType = obj.GetType();
            var methods   = classType.GetMethods();
            foreach (var method in methods)
            {
                var eventAttributes = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (eventAttributes == null) continue;

                var eventAttribute = (EventAttribute)eventAttributes;
                var parms          = method.GetParameters();
                if (parms.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (actionDict.TryGetValue(eventAttribute.EventName, out var value)) value.Remove(action);

                    var eventName = eventAttribute.EventName;

                    EventCenter.UnregisterEvent(eventName, action);
                }
                else
                {
                    var action = method.ToAction(obj);

                    if (parametricActionDict.TryGetValue(eventAttribute.EventName, out var value)) value.Remove(action);

                    EventCenter.UnregisterEvent(eventAttribute.EventName, action);
                }
            }
        }


        #region 处理签入的事件类

        public void RegisterEventHandler(object obj) { HandleEventOnRegister(obj); }

        #endregion

        #region 处理签出的事件类

        public void UnregisterEventHandler(object obj) { HandleEventOnUnregister(obj); }

        #endregion

        #region 声明周期

        ~EventManager() { EventCenter.UnregisterAllEvent(); }

        #endregion
    }
}