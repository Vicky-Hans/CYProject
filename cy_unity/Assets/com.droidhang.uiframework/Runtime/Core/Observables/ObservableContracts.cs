using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DH.UIFramework.Observables
{
    public interface INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }

    public interface INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }

    public interface IObservableProperty
    {
        event EventHandler ValueChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        Type Type { get; }

        object Value { get; set; }
    }

    public interface IObservableProperty<T> : IObservableProperty
    {
        new T Value { get; set; }
    }

    /// <summary>
    /// 1.必须绑定为OnWayToSource
    /// 2.最好写在绑定Collection之前，可以减少一次赋值操作
    /// 3.若绑定的对象为Dictionary，则Filter Comparer传入的参数都是KeyValuePair，需要注意类型转换，否则无法得到正确的结果
    /// </summary>
    public interface ICollectionView
    {
        /// <summary>
        /// 任何可能影响排序或者过滤地方发生变化都需要调用Refresh
        /// </summary>
        void Refresh();

        /// <summary>
        /// 用于设置过滤方法，返回false表示该元素会被过滤掉
        /// </summary>
        Predicate<object> Filter { set; }

        /// <summary>
        /// 用于判断是否显示对应的item，返回true才显示出来，否则不显示（主要适用于tabgroup和对应scrollview关联，使用单个scrollview的时候用上面的Filter）
        /// </summary>
        Predicate<object> CellItemShowPredicate { set; get; }

        /// <summary>
        /// 排序方式
        /// </summary>
        IComparer Comparer { set; }

        object GetItem(object source, bool previous);
    }

    /// <summary>
    /// 1.必须绑定为OnWayToSource
    /// 2.最好写在绑定Collection之前，可以减少一次赋值操作
    /// 3.若绑定的对象为Dictionary，则Filter Comparer传入的参数都是KeyValuePair，需要注意类型转换，否则无法得到正确的结果
    /// </summary>
    public interface IGroupedCollectionView : ICollectionView
    {
        /// <summary>
        /// 分组的key的选择回调
        /// </summary>
        Func<object, object> KeySelector { get; set; }

        /// <summary>
        /// 分组的key排序
        /// </summary>
        IComparer KeyComparer { get; set; }
    }
}
