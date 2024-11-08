using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class ViewModelBase
    {
        public static void BindCollection<TSource, TTarget>(List<TSource> source, List<TTarget> target, Func<TSource, TTarget> func)
        {

        }

        public static void BindCollection<TKey, TSource, TTarget>(Dictionary<TKey, TSource> source, Dictionary<TKey, TTarget> target, Func<TSource, TTarget> func)
        {

        }

        protected static void BindCollection<TKey, TSource, TTarget>(Dictionary<TKey, TSource> source,
            List<TTarget> target,
            Func<TKey, TSource, TTarget> func,
            Func<TKey, TSource, TTarget, bool> predicate)
        {
        }
    }
}
