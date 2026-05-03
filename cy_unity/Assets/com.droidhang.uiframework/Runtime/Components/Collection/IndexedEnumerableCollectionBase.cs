using System;
using System.Collections;
using System.Collections.Specialized;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;
using INotifyCollectionChanged = DH.UIFramework.Observables.INotifyCollectionChanged;

public class IndexedEnumerableCollectionBase : MonoBehaviour, ICollectionView
{
    public Func<object, object> BindDictionaryGetValueFunc;
    public event Action OnAssetLoaded;
    public bool AssetReady => prefab;
    
    public Predicate<object> CellItemShowPredicate { get; set; }

    IComparer ICollectionView.Comparer
    {
        set
        {
            if (Equals(comparer, value)) return;
            comparer = value;
            if (comparer == null) return;
            if (indexedEnumerable == null) return;

            indexedEnumerable.Comparer = comparer;
        }
    }
    
    Predicate<object> ICollectionView.Filter
    {
        set
        {
            if (Equals(filter, value)) return;

            filter = value;
            if (filter == null) return;

            if (indexedEnumerable == null) return;

            indexedEnumerable.FilterCallback = filter;
        }
    }

    public ICollection Collection
    {
        get => indexedEnumerable.Collection;
        set
        {
            if (indexedEnumerable != null && Equals(indexedEnumerable.Collection, value))
                return;

            var collection = indexedEnumerable?.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged -= OnCollectionChanged;

            bool dirtyView = indexedEnumerable != null;
            if (indexedEnumerable != null)
            {
                indexedEnumerable.Invalidate();
            }

            indexedEnumerable = new IndexedEnumerable(value, filter, comparer);

            if (dirtyView)
            {
                dirty = true;
            }
            else
            {
                OnPrefabsPrepared();
            }

            collection = indexedEnumerable.Collection as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += OnCollectionChanged;
        }
    }

    public string PrefabPath
    {
        get => prefabPath;
        set
        {
            if (prefabPath == value) return;

            prefabPath = value;
            if (string.IsNullOrEmpty(prefabPath) && prefab != null)
            {
                AssetsManager.Release(prefab);
                prefab = null;
            }
            else
            {
                LoadPrefab().Forget();
            }
        }
    }
    
    protected virtual bool needPrefab => true;
    protected IndexedEnumerable indexedEnumerable;
    protected GameObject prefab;
    
    private string prefabPath;
    private Predicate<object> filter;
    private IComparer comparer;
    private bool dirty;

    public object GetItem(object source, bool previous)
    {
        return indexedEnumerable?.GetItem(source, previous) ?? null;
    }
    
    public void Refresh()
    {
        if (prefab == null) return;
        if (indexedEnumerable == null) return;

        indexedEnumerable.InvalidateEnumerator();
        dirty = true;
    }

    protected virtual void OnPrefabsPrepared()
    {
        if (indexedEnumerable == null) return;
        if (needPrefab && !prefab) return;

        RefreshView();
    }

    protected virtual void Awake()
    {
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void LateUpdate()
    {
        if (dirty && (!needPrefab || prefab))
        {
            dirty = false;
            RefreshView();
        }
    }

    protected object GetValue(object obj)
    {
        if (BindDictionaryGetValueFunc != null) return BindDictionaryGetValueFunc.Invoke(obj);
        return null;
    }

    protected virtual void RefreshView()
    {
        
    }

    protected virtual void OnDestroy()
    {
        indexedEnumerable?.Invalidate();
        indexedEnumerable = null;
        PrefabPath = null;
    }

    private async UniTaskVoid LoadPrefab()
    {
        prefab = await AssetsManager.LoadAssetAsync<GameObject>(prefabPath);
        OnPrefabsPrepared();
            
        OnAssetLoaded?.Invoke();
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        indexedEnumerable?.InvalidateEnumerator();
        dirty = true;
    }
}
