using System;
using System.Collections.Specialized;
using DH.UIFramework.Observables;

public class ObservableListLink<T, TSource>
{
    private ObservableList<T> vmList; //vm里list
    private ObservableList<TSource> sourceList; //dataCenter里被关联的list

    private Func<TSource, T> creatorFuc;
    
    public ObservableListLink(ObservableList<T>vmList, ObservableList<TSource> sourceList, Func<TSource, T> creator)
    {
        this.vmList = vmList;
        this.sourceList = sourceList;
        creatorFuc = creator;
        
        this.sourceList.CollectionChanged += OnCollectionChanged;
            
        foreach (var item in sourceList)
        {
            var itemData = creator(item);

            vmList.Add(itemData);
        }
    }

    public void Dispose()
    {
        if (this.sourceList != null)
        {
            this.sourceList.CollectionChanged -= OnCollectionChanged;
        }
    }
        
    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                ObservableList<T> tmpList = new();

                foreach (var item in eventArgs.NewItems)
                {
                    var itemData = creatorFuc((TSource)item);

                    tmpList.Add(itemData);
                }

                vmList.InsertRange(eventArgs.NewStartingIndex, tmpList);
            }
                break;
            case NotifyCollectionChangedAction.Remove:
                vmList.RemoveAt(eventArgs.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
            {
                var itemData = creatorFuc((TSource)eventArgs.NewItems[0]);
                vmList[eventArgs.OldStartingIndex] = itemData;
            }
                break;
            case NotifyCollectionChangedAction.Reset:
                vmList.Clear();
                break;
            case NotifyCollectionChangedAction.Move:
                vmList.Move(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex);
                break;
        }
    }
}
