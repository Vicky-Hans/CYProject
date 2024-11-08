using System;
using System.Collections.Generic;

namespace DH.UIFramework.Contexts
{
    public interface IUnityLifeCycle
    {
        void Update();
        void LateUpdate();
    }
}