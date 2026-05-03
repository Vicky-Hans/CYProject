using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DH.UIFramework.Observables;

public class ObservableList2DictionaryLink<T, TSourceKey, TSourceValue>
{
    private ObservableList<T> vmList; //vm里list
    private ObservableDictionary<TSourceKey, TSourceValue> sourceDictionary; //dataCenter里被关联的Dictionary
    private Func<TSourceValue, T> creatorFuc;
    private readonly Dictionary<TSourceValue, T> src2VmItemDic = new ();

    public ObservableList2DictionaryLink(ObservableList<T>vmList, ObservableDictionary<TSourceKey, TSourceValue> sourceDic, 
        Func<TSourceValue, T> creator)
    {
        this.vmList = vmList;
        this.sourceDictionary = sourceDic;
        creatorFuc = creator;
        
        AddNewItems(sourceDictionary.Values.ToList());
        this.sourceDictionary.CollectionChanged += OnCollectionChanged;
    }

    public void Dispose()
    {
        if (this.sourceDictionary != null)
        {
            this.sourceDictionary.CollectionChanged -= OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        var newItems = eventArgs.NewItems;
        var oldItems = eventArgs.OldItems;
        
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddNewItems((IList<TSourceValue>)newItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveOldItems((IList<TSourceValue>)oldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                RemoveOldItems((IList<TSourceValue>)oldItems);
                AddNewItems((IList<TSourceValue>)newItems);
                break;
            case NotifyCollectionChangedAction.Reset:
                vmList.Clear();
                src2VmItemDic.Clear();
                break;
        }
    }

    private void RemoveOldItems(IList<TSourceValue> oldItems)
    {
        foreach (var item in oldItems)
        {
            if (!src2VmItemDic.TryGetValue(item, out var linkItem)) continue;
            vmList.Remove(linkItem);
            src2VmItemDic.Remove(item);
        }
    }

    private void AddNewItems(IList<TSourceValue> newItems)
    {
        ObservableList<T> tmpList = new();

        foreach (var item in newItems)
        {
            var itemData = creatorFuc(item);
            tmpList.Add(itemData);
            src2VmItemDic.Add(item, itemData);
        }

        vmList.AddRange(tmpList);
    }
}
