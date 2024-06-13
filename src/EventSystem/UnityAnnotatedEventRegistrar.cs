using Framework.MessageModule.EventSystem.Attr;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace Framework.MessageModule.EventSystem
{
    public class UnityAnnotatedEventRegistrar : MonoBehaviour, IEventRegister
    {
        public void Register(object obj)
        {
            EventManager.Instance.RegisterEventHandler(obj);
        }

        public void Unregister(object obj)
        {
            EventManager.Instance.UnregisterEventHandler(obj);
        }

        public void Start()
        {
            Register(this);
        }

        public void OnDestroy()
        {
            Unregister(this);
        }
    }
}