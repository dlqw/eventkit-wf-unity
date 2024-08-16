using WFramework.CoreDevKit.EventKit.Attr;

namespace WFramework.CoreDevKit.EventKit
{
    public class AnnotatedEventRegistrar : IEventRegister
    {
        protected AnnotatedEventRegistrar() { Register(this); }

        public void Register(object obj) { EventManager.Instance.RegisterEventHandler(obj); }

        public void Unregister(object obj) { EventManager.Instance.UnregisterEventHandler(obj); }

        ~AnnotatedEventRegistrar() { Unregister(this); }
    }
}