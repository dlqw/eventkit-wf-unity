using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using WFramework.CoreDevKit.LambdaKit;
using WFramework.Unity.CoreDevKit.TemplateKit.Singleton;
using Object = UnityEngine.Object;

namespace WFramework.CoreDevKit.EventKit.Hook
{
    using static WFramework.Unity.CoreDevKit.Core;

    internal enum HookType
    {
        Field,
        Property
    }

    internal struct HookInfo
    {
        public string       ChangeEventName;
        public FieldInfo    FieldInfo;
        public PropertyInfo PropertyInfo;
        public object       LastValue;
        public HookType     HookType;
        public bool         IsValueType;
    }

    public class HookManager : UnitySingleton<HookManager>
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [ShowInInspector] private readonly Dictionary<object, List<HookInfo>> registry = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // ReSharper disable once Unity.IncorrectMethodSignature
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            var hookables = FindObjectsByType<Object>(FindObjectsSortMode.InstanceID)
               .Where(IsHookable);

            foreach (var hookable in hookables) Register(hookable);
        }

        private void Update()
        {
            foreach (var (obj, hookInfos) in registry)
                for (var i = 0; i < hookInfos.Count; i++)
                {
                    var info = hookInfos[i];
                    var newValue = info.HookType == HookType.Field
                        ? info.FieldInfo.GetValue(obj)
                        : info.PropertyInfo.GetValue(obj);

                    if (info.LastValue == null && newValue == null) continue;
                    if (newValue == null) continue;

                    info.LastValue ??= info.IsValueType ? newValue : CraeteDeepCopyByJson(newValue);

                    if (!info.LastValue.Equals(newValue))
                    {
                        EventCenter.PublishEvent(info.ChangeEventName, new[] { newValue });
                        info.LastValue = info.IsValueType ? newValue : CraeteDeepCopyByJson(newValue);
                    }

                    hookInfos[i] = info;
                }
        }

        private void Register(Object obj)
        {
            var fields = obj.GetType()
                            .GetFields(k_BindingFlags)
                            .Where(field => Attribute.IsDefined(field, typeof(HookAttribute)));

            foreach (var field in fields)
            {
                var hook           = field.GetCustomAttribute<HookAttribute>();
                var hookMethodName = hook.MethodName;
                var hookMethod = obj.GetType()
                                    .GetMethod(hookMethodName, k_BindingFlags);
                if (hookMethod == null)
                    throw new Exception($"找不到{obj.GetType().Name}的方法{hookMethodName}");
                if (hookMethod.GetParameters()
                              .Length
                 == 0)
                    throw new Exception($"方法{hookMethodName}必须有参数");
                if (hookMethod.GetParameters()[0].ParameterType != field.FieldType)
                    throw new Exception($"方法{hookMethodName}的参数类型必须与字段{field.Name}的类型一致");
                var hookAction      = hookMethod.ToAction(obj);
                var changeEventName = $"hook: {obj.GetType().Name}.{field.Name} change - {obj.GetInstanceID()}";
                EventCenter.RegisterEvent(changeEventName, hookAction);
                var lastValue = CraeteDeepCopyByJson(field.GetValue(obj));
                var newHookInfo = new HookInfo
                {
                    ChangeEventName = changeEventName,
                    FieldInfo       = field,
                    LastValue       = lastValue,
                    HookType        = HookType.Field,
                    IsValueType     = field.FieldType.IsValueType
                };

                if (registry.ContainsKey(obj))
                {
                    if (registry[obj]
                       .Any(info => info.FieldInfo == field))
                        throw new Exception($"已经存在{obj.GetType().Name}的字段{field.Name}的钩子");
                    registry[obj]
                       .Add(newHookInfo);
                }
                else { registry.Add(obj, new List<HookInfo> { newHookInfo }); }
            }

            var properties = obj.GetType()
                                .GetProperties(k_BindingFlags)
                                .Where(property => Attribute.IsDefined(property, typeof(HookAttribute)));

            foreach (var property in properties)
            {
                var hook           = property.GetCustomAttribute<HookAttribute>();
                var hookMethodName = hook.MethodName;
                var hookMethod = obj.GetType()
                                    .GetMethod(hookMethodName, k_BindingFlags);
                if (hookMethod == null)
                    throw new Exception($"找不到{obj.GetType().Name}的方法{hookMethodName}");
                if (hookMethod.GetParameters()
                              .Length
                 == 0)
                    throw new Exception($"方法{hookMethodName}必须有参数");
                if (hookMethod.GetParameters()[0].ParameterType != property.PropertyType)
                    throw new Exception($"方法{hookMethodName}的参数类型必须与属性{property.Name}的类型一致");
                var hookAction      = hookMethod.ToAction(obj);
                var changeEventName = $"hook: {obj.GetType().Name}.{property.Name} change - {obj.GetInstanceID()}";
                EventCenter.RegisterEvent(changeEventName, hookAction);
                var lastValue = CraeteDeepCopyByJson(property.GetValue(obj));
                var newHookInfo = new HookInfo
                {
                    ChangeEventName = changeEventName,
                    PropertyInfo    = property,
                    LastValue       = lastValue,
                    HookType        = HookType.Property,
                    IsValueType     = property.PropertyType.IsValueType
                };

                if (registry.ContainsKey(obj))
                {
                    if (registry[obj]
                       .Any(info => info.PropertyInfo == property))
                        throw new Exception($"已经存在{obj.GetType().Name}的属性{property.Name}的钩子");
                    registry[obj]
                       .Add(newHookInfo);
                }
                else { registry.Add(obj, new List<HookInfo> { newHookInfo }); }
            }
        }

        private static bool IsHookable(object obj)
        {
            return obj.GetType()
                      .GetMembers(k_BindingFlags)
                      .Any(member => Attribute.IsDefined(member, typeof(HookAttribute)));
        }
    }
}