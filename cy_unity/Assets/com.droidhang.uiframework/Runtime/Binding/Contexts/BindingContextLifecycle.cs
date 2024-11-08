using DH.UIFramework.ViewModels;
using UnityEngine;

namespace DH.UIFramework.Contexts
{
    public sealed class BindingContextLifecycle : MonoBehaviour
    {
        #if UNITY_EDITOR
        public long instanceId;
        #endif
        private IBindingContext bindingContext;
        public IBindingContext BindingContext
        {
            get { return this.bindingContext; }
            set
            {
                if (this.bindingContext == value)
                    return;

                if (this.bindingContext != null)
                    this.bindingContext.Dispose();

                this.bindingContext = value;
            }
        }

        public object DataContext
        {
            get => this.bindingContext.DataContext;
            set
            {
                this.bindingContext.DataContext = value;
 #if UNITY_EDITOR
                if (value == null)
                {
                    instanceId = -1;
                    return;
                }
                instanceId = value.GetHashCode();
#endif
            }
        }

        private void Update()
        {
            if (bindingContext is IUnityLifeCycle unitySupport)
            {
                unitySupport.Update();
            }
        }

        private void LateUpdate()
        {
            if (bindingContext is IUnityLifeCycle unitySupport)
            {
                unitySupport.LateUpdate();
            }
        }

        private void OnDestroy()
        {
            if (this.bindingContext != null)
            {
                this.bindingContext.Dispose();
                this.bindingContext = null;
            }
        }
    }
}
