using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Framework.Utility.ClassTemplate.Singleton;
using UnityEngine;

namespace Framework.MessageModule.EventSystem.Attr
{
    public class EventManager : EagerSingleton<EventManager>
    {
        private readonly Dictionary<string, List<Action>> actionDict = new Dictionary<string, List<Action>>();

        private readonly Dictionary<string, List<Action<object[]>>> parametricActionDict =
            new Dictionary<string, List<Action<object[]>>>();


        private void HandleEventOnRegister(object obj)
        {
            var classType = obj.GetType();
            var methods = classType.GetMethods();
            foreach (var method in methods)
            {
                var eventAttributes = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (eventAttributes == null)
                {
                    continue;
                }

                var eventAttribute = (EventAttribute)eventAttributes;
                var parms = method.GetParameters();
                foreach (var info in parms)
                {
                    Debug.Log($"存在参数{info.Name}，类型为{info.ParameterType}");
                }

                if (parms.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (!actionDict.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        actionDict[eventAttribute.EventName] = new List<Action>();
                    }
                    else
                    {
                        value.Add(action);
                    }

                    var eventName = eventAttribute.EventName;

                    EventCenter.Instance.RegisterEvent(eventName, action);
                }
                else
                {
                    var action = CreateDelegate(method, obj);

                    if (parametricActionDict.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        value.Add(action);
                    }
                    else
                    {
                        parametricActionDict[eventAttribute.EventName] = new List<Action<object[]>> { action };
                    }

                    EventCenter.Instance.RegisterEvent(eventAttribute.EventName, action);
                }
            }
        }


        private void HandleEventOnUnregister(object obj)
        {
            var classType = obj.GetType();
            var methods = classType.GetMethods();
            foreach (var method in methods)
            {
                var eventAttributes = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (eventAttributes == null)
                {
                    continue;
                }

                var eventAttribute = (EventAttribute)eventAttributes;
                var parms = method.GetParameters();
                if (parms.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (actionDict.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        value.Remove(action);
                    }

                    var eventName = eventAttribute.EventName;

                    EventCenter.Instance.UnregisterEvent(eventName, action);
                }
                else
                {
                    var action = CreateDelegate(method, obj);

                    if (parametricActionDict.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        value.Remove(action);
                    }

                    EventCenter.Instance.UnregisterEvent(eventAttribute.EventName, action);
                }
            }
        }

        public static Action<object[]> CreateDelegate(MethodInfo methodInfo, object target)
        {
            // 参数: object[]
            var parameter = Expression.Parameter(typeof(object[]), "args");

            // 创建方法参数表达式
            var methodParameters = methodInfo.GetParameters();
            var arguments = new Expression[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = methodParameters[i].ParameterType;
                var parameterAccessor = Expression.ArrayIndex(parameter, index);
                var parameterCast = Expression.Convert(parameterAccessor, parameterType);
                arguments[i] = parameterCast;
            }

            // 创建实例表达式
            var instance = Expression.Constant(target);

            // 创建方法调用表达式
            var methodCall = Expression.Call(instance, methodInfo, arguments);

            // 创建并编译 lambda 表达式
            var lambda = Expression.Lambda<Action<object[]>>(methodCall, parameter);
            return lambda.Compile();
        }


        #region 处理签入的事件类

        public void RegisterEventHandler(object obj)
        {
            // eventHandlerList.Add(obj);
            HandleEventOnRegister(obj);
        }

        #endregion

        #region 处理签出的事件类

        public void UnregisterEventHandler(object obj)
        {
            // eventHandlerList.Remove(obj);
            HandleEventOnUnregister(obj);
        }

        #endregion

        #region 声明周期

        ~EventManager()
        {
            EventCenter.Instance.UnregisterAllEvent();
        }

        #endregion
    }
}