using System;

namespace WFramework.CoreDevKit.EventKit.Attr
{
    /// <summary>
    ///     标记事件的提供者
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public EventAttribute(string eventName) { EventName = eventName; }

        public string EventName { get; }
    }
}