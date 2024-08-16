using UnityEngine;
using WFramework.CoreDevKit.EventKit.Attr;

namespace WFramework.CoreDevKit.EventKit
{
    public class UnityAnnotatedEventRegistrar : MonoBehaviour, IEventRegister
    {
        public virtual void Start() { Register(this); }

        public virtual void OnDestroy() { Unregister(this); }

        public void Register(object obj) { EventManager.Instance.RegisterEventHandler(obj); }

        public void Unregister(object obj) { EventManager.Instance.UnregisterEventHandler(obj); }
    }
}