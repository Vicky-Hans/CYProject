using System;
using Cysharp.Threading.Tasks;

namespace DH.UIFramework
{
    /// <summary>
    /// 用于列表渲染的View
    /// </summary>
    public class BaseItemView : BaseView
    {
        protected void Start()
        {
            CreateWrap().Forget();
        }

        protected async UniTaskVoid CreateWrap()
        {
            await Create();
        }

        protected void OnDestroy()
        {
            Release();
        }
    }
}