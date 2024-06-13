using System;

namespace Framework.MessageModule.EventSystem.Attr
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public string EventName { get; }

        public EventAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}