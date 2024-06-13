using Framework.MessageModule.EventSystem.Attr;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Framework.MessageModule.EventSystem
{
    public class AnnotatedEventRegistrar : IEventRegister
    {
        protected AnnotatedEventRegistrar()
        {
            Register(this);
        }

        ~AnnotatedEventRegistrar()
        {
            Unregister(this);
        }

        public void Register(object obj)
        {
            EventManager.Instance.RegisterEventHandler(obj);
        }

        public void Unregister(object obj)
        {
            EventManager.Instance.UnregisterEventHandler(obj);
        }
    }
}