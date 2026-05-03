using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DH.UIFramework.Observables;

public class ObservableDictionary2DictionaryLink<TKey, TValue, TSourceValue>
{
    private ObservableDictionary<TKey, TValue> vmDictionary; //vm里list
    private ObservableDictionary<TKey, TSourceValue> sourceDictionary; //dataCenter里被关联的Dictionary
    private Func<TSourceValue, TValue> creatorFuc;

    public ObservableDictionary2DictionaryLink(ObservableDictionary<TKey, TValue> vmDic,
        ObservableDictionary<TKey, TSourceValue> sourceDic,
        Func<TSourceValue, TValue> creator)
    {
        vmDictionary = vmDic;
        sourceDictionary = sourceDic;
        creatorFuc = creator;

        foreach (var item in sourceDic)
        {
            var itemData = creatorFuc(item.Value);
            var key = item.Key;

            if (vmDictionary.ContainsKey(key)) continue;

            vmDictionary.Add(key, itemData);
        }

        sourceDictionary.CollectionChanged += OnCollectionChanged;
    }

    public void Dispose()
    {
        if (sourceDictionary != null) sourceDictionary.CollectionChanged -= OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        var newItems = eventArgs.NewItems;
        var oldItems = eventArgs.OldItems;

        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddNewItems(newItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveOldItems(oldItems);
                break;
            case NotifyCollectionChangedAction.Replace:
                RemoveOldItems(oldItems);
                AddNewItems(newItems);
                break;
            case NotifyCollectionChangedAction.Reset:
                vmDictionary.Clear();
                break;
        }
    }

    private void RemoveOldItems(IList oldItems)
    {
        foreach (var item in (IList<KeyValuePair<TKey, TSourceValue>>)oldItems) vmDictionary.Remove(item.Key);
    }

    private void AddNewItems(IList newItems)
    {
        foreach (var item in (IList<KeyValuePair<TKey, TSourceValue>>)newItems)
        {
            var itemData = creatorFuc(item.Value);
            var key = item.Key;

            if (vmDictionary.ContainsKey(key)) continue;

            vmDictionary.Add(key, itemData);
        }
    }
}