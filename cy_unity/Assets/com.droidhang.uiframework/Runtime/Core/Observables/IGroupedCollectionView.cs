using System;
using System.Collections;

namespace DH.UIFramework.Observables
{
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