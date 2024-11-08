using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DH.Asset;
using UnityEngine;

namespace DH.UIFramework
{
    public class SubViewBinder : MonoBehaviour
    {
        public Transform parentNode;
        private string prefabPath;
        private object source;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private BaseView item;
        private bool loading;
        
        public string PrefabPath
        {
            get => prefabPath;
            set
            {
                if (prefabPath != null && prefabPath == value)
                {
                    return;
                }

                prefabPath = value;

                if (loading)
                {
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                }
                CreateItem().Forget();
            }
        }
        
        public object Source
        {
            get => source;
            set
            {
                source = value;
                if (item)
                {
                    item.SetDataContext(source);
                }
            }
        }

        private async UniTaskVoid CreateItem()
        {
            if (item)
            {
                item.SetDataContext(null);
                item.Release();
                item = null;
            }

            loading = true;
            var instance = await AssetsManager.InstantiateWithParentAsync(prefabPath, parentNode, false, cts.Token);
            loading = false;
            if (!instance)
            {
                return;
            }

            item = instance.GetComponent<BaseView>();
            if (source != null)
            {
                item.SetDataContext(source);
            }

            if (item is not BaseItemView)
            {
                item.Create();
            }
        }
        
        private void OnDestroy()
        {
            if (item)
            {
                item.SetDataContext(null);
                item.Release();
            }
            cts.Cancel();
        }
    }
}