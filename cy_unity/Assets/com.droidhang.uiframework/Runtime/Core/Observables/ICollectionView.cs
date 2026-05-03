using System;
using System.Collections;

namespace DH.UIFramework.Observables
{
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
}