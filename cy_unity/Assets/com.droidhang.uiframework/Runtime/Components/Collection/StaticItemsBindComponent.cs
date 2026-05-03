using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.UIFramework
{
    public class StaticItemsBindComponent : IndexedEnumerableCollectionBase
    {
        protected override bool needPrefab => false;
        
        protected override void RefreshView()
        {
            var pool = ListPool<BaseView>.Get();
            try
            {
                foreach (Transform child in transform)
                {
                    var view = child.GetComponent<BaseView>();
                    if (!view)
                    {
                        continue;
                    }
                    pool.Add(view);
                }

                int index = 0;
                foreach (var node in pool)
                {
                    if (node is IViewKey keyNode)
                    {
                        var dataContext = GetValue(keyNode.Key);
                        node.SetDataContext(dataContext);
                    }
                    else if (Collection is IList list)
                    {
                        if (index >= list.Count)
                        {
                            node.gameObject.SetActive(false);
                        }
                        else
                        {
                            node.gameObject.SetActive(true);
                            node.SetDataContext(list[index]);
                        }
                    }

                    index++;
                }
            }
            finally
            {
                ListPool<BaseView>.Release(pool);
            }
        }
    }
}
