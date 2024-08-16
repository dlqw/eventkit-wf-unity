using System;

namespace WFramework.CoreDevKit.EventKit.Hook
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HookAttribute : Attribute
    {
        public readonly string MethodName;

        public HookAttribute(string methodName) { MethodName = methodName; }
    }
}